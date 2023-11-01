using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;
using WeatherAPI.Data;
using WeatherAPI.Services;
using WeatherAPI.Services.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<WeatherAPIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WeatherAPIContext") ?? throw new InvalidOperationException("Connection string 'WeatherAPIContext' not found.")));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    {
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Weather API", Version = "v1" });
        }

        // Include the XML comments file
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    }
);
//Add Console Logger
builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
