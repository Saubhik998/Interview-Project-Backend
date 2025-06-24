using Microsoft.OpenApi.Models;
using AudioInterviewer.API.Services;           // InterviewService
using AudioInterviewer.API.Data;               // MongoDBContext

using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Load MongoDB settings from appsettings.json
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

// Register MongoDBContext for DI
builder.Services.AddSingleton<MongoDbContext>();

// Register InterviewService for in-memory/business logic
builder.Services.AddSingleton<InterviewService>();

// Register Controllers
builder.Services.AddControllers();

// Add Swagger (API documentation)
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

// Swagger UI configuration (Dev only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Audio Interview API V1");
        c.RoutePrefix = string.Empty; // Swagger UI at root
    });
}


app.UseSwagger();
app.UseSwaggerUI();

// HTTPS redirection
app.UseHttpsRedirection();

// AuthZ (optional - used for future JWT tokens or identity)
app.UseAuthorization();

// Map controller routes
app.MapControllers();

// Start the app
app.Run();
