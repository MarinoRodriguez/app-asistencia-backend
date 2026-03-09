using AssistantApp.API.Data;
using AssistantApp.API.Identity;
using AssistantApp.API.Services;
using AssistantApp.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// --- CONFIGURACIÓN BASE DE DATOS (SQLite) ---
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=./DefaultData/asistencia.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
// --------------------------------------------

// --- IDENTITY + AUTH ---
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-change-this-key";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AssistantApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AssistantApp";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.All)
    {
        options.AddPolicy(permission, policy =>
            policy.RequireClaim("permission", permission));
    }
});
// ------------------------

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


//// --- ZONA DE SEEDING ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    
    // Ejecuta el seeder
    try 
    {
        // Asegura que la BD exista
        context.Database.EnsureCreated();
        // DataSeed.SeedData(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al insertar datos de prueba.");
    }
}
// // -----------------------

app.Run();
