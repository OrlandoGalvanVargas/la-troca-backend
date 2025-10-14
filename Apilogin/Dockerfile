# 1. ETAPA DE CONSTRUCCIÓN (BUILD)
# Usamos la imagen SDK de .NET 9 para compilar todo el código.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia los archivos de la solución para restaurar las dependencias (NuGet)
COPY ["LaTroca.API/LaTroca.API.csproj", "LaTroca.API/"]
COPY ["LaTroca.Application/LaTroca.Application.csproj", "LaTroca.Application/"]
COPY ["LaTroca.Domain/LaTroca.Domain.csproj", "LaTroca.Domain/"]
COPY ["LaTroca.Infrastructure/LaTroca.Infrastructure.csproj", "LaTroca.Infrastructure/"]

# Restaura las dependencias
RUN dotnet restore "LaTroca.API/LaTroca.API.csproj"

# Copia todo el código
COPY . .
WORKDIR "/src/LaTroca.API"

# Publica el proyecto
RUN dotnet publish "LaTroca.API.csproj" -c Release -o /app/publish

# 2. ETAPA FINAL (RUNTIME)
# Imagen liviana de runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render usará automáticamente $PORT
ENV ASPNETCORE_URLS=http://+:$PORT

# Comando de inicio
ENTRYPOINT ["dotnet", "LaTroca.API.dll"]
