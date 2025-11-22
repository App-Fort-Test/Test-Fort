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
        
        options.SuppressModelStateInvalidFilter = true;
    });


builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddHttpClient(); 


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


var dbDirectory = Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH");
if (string.IsNullOrEmpty(dbDirectory))
{
    Console.WriteLine("⚠️ AVISO: RAILWAY_VOLUME_MOUNT_PATH não está configurado!");
    Console.WriteLine("⚠️ O banco será criado em /tmp (dados serão perdidos entre rebuilds)");
    Console.WriteLine("💡 Configure um volume persistente no Railway e a variável RAILWAY_VOLUME_MOUNT_PATH=/data");
    
    var tmpDir = "/tmp";
    if (Directory.Exists(tmpDir) && IsDirectoryWritable(tmpDir))
    {
        dbDirectory = tmpDir;
        Console.WriteLine($"⚠️ Usando diretório temporário: {tmpDir}");
    }
    else
    {
        dbDirectory = Directory.GetCurrentDirectory();
        Console.WriteLine($"⚠️ Usando diretório atual: {dbDirectory}");
    }
}
else
{
    Console.WriteLine($"✅ RAILWAY_VOLUME_MOUNT_PATH configurado: {dbDirectory}");
}


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
                Thread.Sleep(500); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Não foi possível remover {tempFile}: {ex.Message}");
            }
        }
    }
}


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
            "http://localhost:3000",
            "https://test-fort-ulwx.vercel.app",
            "https://test-fort-ulwx-git-main-marcelleaps-projects.vercel.app",
            "https://test-fort-ulwx-marcelleaps-projects.vercel.app",
            "https://test-fort-nine.vercel.app"
        };
        

        var vercelPreviewPattern = "https://test-fort-ulwx-*.vercel.app";
 
        

        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            if (!allowedOrigins.Contains(frontendUrl))
            {
                allowedOrigins.Add(frontendUrl);
            }
        }
        
        // Permitir qualquer origem do Vercel que comece com test-fort-ulwx
        // Isso cobre todas as URLs de preview dinâmicas
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin))
                return false;
            
            // Permitir localhost
            if (origin.StartsWith("http://localhost"))
                return true;
            
            // Permitir qualquer URL do Vercel que contenha test-fort-ulwx ou test-fort-nine
            if (origin.Contains("test-fort-ulwx") || origin.Contains("test-fort-nine") || origin.Contains("vercel.app"))
            {
                Console.WriteLine($"✅ Origem permitida (Vercel): {origin}");
                return true;
            }
            
            // Permitir URLs na lista
            return allowedOrigins.Contains(origin);
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .WithExposedHeaders("X-User-Id");
        
        Console.WriteLine($"✅ CORS configurado com {allowedOrigins.Count} origens fixas + wildcard para Vercel:");
        foreach (var origin in allowedOrigins)
        {
            Console.WriteLine($"   - {origin}");
        }
        Console.WriteLine($"   - Qualquer URL contendo 'test-fort-ulwx', 'test-fort-nine' ou 'vercel.app'");
    });
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    int maxRetries = 5;
    int retryDelay = 2000;
    bool dbCreated = false;
    
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        Console.WriteLine($"=== Iniciando criação do banco de dados ===");
        Console.WriteLine($"Caminho do banco: {dbPath}");
        Console.WriteLine($"Diretório: {dbDirectory}");
        Console.WriteLine($"Diretório existe: {Directory.Exists(dbDirectory)}");
        Console.WriteLine($"Permissão de escrita: {IsDirectoryWritable(dbDirectory)}");
        Console.WriteLine($"Ambiente: {app.Environment.EnvironmentName}");
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                Console.WriteLine($"Tentativa {i + 1}/{maxRetries} de criar o banco...");
                
                if (!Directory.Exists(dbDirectory))
                {
                    Console.WriteLine($"Criando diretório: {dbDirectory}");
                    Directory.CreateDirectory(dbDirectory);
                }
                
                dbContext.Database.EnsureCreated();
                
                if (File.Exists(dbPath))
                {
                    var fileInfo = new FileInfo(dbPath);
                    Console.WriteLine($"✅ Banco de dados criado/verificado com sucesso!");
                    Console.WriteLine($"   Caminho: {dbPath}");
                    Console.WriteLine($"   Tamanho: {fileInfo.Length} bytes");
                    Console.WriteLine($"   Criado em: {fileInfo.CreationTime}");
                }
                else
                {
                    Console.WriteLine($"⚠️ EnsureCreated() executou, mas arquivo não encontrado: {dbPath}");
                }
                
                dbCreated = true;
                break;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                Console.WriteLine($"❌ Tentativa {i + 1} falhou: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"   StackTrace: {ex.StackTrace}");
                Console.WriteLine($"   Aguardando {retryDelay}ms antes de tentar novamente...");
                
                Thread.Sleep(retryDelay);
                CleanSqliteTempFiles(dbPath);
                
                retryDelay = Math.Min(retryDelay * 2, 10000);
            }
        }
        
        if (!dbCreated)
        {
            throw new Exception($"Não foi possível criar o banco após {maxRetries} tentativas");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERRO CRÍTICO ao criar banco de dados:");
        Console.WriteLine($"   Mensagem: {ex.Message}");
        Console.WriteLine($"   Tipo: {ex.GetType().Name}");
        Console.WriteLine($"   Caminho tentado: {dbPath}");
        Console.WriteLine($"   Diretório atual: {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"   RAILWAY_VOLUME_MOUNT_PATH: {Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH") ?? "não definido"}");
        Console.WriteLine($"   Diretório existe: {Directory.Exists(dbDirectory)}");
        Console.WriteLine($"   Permissão de escrita: {IsDirectoryWritable(dbDirectory)}");
        
        Console.WriteLine($"⚠️ A aplicação continuará, mas o banco será criado na primeira requisição.");
        Console.WriteLine($"💡 Dica: Verifique se o volume persistente está configurado no Railway.");
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

app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();
    if (!string.IsNullOrEmpty(origin))
    {
        Console.WriteLine($"🌐 Requisição de origem: {origin}, Método: {context.Request.Method}");
    }
    await next();
});

app.Use(async (context, next) =>
{
    try
    {
        if (!File.Exists(dbPath))
        {
            Console.WriteLine($"⚠️ Banco não existe, tentando criar antes da requisição: {dbPath}");
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.EnsureCreated();
                    Console.WriteLine($"✅ Banco criado com sucesso antes da requisição");
                }
            }
            catch (Exception createEx)
            {
                Console.WriteLine($"❌ Falha ao criar banco antes da requisição: {createEx.Message}");
            }
        }
        
        await next();
    }
    catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("no such table") || ex.Message.Contains("unable to open database"))
    {
        Console.WriteLine($"⚠️ Erro de banco detectado na requisição: {ex.Message}");
        Console.WriteLine($"   Tentando criar banco de dados...");
        
        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureCreated();
                Console.WriteLine($"✅ Banco criado com sucesso após erro na requisição");
            }
            
            await next();
        }
        catch (Exception createEx)
        {
            Console.WriteLine($"❌ Falha ao criar banco após erro: {createEx.Message}");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("{\"message\":\"Erro ao acessar banco de dados. Tente novamente em alguns instantes.\"}");
        }
    }
});




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
