using CaptureSys.ScriptExecutionService.Application.Interfaces;
using CaptureSys.ScriptExecutionService.Application.Services;
using CaptureSys.ScriptExecutionService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Services
builder.Services.AddScoped<IScriptExecutionService, ScriptExecutionApplicationService>();
builder.Services.AddScoped<IScriptRunner, ScriptRunner>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("ScriptExecutionService starting up...");
Console.WriteLine("Service available at: http://localhost:5010");

app.Run("http://localhost:5010");

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
