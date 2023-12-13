using Microsoft.Extensions.Configuration;
using Nest;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Register Elasticsearch
var elasticsearchUri = builder.Configuration.GetConnectionString("ElasticsearchConnection");

var settings = new ConnectionSettings(new Uri(elasticsearchUri))
    .DefaultIndex("country"); 

var client = new ElasticClient(settings);
builder.Services.AddLogging();
builder.Services.AddSingleton<IElasticClient>(client);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();
