using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TrackNest.Application.Events;


namespace TrackNest.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMqWorkerSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private IConnection? _connection;
    private IChannel? _channel;

    public Worker(ILogger<Worker> logger, IOptions<RabbitMqWorkerSettings> settings, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(_settings.QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(_settings.QueueName, _settings.ExchangeName, routingKey: "expense.changed", cancellationToken: cancellationToken);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<ExpenseChangedEvent>(json);

                if (evt is not null)
                {
                    _logger.LogInformation("Syncing ChromaDB for user {UserId} ({ChangeType})", evt.UserId, evt.ChangeType);

                    var client = _httpClientFactory.CreateClient("FastApi");
                    var response = await client.PostAsJsonAsync("/ingest", new { user_id = evt.UserId }, stoppingToken);

                    if (!response.IsSuccessStatusCode)
                        _logger.LogWarning("FastAPI /ingest failed with {StatusCode}", response.StatusCode);
                }

                await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message, nack without requeue");
                await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await _channel!.BasicConsumeAsync(_settings.QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        // Keep running until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { });
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.CloseAsync(cancellationToken);
        if (_connection is not null) await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}