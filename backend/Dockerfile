
# Etapa 1: Compilar o projeto .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Restaurar dependências
COPY backend.csproj ./
RUN dotnet restore backend.csproj

# Copiar arquivos do backend
COPY . ./

# Publicar aplicação em Release
RUN dotnet publish backend.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Etapa 2: Executar a API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish ./

# Configurações de produção
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "backend.dll"]
