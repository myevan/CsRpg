using Microsoft.Extensions.Configuration.Yaml;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddYamlFile("AppSettings.yaml");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok("Hello"));

app.Run();
