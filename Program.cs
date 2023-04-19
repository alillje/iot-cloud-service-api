using Elasticsearch.Net;
using Nest;
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
    .AddSingleton<IElasticService, ElasticService>();


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

app.UseAuthorization();

app.MapControllers();

app.Run();
