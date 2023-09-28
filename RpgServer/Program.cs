using Microsoft.Extensions.Configuration.Yaml;
using RpgServer.Configs;
using RpgServer.Interfaces;
using RpgServer.Serializers;
using RpgServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddYamlFile("AppSettings.yaml");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<ICacheSerializer, JsonCacheSerializer>();
builder.Services.AddSingleton<ContextConfig>();
builder.Services.AddScoped<ContextService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

//app.MapGet("/", () => Results.Ok("Hello"));

app.Run();
