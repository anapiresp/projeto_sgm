using SeaRise.Services.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configure logging to console explicitly for clearer diagnostics
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add controllers
builder.Services.AddControllers();

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler("/error");
}

// Map controller routes
app.MapControllers();

// Serve static files from the `Components` folder so the HTML/CSS/JS pages are reachable
var componentsPath = Path.Combine(Directory.GetCurrentDirectory(), "Components");
if (Directory.Exists(componentsPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(componentsPath),
        RequestPath = ""
    });
}

app.UseHttpsRedirection();

// Map root to the login page so http://localhost:PORT/ opens the login/signup flow
app.MapGet("/", async context =>
{
    var file = Path.Combine("Components", "Pages", "SignUpLogin", "login.html");
    if (File.Exists(file))
    {
        await context.Response.SendFileAsync(file);
    }
    else
    {
        // Fallback to main if login not present
        var fallback = Path.Combine("Components", "Pages", "Main", "main.html");
        if (File.Exists(fallback)) await context.Response.SendFileAsync(fallback);
        else context.Response.StatusCode = 404;
    }
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
