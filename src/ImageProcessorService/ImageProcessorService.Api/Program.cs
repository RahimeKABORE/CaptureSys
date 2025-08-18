using CaptureSys.ImageProcessorService.Application.Interfaces;
using CaptureSys.ImageProcessorService.Application.Services;
using CaptureSys.ImageProcessorService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Services
builder.Services.AddScoped<IImageProcessingService, ImageProcessingApplicationService>();
builder.Services.AddScoped<IImageProcessor, ImageProcessor>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("ImageProcessorService starting up...");
Console.WriteLine("Service available at: http://localhost:5008");

app.Run("http://localhost:5008");