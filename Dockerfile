# =============================================
# MenuSemanal - Dockerfile (multi-stage)
#   docker build -t menusemanal .
#   docker run -d -p 8080:8080 -v menusemanal-data:/app/data menusemanal
# =============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
WORKDIR /src
COPY MenuSemanal.csproj .
RUN dotnet restore MenuSemanal.csproj
COPY . .
RUN dotnet publish MenuSemanal.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS final
WORKDIR /app

# wget para el HEALTHCHECK
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_PRINT_TELEMETRY_MESSAGE=false \
    DB_PATH=/app/data/menus.db \
    TZ=America/Bogota

# La BD vive en un volumen para sobrevivir a recreaciones del contenedor.
VOLUME ["/app/data"]
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MenuSemanal.dll"]
