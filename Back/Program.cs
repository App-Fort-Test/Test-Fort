using Backend.Services;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Threading;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Permitir roteamento case-insensitive
        options.SuppressModelStateInvalidFilter = true;
    });

// Configurar roteamento case-insensitive
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddHttpClient(); 

// Função auxiliar para verificar permissão de escrita (definida antes de usar)
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

// Configurar Entity Framework com SQLite
// No Railway, usar diretório persistente ou /tmp se disponível
var dbDirectory = Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH");
if (string.IsNullOrEmpty(dbDirectory))
{
    // Tentar usar /tmp no Railway ou diretório atual
    var tmpDir = "/tmp";
    if (Directory.Exists(tmpDir) && IsDirectoryWritable(tmpDir))
    {
        dbDirectory = tmpDir;
    }
    else
    {
        dbDirectory = Directory.GetCurrentDirectory();
    }
}

// Garantir que o diretório existe e tem permissões
if (!Directory.Exists(dbDirectory))
{
    try
    {
        Directory.CreateDirectory(dbDirectory);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Aviso: Não foi possível criar diretório {dbDirectory}: {ex.Message}");
        dbDirectory = Directory.GetCurrentDirectory();
    }
}

var dbPath = Path.Combine(dbDirectory, "fortnite.db");
Console.WriteLine($"Diretório do banco: {dbDirectory}");
Console.WriteLine($"Caminho completo do banco: {dbPath}");

// Função para limpar arquivos temporários do SQLite
static void CleanSqliteTempFiles(string dbPath)
{
    var tempFiles = new[] { dbPath + "-shm", dbPath + "-wal", dbPath + "-journal" };
    
    foreach (var tempFile in tempFiles)
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                if (File.Exists(tempFile))
                {
                    // Tentar remover atributo somente leitura se existir
                    var fileInfo = new FileInfo(tempFile);
                    if (fileInfo.Exists)
                    {
                        fileInfo.IsReadOnly = false;
                    }
                    
                    File.Delete(tempFile);
                    Console.WriteLine($"Arquivo temporário {tempFile} removido");
                    break;
                }
            }
            catch (Exception ex) when (attempt < 2)
            {
                Console.WriteLine($"Tentativa {attempt + 1} falhou ao remover {tempFile}: {ex.Message}");
                Thread.Sleep(500); // Aguardar 500ms antes de tentar novamente
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Não foi possível remover {tempFile}: {ex.Message}");
            }
        }
    }
}

// Limpar arquivos temporários antes de configurar o banco
CleanSqliteTempFiles(dbPath);

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
        
        // Em produção, permitir qualquer origem (para facilitar debug)
        if (builder.Environment.IsProduction())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .WithExposedHeaders("X-User-Id");
        }
        else
        {
            policy.WithOrigins(allowedOrigins.ToArray())
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .WithExposedHeaders("X-User-Id");
        }
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
                CleanSqliteTempFiles(dbPath);
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// No Railway, não usar HTTPS redirection (usa HTTP)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
