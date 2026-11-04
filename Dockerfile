# syntax=docker/dockerfile:1

# ---- build ----------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/Plexa.Domain/Plexa.Domain.csproj src/Plexa.Domain/
COPY src/Plexa.Application/Plexa.Application.csproj src/Plexa.Application/
COPY src/Plexa.Infrastructure/Plexa.Infrastructure.csproj src/Plexa.Infrastructure/
COPY src/Plexa.Web/Plexa.Web.csproj src/Plexa.Web/
RUN dotnet restore src/Plexa.Web/Plexa.Web.csproj

COPY src/ src/
RUN dotnet publish src/Plexa.Web/Plexa.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---- runtime --------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true

EXPOSE 8080

COPY --from=build /app/publish .

# Ejecuta como usuario no root incluido en la imagen base.
USER $APP_UID

ENTRYPOINT ["dotnet", "Plexa.Web.dll"]
