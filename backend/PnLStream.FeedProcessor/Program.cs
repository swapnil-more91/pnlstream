using PnLStream.FeedProcessor.Models;
using PnLStream.FeedProcessor.Workers;
using PnLStream.Persistence.DependencyInjection;
using PnlStream.ValidationEngine;
using PnLStream.FeedProcessor.Services;

var builder = Host.CreateApplicationBuilder(args);

var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
              ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
              ?? builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true);

builder.Services.Configure<FeedProcessorOptions>(builder.Configuration.GetSection("FeedProcessor"));
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<SignalROptions>(builder.Configuration.GetSection("SignalR"));

builder.Services.AddSingleton<IKafkaConsumerService,KafkaConsumerService>();
builder.Services.AddSingleton<IRealtimeNotifier, SignalRNotifier>();
builder.Services.AddValidationEngine();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddHostedService<FeedProcessorWorker>();

var app = builder.Build();

app.Run();
