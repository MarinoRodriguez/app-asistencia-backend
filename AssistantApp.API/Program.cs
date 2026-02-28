using AssistantApp.API.Data;
using AssistantApp.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// --- CONFIGURACIÓN BASE DE DATOS (SQLite) ---
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=./DefaultData/asistencia.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
// --------------------------------------------

// --- SERVICIOS DE NEGOCIO (Scoped) ---
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<GroupService>();
// -------------------------------------

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});
// ------------

// Habilitar Controladores y configurar JSON para evitar ciclos en relaciones N:M
builder.Services.AddControllers().AddJsonOptions(x =>
   x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!string.Equals(Environment.GetEnvironmentVariable("DISABLE_HTTPS_REDIRECT"), "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}

// IMPORTANTE: UseCors debe ir antes de UseAuthorization
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

//// --- ZONA DE SEEDING ---
// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     var context = services.GetRequiredService<AppDbContext>();
//     
//     // Ejecuta el seeder
//     try 
//     {
//         // Asegura que la BD exista
//         context.Database.EnsureCreated();
//         DataSeed.SeedData(context);
//     }
//     catch (Exception ex)
//     {
//         var logger = services.GetRequiredService<ILogger<Program>>();
//         logger.LogError(ex, "Ocurrió un error al insertar datos de prueba.");
//     }
// }
// // -----------------------

app.Run();
