using Backend.Services;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Threading;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpClient(); 

// Configurar Entity Framework com SQLite
// Usar caminho absoluto para evitar problemas de permissão
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "fortnite.db");
var dbDirectory = Directory.GetCurrentDirectory();

// Garantir que o diretório existe e tem permissões
if (!Directory.Exists(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}

// Verificar e remover arquivos temporários do SQLite que podem estar bloqueando
try
{
    var dbShm = dbPath + "-shm";
    var dbWal = dbPath + "-wal";
    
    if (File.Exists(dbShm))
    {
        File.Delete(dbShm);
        Console.WriteLine($"Arquivo temporário {dbShm} removido");
    }
    
    if (File.Exists(dbWal))
    {
        File.Delete(dbWal);
        Console.WriteLine($"Arquivo temporário {dbWal} removido");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Aviso ao limpar arquivos temporários: {ex.Message}");
}

Console.WriteLine($"Banco de dados será criado em: {dbPath}");
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
            "http://localhost:3000"
        };
        
        // Adicionar origem do Vercel se estiver configurada
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            allowedOrigins.Add(frontendUrl);
        }
        
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("X-User-Id"); // Expor o header customizado
    });
});

var app = builder.Build();

// Criar banco de dados se não existir
using (var scope = app.Services.CreateScope())
{
    // Tentar criar o banco com retry
    int maxRetries = 3;
    int retryDelay = 1000; // 1 segundo
    bool dbCreated = false;
    
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                dbContext.Database.EnsureCreated();
                Console.WriteLine($"Banco de dados criado/verificado em: {dbPath}");
                dbCreated = true;
                break;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                Console.WriteLine($"Tentativa {i + 1} falhou: {ex.Message}. Tentando novamente em {retryDelay}ms...");
                Thread.Sleep(retryDelay);
                
                // Tentar limpar arquivos temporários novamente
                try
                {
                    var dbShm = dbPath + "-shm";
                    var dbWal = dbPath + "-wal";
                    if (File.Exists(dbShm)) File.Delete(dbShm);
                    if (File.Exists(dbWal)) File.Delete(dbWal);
                }
                catch { }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao criar banco de dados após {maxRetries} tentativas: {ex.Message}");
        Console.WriteLine($"Caminho tentado: {dbPath}");
        Console.WriteLine($"Diretório atual: {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"Diretório existe: {Directory.Exists(Directory.GetCurrentDirectory())}");
        Console.WriteLine($"Permissão de escrita: {IsDirectoryWritable(Directory.GetCurrentDirectory())}");
        // Não lançar exceção para permitir que a aplicação inicie mesmo com erro no banco
        // O banco será criado na primeira requisição que precisar dele
    }
    
    if (!dbCreated)
    {
        Console.WriteLine($"Aviso: Banco de dados não foi criado após {maxRetries} tentativas. A aplicação continuará, mas o banco será criado na primeira requisição.");
    }
}

// Função auxiliar para verificar permissão de escrita
static bool IsDirectoryWritable(string dirPath)
{
    try
    {
        var testFile = Path.Combine(dirPath, $"test_{Guid.NewGuid()}.tmp");
        File.WriteAllText(testFile, "test");
        File.Delete(testFile);
        return true;
    }
    catch
    {
        return false;
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
