# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY Backend.csproj* ./
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port (Railway usa variável PORT)
EXPOSE ${PORT:-5155}

# Usar variável PORT do Railway se disponível
ENV ASPNETCORE_URLS=http://+:${PORT:-5155}

ENTRYPOINT ["dotnet", "Backend.dll"]

