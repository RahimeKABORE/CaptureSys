using CaptureSys.Worker.Orchestrator.Application.Interfaces;
using CaptureSys.Worker.Orchestrator.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register services
builder.Services.AddScoped<IWorkflowOrchestrator, WorkflowOrchestrator>();
builder.Services.AddScoped<IServiceCommunicator, ServiceCommunicator>();

var app = builder.Build();

app.MapControllers();

Console.WriteLine("CaptureSys Worker Orchestrator starting up...");
Console.WriteLine("Service running on: http://localhost:5007");

app.Run("http://localhost:5007");
