namespace TrackNest.Worker;

public class RabbitMqWorkerSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "tracknest.expenses";
    public string QueueName { get; set; } = "tracknest.expenses.sync";
}