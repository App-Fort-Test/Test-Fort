using Backend.Services;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpClient(); 

// Configurar Entity Framework com SQLite
// Usar caminho absoluto para evitar problemas de permissão
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "fortnite.db");
var dbDirectory = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddHttpClient("FortniteApi", client =>
{
    client.BaseAddress = new Uri("https://fortnite-api.com/v2/");
});

builder.Services.AddScoped<CosmeticsServices>();
builder.Services.AddScoped<CosmeticsNewServices>();
builder.Services.AddScoped<ShopServices>();
builder.Services.AddScoped<CosmeticsEnrichedServices>();
builder.Services.AddScoped<UserInventoryService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = new List<string>
        {
            "http://localhost:5173",
            "http://localhost:5175",
            "http://localhost:5176",
            "http://localhost:3000",
            "http://localhost"
        };
        
        // Adicionar origem do Vercel se estiver configurada
        var vercelUrl = Environment.GetEnvironmentVariable("VERCEL_URL");
        if (!string.IsNullOrEmpty(vercelUrl))
        {
            allowedOrigins.Add($"https://{vercelUrl}");
        }
        
        // Adicionar origem customizada se configurada
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            allowedOrigins.Add(frontendUrl);
        }
        
        // Em produção, permitir qualquer origem (ou configurar domínios específicos)
        if (builder.Environment.IsProduction())
        {
            policy.SetIsOriginAllowed(_ => true); // Permite qualquer origem
        }
        else
        {
            policy.WithOrigins(allowedOrigins.ToArray());
        }
        
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("X-User-Id"); // Expor o header customizado
    });
});

var app = builder.Build();

// Criar banco de dados se não existir
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        Console.WriteLine($"Banco de dados criado/verificado em: {dbPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao criar banco de dados: {ex.Message}");
        Console.WriteLine($"Caminho tentado: {dbPath}");
        Console.WriteLine($"Diretório atual: {Directory.GetCurrentDirectory()}");
        // Não lançar exceção para permitir que a aplicação inicie mesmo com erro no banco
        // O banco será criado na primeira requisição que precisar dele
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

/*var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();*/

app.UseCors();
app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
