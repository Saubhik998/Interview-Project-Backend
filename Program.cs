using Microsoft.OpenApi.Models;
using AudioInterviewer.API.Services; // InterviewService
using AudioInterviewer.API.Data;     // MongoDBContext
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Load MongoDB settings from appsettings.json
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

// Register services
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<InterviewService>();
builder.Services.AddControllers();

// Swagger setup
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

//  Moved CORS setup BEFORE builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// Build app AFTER all services are registered
var app = builder.Build();

// Swagger UI (for development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Audio Interview API V1");
        c.RoutePrefix = string.Empty;
    });
}

// Middleware
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Run app
app.Run();
