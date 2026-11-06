# =============================================
# Stage 1: Build & Publish
# =============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy project file and restore deps (layer caching)
Copy *.csproj ./
Dotnet restore

# Copy everything and publish
Copy --chmod=+x ../../.gitignore .gitignore 2>/dev/null || true
Copy . ./
Dotnet publish -c Release -o /app --self-contained false --no-restore

# =============================================
# Stage 2: Runtime
# =============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN groupadd -g 1000 appgroup \
    && useradd -u 1000 -g appgroup -m appuser
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_PRINT_TELEMETRY_MESSAGE=false

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MenuSemanal.dll"]
