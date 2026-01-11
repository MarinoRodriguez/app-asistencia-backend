using AssistantApp.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// --- CONFIGURACIÓN BASE DE DATOS (SQLite) ---
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=./DefaultData/asistencia.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
// --------------------------------------------

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// ... código anterior (app.MapControllers, etc)

// --- ZONA DE SEEDING ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    
    // Ejecuta el seeder
    try 
    {
        DataSeed.SeedData(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al insertar datos de prueba.");
    }
}
// -----------------------

app.Run();
app.Run();
