using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//  Add Swagger with metadata
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //  Enable Swagger middleware
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Audio Interview API V1");
        c.RoutePrefix = string.Empty; 
    });
}

app.UseHttpsRedirection(); // This redirects HTTP to HTTPS

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
