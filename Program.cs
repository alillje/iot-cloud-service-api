using System.Text.Json;
using iot_cloud_service_api.Interfaces;
using iot_cloud_service_api.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddHttpClient(); // This line registers IHttpClientFactory which provides HttpClient instances

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    // .AddSingleton<IElasticClient>(elasticClient)
    .AddScoped<IAdafruitService, AdafruitService>();

// Extract API keys from config to hashset for faster lookups
var allowedApiKey = builder.Configuration["ApiKey"];

builder.Services.AddCors(); // Add this line without configuration

// Only allow requests from the specified client
var allowedOrigins = builder.Configuration["AllowedClient"];

var app = builder.Build();

app.UseCors(builder => builder
    .WithOrigins(allowedOrigins)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
    
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} else {
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();

// Check API-key header and return 401 if invalid
app.Use(async (context, next) =>
{
    var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();

    if (string.IsNullOrEmpty(apiKey) || !allowedApiKey.Equals(apiKey))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Invalid API Key" }));
    }
    else
    {
        await next();
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} else {
    app.UseHttpsRedirection();
}

// app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
