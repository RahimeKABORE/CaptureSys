using CaptureSys.Gateway.Auth.Application;
using CaptureSys.Gateway.Auth.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add application layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("CaptureSys Gateway Auth starting up...");
Console.WriteLine("Service running on: http://localhost:5006");
Console.WriteLine("API endpoints available at: http://localhost:5006/api/Auth");

app.Run("http://localhost:5006");