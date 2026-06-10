using PnlFileIngester;
using PnlFileIngester.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .Configure<PnlFileWorkerOptions>(builder.Configuration.GetSection(PnlFileWorkerOptions.Section))
    .AddSingleton<PnlFileReader>()
    .AddHostedService<PnlFileWorkerService>();

var host = builder.Build();
await host.RunAsync();
