# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY src/CustomerEngagement.Api/CustomerEngagement.Api.csproj src/CustomerEngagement.Api/
COPY src/CustomerEngagement.Core/CustomerEngagement.Core.csproj src/CustomerEngagement.Core/
COPY src/CustomerEngagement.Application/CustomerEngagement.Application.csproj src/CustomerEngagement.Application/
COPY src/CustomerEngagement.Infrastructure/CustomerEngagement.Infrastructure.csproj src/CustomerEngagement.Infrastructure/
COPY src/CustomerEngagement.Enterprise/CustomerEngagement.Enterprise.csproj src/CustomerEngagement.Enterprise/

RUN dotnet restore src/CustomerEngagement.Api/CustomerEngagement.Api.csproj

# Copy remaining source and publish
COPY src/ src/
RUN dotnet publish src/CustomerEngagement.Api/CustomerEngagement.Api.csproj \
    -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser -d /app -s /sbin/nologin appuser

COPY --from=build /app/publish .

# Create data directory for GeoIP database
RUN mkdir -p /app/data && chown -R appuser:appuser /app

USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "CustomerEngagement.Api.dll"]
