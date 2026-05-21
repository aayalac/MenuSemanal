FROM mcr.microsoft.com/dotnet/core/sdk:8.0 AS build-env
WORKDIR /src
COPY . .
WORKDIR /src/MenuSemanal
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/core/aspnet:8.0
WORKDIR /app
COPY --from=build-env /src/MenuSemanal/out ./
ENTRYPOINT ["dotnet", "MenuSemanal.dll"]