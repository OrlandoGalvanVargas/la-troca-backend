# 1. ETAPA DE CONSTRUCCIÓN (BUILD)
# Usamos la imagen SDK de .NET 9 para compilar todo el código.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia los archivos de la solución para restaurar las dependencias (NuGet)
# Esto optimiza el caché de Docker.
COPY ["TorneoUniversitario.API/TorneoUniversitario.API.csproj", "TorneoUniversitario.API/"]
COPY ["TorneoUniversitario.Application/TorneoUniversitario.Application.csproj", "TorneoUniversitario.Application/"]
COPY ["TorneoUniversitario.Domain/TorneoUniversitario.Domain.csproj", "TorneoUniversitario.Domain/"]
COPY ["TorneoUniversitario.Infrastructure/TorneoUniversitario.Infrastructure.csproj", "TorneoUniversitario.Infrastructure/"]

# Copia el archivo de solución (.sln) y el resto del código
COPY . .
WORKDIR "/src/TorneoUniversitario.API"

# Restaura las dependencias y publica el proyecto final
RUN dotnet restore "TorneoUniversitario.API.csproj"
RUN dotnet publish "TorneoUniversitario.API.csproj" -c Release -o /app/publish

# 2. ETAPA FINAL (RUNTIME)
# Usamos la imagen de runtime de ASP.NET 9 (mucho más pequeña y segura)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
# Copia los archivos publicados de la etapa 'build'
COPY --from=build /app/publish .

# Define el puerto. Render usará automáticamente $PORT.
ENV ASPNETCORE_URLS=http://+:$PORT

# Comando para iniciar la aplicación
ENTRYPOINT ["dotnet", "TorneoUniversitario.API.dll"]