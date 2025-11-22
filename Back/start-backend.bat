@echo off
echo Compilando backend...
cd /d "%~dp0"

echo Limpando build anterior...
dotnet clean >nul 2>&1

echo Compilando projeto...
dotnet build -c Release

if errorlevel 1 (
    echo Erro na compilacao!
    pause
    exit /b 1
)

echo.
echo Iniciando backend...
echo Backend rodando em: http://localhost:5155
echo Swagger disponivel em: http://localhost:5155/swagger
echo.
echo Pressione Ctrl+C para parar o servidor
echo.

bin\Release\net8.0\Backend.exe

pause

