using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AssistantApp.Client;
using AssistantApp.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configuración del HttpClient
var apiUrl = "https://localhost:7224/"; 
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

// Servicios del Cliente
builder.Services.AddScoped<ClientPersonService>();
builder.Services.AddScoped<ClientEventService>();
builder.Services.AddScoped<ClientAttendanceService>();
builder.Services.AddScoped<ClientGroupService>();

await builder.Build().RunAsync();