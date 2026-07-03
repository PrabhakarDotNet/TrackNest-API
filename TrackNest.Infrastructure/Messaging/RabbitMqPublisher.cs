using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TrackNest.Application.Interfaces;

namespace TrackNest.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public RabbitMqPublisher(IOptions<RabbitMqSettings> options)
    {
        _settings = options.Value;
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) return;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_channel is not null) return; // double-check after acquiring lock

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.ExchangeDeclareAsync(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                cancellationToken: cancellationToken);
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await _channel!.BasicPublishAsync(
            exchange: _settings.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.CloseAsync();
        if (_connection is not null) await _connection.CloseAsync();
    }
}