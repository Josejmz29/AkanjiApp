# Usa imagen oficial de .NET 7 SDK para compilar y publicar
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
# Necesitas mysql-client para el script wait-for-mysql.sh

WORKDIR /app

# Copia el archivo de solución al directorio de trabajo
COPY AkanjiApp.sln ./

# Copia el proyecto .csproj al directorio correcto
COPY AkanjiApp/AkanjiApp.csproj ./AkanjiApp/
# Restaura dependencia
RUN dotnet restore

COPY . ./
RUN dotnet publish AkanjiApp/AkanjiApp.csproj -c Release -o /out

# Usa el runtime de .NET 7 para ejecutar la app
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /out ./

EXPOSE 5215
ENTRYPOINT ["dotnet", "AkanjiApp.dll"]
