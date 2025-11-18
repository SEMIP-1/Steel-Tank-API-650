using Microsoft.OpenApi.Models;
using SteelTankAPI650.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<ITankDesignService, TankDesignService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Steel Tank API 650", 
        Version = "v1" 
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Steel Tank API 650 v1");
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
