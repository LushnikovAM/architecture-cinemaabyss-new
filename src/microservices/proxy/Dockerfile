FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CinemaAbyss.Proxy.csproj ./
RUN dotnet restore CinemaAbyss.Proxy.csproj

COPY . .
RUN dotnet publish CinemaAbyss.Proxy.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 8000

ENTRYPOINT ["dotnet", "CinemaAbyss.Proxy.dll"]
