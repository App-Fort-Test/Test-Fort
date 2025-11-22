#!/bin/bash

echo "Compilando backend..."

# Navegar para o diretório do script
cd "$(dirname "$0")"

# Limpar build anterior
echo "Limpando build anterior..."
dotnet clean > /dev/null 2>&1

# Compilar o projeto
echo "Compilando projeto..."
dotnet build -c Release

if [ $? -ne 0 ]; then
    echo "Erro na compilação!"
    exit 1
fi

# Executar o executável
echo ""
echo "Iniciando backend..."
echo "Backend rodando em: http://localhost:5155"
echo "Swagger disponível em: http://localhost:5155/swagger"
echo ""
echo "Pressione Ctrl+C para parar o servidor"
echo ""

./bin/Release/net8.0/Backend

