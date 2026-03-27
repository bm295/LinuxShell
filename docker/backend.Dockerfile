FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY FootballManager.slnx ./
COPY dotnet-tools.json ./
COPY src/backend/FootballManager.Domain/FootballManager.Domain.csproj src/backend/FootballManager.Domain/
COPY src/backend/FootballManager.Application/FootballManager.Application.csproj src/backend/FootballManager.Application/
COPY src/backend/FootballManager.Infrastructure/FootballManager.Infrastructure.csproj src/backend/FootballManager.Infrastructure/
COPY src/backend/FootballManager.Api/FootballManager.Api.csproj src/backend/FootballManager.Api/

RUN dotnet restore src/backend/FootballManager.Api/FootballManager.Api.csproj

COPY src ./src

RUN dotnet publish src/backend/FootballManager.Api/FootballManager.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FootballManager.Api.dll"]
