using Elasticsearch.Net;
using Nest;
using System.Text.Json;
using iot_cloud_service_api.Interfaces;
using iot_cloud_service_api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionSettings = new ConnectionSettings(new Uri(builder.Configuration["ElasticSearch:Url"]));
var password = builder.Configuration["ElasticSearch:Password"];

connectionSettings.DisableDirectStreaming();
connectionSettings.BasicAuthentication(builder.Configuration["ElasticSearch:User"], builder.Configuration["ElasticSearch:Password"]);
connectionSettings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);

var elasticClient = new ElasticClient(connectionSettings);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddSingleton<IElasticClient>(elasticClient)
    .AddScoped<IElasticService, ElasticService>();

// Extract API keys from config to hashset for faster lookups
var allowedApiKeys = new HashSet<string>(builder.Configuration.GetSection("ApiKeys").Get<string[]>());

// Only allow requests from the specified client
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration["Cors:AllowedClient"];
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithHeaders("Access-Control-Allow-Origin");
        });
});

var app = builder.Build();

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

    if (string.IsNullOrEmpty(apiKey) || !allowedApiKeys.Contains(apiKey))
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

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
