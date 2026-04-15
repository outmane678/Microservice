using EmployeService.Client;
using EmployeService.Data;
using Microsoft.EntityFrameworkCore;
using EmployeService.Services.Implementations; 
using EmployeService.Services.Interfaces;
using Refit;

var builder = WebApplication.CreateBuilder(args);
// On utilise directement builder.Configuration.AsEnumerable()
foreach (var (key, value) in builder.Configuration.AsEnumerable())
{
    if (value is string str && str.StartsWith("${") && str.EndsWith("}"))
    {
        var envVarName = str[2..^1]; // Enlève ${ et }
        var envValue = Environment.GetEnvironmentVariable(envVarName);
        if (!string.IsNullOrEmpty(envValue))
        {
            // CORRECTION: Accès direct à builder.Configuration
            builder.Configuration[key] = envValue;
            Console.WriteLine($" {key} = {envValue.Substring(0, Math.Min(3, envValue.Length))}***");
        }
        else
        {
            Console.WriteLine($" Variable d'environnement {envVarName} non trouvée pour {key}");
        }
    }
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")));

builder.Services.AddScoped<IEmployeeService, EmployeeServiceImp>();
builder.Services.AddScoped<IEmailSender, EmailService>();

// Refit Client - CORRIGÉ
builder.Services.AddRefitClient<IDepartmentAPI>()
    .ConfigureHttpClient(c =>
    {
        var deptUrl = builder.Configuration["DepartmentServiceUrl"] ?? "http://localhost:5175";
        c.BaseAddress = new Uri(deptUrl);
        c.Timeout = TimeSpan.FromSeconds(30);
        c.DefaultRequestHeaders.Add("Accept", "application/json");
    });


var app = builder.Build();

var pathBase = app.Configuration["ASPNETCORE_PATHBASE"];
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
}

var enableSwagger = app.Environment.IsDevelopment()
    || app.Configuration.GetValue<bool>("EnableSwagger");
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.MapControllers();

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
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}