# Use a imagem base do .NET 8.0 SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar o arquivo de projeto e restaurar dependências
COPY Backend.csproj .
RUN dotnet restore

# Copiar o resto dos arquivos e fazer build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Use a imagem runtime do .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar os arquivos publicados
COPY --from=build /app/publish .

# Expor a porta (Railway define a variável PORT)
ENV ASPNETCORE_URLS=http://+:${PORT}
EXPOSE 8080

# Comando de start
ENTRYPOINT ["dotnet", "Backend.dll"]

