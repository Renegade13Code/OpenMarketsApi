FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["OpenMarkets.Api/OpenMarkets.Api.csproj", "OpenMarkets.Api/"]
COPY ["OpenMarkets.Core/OpenMarkets.Core.csproj", "OpenMarkets.Core/"]
COPY ["OpenMarkets.Infrastructure/OpenMarkets.Infrastructure.csproj", "OpenMarkets.Infrastructure/"]

RUN dotnet restore "OpenMarkets.Api/OpenMarkets.Api.csproj"

COPY . .
WORKDIR "/src/OpenMarkets.Api"
RUN dotnet build "OpenMarkets.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenMarkets.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "OpenMarkets.Api.dll"]
