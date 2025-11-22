# Script para parar processos Backend bloqueados
Write-Host "Parando processos Backend..." -ForegroundColor Yellow

# Função para parar processo de forma mais agressiva
function Stop-ProcessForce {
    param([int]$ProcessId)
    
    try {
        # Tentar parar normalmente primeiro
        Stop-Process -Id $ProcessId -Force -ErrorAction SilentlyContinue
        Start-Sleep -Milliseconds 500
        
        # Se ainda estiver rodando, usar taskkill
        if (Get-Process -Id $ProcessId -ErrorAction SilentlyContinue) {
            taskkill /F /PID $ProcessId 2>$null | Out-Null
            Start-Sleep -Milliseconds 500
        }
        
        # Verificar se foi parado
        if (-not (Get-Process -Id $ProcessId -ErrorAction SilentlyContinue)) {
            Write-Host "Processo $ProcessId parado com sucesso" -ForegroundColor Green
            return $true
        } else {
            Write-Host "Aviso: Processo $ProcessId ainda está rodando" -ForegroundColor Yellow
            return $false
        }
    } catch {
        Write-Host "Erro ao parar processo $ProcessId : $_" -ForegroundColor Red
        return $false
    }
}

# Parar processo específico mencionado no erro
$processId = 24160
if (Get-Process -Id $processId -ErrorAction SilentlyContinue) {
    Stop-ProcessForce -ProcessId $processId
}

# Parar todos os processos Backend
Get-Process | Where-Object {$_.ProcessName -like "*Backend*"} | ForEach-Object {
    Stop-ProcessForce -ProcessId $_.Id
}

# Parar processos dotnet que possam estar usando o arquivo
Get-Process dotnet -ErrorAction SilentlyContinue | ForEach-Object {
    $procPath = $_.Path
    if ($procPath -like "*Back*" -or $procPath -like "*Privado*") {
        Stop-ProcessForce -ProcessId $_.Id
    }
}

# Aguardar um pouco para garantir que os processos foram finalizados
Write-Host "`nAguardando processos finalizarem..." -ForegroundColor Cyan
Start-Sleep -Seconds 3

# Verificar se ainda há processos bloqueando
$blockingProcesses = Get-Process | Where-Object {
    $_.ProcessName -like "*Backend*" -or 
    ($_.ProcessName -eq "dotnet" -and $_.Path -like "*Back*")
}

if ($blockingProcesses) {
    Write-Host "`nAviso: Ainda há processos rodando:" -ForegroundColor Yellow
    $blockingProcesses | Format-Table Id, ProcessName, Path -AutoSize
    Write-Host "Tente fechar manualmente via Gerenciador de Tarefas (Ctrl+Shift+Esc)" -ForegroundColor Yellow
} else {
    Write-Host "Todos os processos foram parados!" -ForegroundColor Green
}

Write-Host "`nTentando limpar e fazer build..." -ForegroundColor Yellow
Set-Location $PSScriptRoot

# Limpar build
dotnet clean | Out-Null
Start-Sleep -Seconds 1

# Tentar build
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild realizado com sucesso!" -ForegroundColor Green
} else {
    Write-Host "`nErro no build. Verifique os logs acima." -ForegroundColor Red
    Write-Host "`nSolução alternativa:" -ForegroundColor Yellow
    Write-Host "1. Abra o Gerenciador de Tarefas (Ctrl+Shift+Esc)" -ForegroundColor Cyan
    Write-Host "2. Procure por 'Backend' ou processo ID 24160" -ForegroundColor Cyan
    Write-Host "3. Finalize o processo manualmente" -ForegroundColor Cyan
    Write-Host "4. Execute 'dotnet build' novamente" -ForegroundColor Cyan
}

