# =============================================
# MenuSemanal - Dockerfile (production)
# =============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS base
WORKDIR /app

# Install wget for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends wget curl && rm -rf /var/lib/apt/lists/*

# SECURITY: Don't use non-root user when mounting volumes owned by root/host user
# The mounted volume /app/data will be writable by whoever mounts it
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_PRINT_TELEMETRY_MESSAGE=false

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1

# Copy published output
COPY . .

ENTRYPOINT ["dotnet", "MenuSemanal.dll"]
