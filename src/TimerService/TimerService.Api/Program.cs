using CaptureSys.TimerService.Application.Interfaces;
using CaptureSys.TimerService.Application.Services;
using CaptureSys.TimerService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Services (sans Quartz pour l'instant)
builder.Services.AddScoped<ITimerService, TimerApplicationService>();
builder.Services.AddSingleton<IQuartzScheduler, MockQuartzScheduler>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("TimerService starting up...");
Console.WriteLine("Service available at: http://localhost:5011");

app.Run("http://localhost:5011");