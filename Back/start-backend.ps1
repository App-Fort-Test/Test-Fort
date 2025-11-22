# Script para compilar e executar o backend
Write-Host "Compilando backend..." -ForegroundColor Yellow

# Navegar para o diretório do backend
Set-Location $PSScriptRoot

# Parar processos Backend que possam estar rodando
Write-Host "Verificando processos Backend rodando..." -ForegroundColor Cyan
$backendProcesses = Get-Process | Where-Object {$_.ProcessName -like "*Backend*"}
if ($backendProcesses) {
    Write-Host "Parando processos Backend existentes..." -ForegroundColor Yellow
    $backendProcesses | ForEach-Object {
        try {
            Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
            taskkill /F /PID $_.Id 2>$null | Out-Null
            Write-Host "  Processo $($_.Id) parado" -ForegroundColor Green
        } catch {
            Write-Host "  Aviso: Não foi possível parar processo $($_.Id)" -ForegroundColor Yellow
        }
    }
    Start-Sleep -Seconds 2
}

# Limpar build anterior
Write-Host "Limpando build anterior..." -ForegroundColor Cyan
dotnet clean | Out-Null

# Compilar o projeto
Write-Host "Compilando projeto..." -ForegroundColor Cyan
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro na compilação!" -ForegroundColor Red
    exit 1
}

# Executar o executável compilado
Write-Host "`nIniciando backend..." -ForegroundColor Green
Write-Host "Backend rodando em: http://localhost:5155" -ForegroundColor Green
Write-Host "Swagger disponível em: http://localhost:5155/swagger" -ForegroundColor Green
Write-Host "`nPressione Ctrl+C para parar o servidor`n" -ForegroundColor Yellow

# Executar o executável
& ".\bin\Release\net8.0\Backend.exe"

