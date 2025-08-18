using CaptureSys.AutoLearningService.Application.Interfaces;
using CaptureSys.AutoLearningService.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Services
builder.Services.AddScoped<IAutoLearningService, AutoLearningApplicationService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("AutoLearningService starting up...");
Console.WriteLine("Service available at: http://localhost:5009");

app.Run("http://localhost:5009");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
