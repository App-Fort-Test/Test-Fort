# Script simples para matar processo Backend
Write-Host "Matando processo Backend (24160)..." -ForegroundColor Red

# Tentar múltiplas formas de parar o processo
$pid = 24160

# Método 1: Stop-Process
try {
    Stop-Process -Id $pid -Force -ErrorAction Stop
    Write-Host "Processo parado via Stop-Process" -ForegroundColor Green
} catch {
    Write-Host "Stop-Process falhou, tentando taskkill..." -ForegroundColor Yellow
}

# Método 2: taskkill
Start-Process -FilePath "taskkill" -ArgumentList "/F", "/PID", $pid -Wait -NoNewWindow -WindowStyle Hidden 2>$null

# Método 3: Parar todos os Backend
Get-Process | Where-Object {$_.ProcessName -eq "Backend"} | ForEach-Object {
    taskkill /F /PID $_.Id 2>$null | Out-Null
    Write-Host "Processo $($_.Id) parado" -ForegroundColor Green
}

Start-Sleep -Seconds 2

# Verificar se foi parado
if (Get-Process -Id $pid -ErrorAction SilentlyContinue) {
    Write-Host "`nERRO: Processo ainda está rodando!" -ForegroundColor Red
    Write-Host "Por favor, pare manualmente via Gerenciador de Tarefas:" -ForegroundColor Yellow
    Write-Host "1. Pressione Ctrl+Shift+Esc" -ForegroundColor Cyan
    Write-Host "2. Procure por processo ID $pid ou 'Backend'" -ForegroundColor Cyan
    Write-Host "3. Clique com botão direito -> Finalizar tarefa" -ForegroundColor Cyan
} else {
    Write-Host "`nProcesso parado com sucesso!" -ForegroundColor Green
}

