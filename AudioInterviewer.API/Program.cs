using Microsoft.OpenApi.Models;
using AudioInterviewer.API.Services;
using AudioInterviewer.API.Data;
using AudioInterviewer.API.Services.External;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using System.IO;

var builder = WebApplication.CreateBuilder(args); // ✅ Let .NET resolve content root

// Load MongoDB settings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

// Bind FastApiClient configuration section to ApiSettings
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("FastApiClient"));

// Register MongoDB context as singleton
builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();

// Register FastApiClient as IApiClient with HttpClient and config, logging injected
builder.Services.AddHttpClient<IApiClient, FastApiClient>();

// Register InterviewService
builder.Services.AddScoped<IInterviewService, InterviewService>();

builder.Services.AddControllers();

// CORS policy – updated
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins(
                            "http://localhost:3000", 
                            "http://audio-interviewer-frontend") // Docker container name
                      .AllowAnyHeader()
                      .AllowAnyMethod());
});

// Add Health Checks with MongoDB
builder.Services.AddHealthChecks()
    .AddMongoDb(
        sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        },
        name: "MongoDB",
        timeout: TimeSpan.FromSeconds(3),
        tags: new[] { "db", "mongo" }
    );

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "AI Audio Interview API",
        Description = "Backend API for voice-based AI interview system",
        Contact = new OpenApiContact
        {
            Name = "Saubhik Dey",
            Email = "saubhikdey1@gmail.com",
            Url = new Uri("https://github.com/Saubhik998/Interview-Project-Backend")
        }
    });
});

var app = builder.Build();

// Swagger in dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Audio Interview API V1");
        c.RoutePrefix = string.Empty;
    });
}

// Serve static files like .webm
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".webm"] = "audio/webm";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.UseHttpsRedirection();

app.MapHealthChecks("/health"); // ✅ Re-enabled for CI testing

app.Run();

public partial class Program { }
