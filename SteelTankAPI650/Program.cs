//---------------------------------------------------------------
#region Using Directives
using SteelTankAPI650.Services.Shell;
using SteelTankAPI650.Services.Config;

#endregion
//---------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Setup Swagger for API documentation
builder.Services.AddSwaggerGen();
// Register design data repository
builder.Services.AddSingleton<IDesignDataRepository, ExcelDesignDataRepository>();
// Register new shell design service
builder.Services.AddScoped<IShellDesignService, ShellDesignService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
