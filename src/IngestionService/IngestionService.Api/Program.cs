using CaptureSys.IngestionService.Application;
using CaptureSys.IngestionService.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuration Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "CaptureSys Ingestion API", 
        Version = "v1",
        Description = "API pour l'ingestion de documents dans CaptureSys"
    });
});

// Add application layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// CORS
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

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CaptureSys Ingestion API v1");
    c.RoutePrefix = "swagger";
    
    // Configuration pour supporter l'ApiGateway
    c.ConfigObject.AdditionalItems["urls"] = new[] {
        new { url = "/swagger/v1/swagger.json", name = "Direct" },
        new { url = "/ingestion-swagger/v1/swagger.json", name = "Via Gateway" }
    };
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Log.Information("CaptureSys Ingestion Service starting up...");
Log.Information("Swagger UI available at: http://localhost:5000/swagger/index.html");

app.Run();

