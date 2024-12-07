using GemBardPT.Models;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços à DI container
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();
builder.Services.Configure<GeminiApiSettings>(builder.Configuration.GetSection("GeminiApiSettings"));

var geminiApiKey = Environment.GetEnvironmentVariable("AIzaSyBiW1wMoiue4ucA_ga5RoLdaDaYZZqSZqM");

builder.Services.AddSingleton(new GeminiApiSettings { ApiKey = geminiApiKey });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Configuração de rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
