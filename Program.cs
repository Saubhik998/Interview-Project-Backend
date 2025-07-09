using Microsoft.OpenApi.Models;
using AudioInterviewer.API.Services;         // InterviewService
using AudioInterviewer.API.Data;            // MongoDBContext
using AudioInterviewer.API.Services.External; //  FastApiClient
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Load MongoDB settings from appsettings.json
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

// Register services
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddHttpClient<FastApiClient>();
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

// CORS policy for frontend React app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// Build the app
var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Audio Interview API V1");
        c.RoutePrefix = string.Empty;
    });
}

//Serve static files with .webm MIME mapping
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".webm"] = "audio/webm";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

// Middleware
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Run the app
app.Run();
