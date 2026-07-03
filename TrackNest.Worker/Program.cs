using TrackNest.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqWorkerSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddHttpClient("FastApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["FastApiBaseUrl"]!);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();