using PnLStream.Persistence.Interfaces;
using PnLStream.Persistence.Repositories;
using PnLStream.Persistence.DependencyInjection;
using PnLStream.Reporting.Api.Hubs;
using PnLStream.Reporting.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddScoped<IReportingService,ReportingService>();

builder.Services.AddControllers();

builder.Services.AddSignalR();


builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Angular",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("Angular");

app.MapControllers();

app.MapHub<PnlHub>("/pnlHub");

app.Run();