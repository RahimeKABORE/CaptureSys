using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuration Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Configuration Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Pipeline simplifié
app.UseCors("AllowAll");

// Ocelot middleware
await app.UseOcelot();

Log.Information("CaptureSys API Gateway starting up...");
Log.Information("Gateway available at: http://localhost:5002");
Log.Information("Routes available:");
Log.Information("  POST http://localhost:5002/ingestion/Documents/upload -> IngestionService");
Log.Information("  POST http://localhost:5002/ocr/Ocr/process -> OcrService");
Log.Information("  POST http://localhost:5002/auth/Auth/login -> Gateway.Auth");
Log.Information("  GET http://localhost:5002/export/Export -> ExportService");
Log.Information("  GET http://localhost:5002/ingestion-swagger/index.html -> Swagger");

app.Run();