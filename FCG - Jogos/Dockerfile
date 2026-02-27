# syntax=docker/dockerfile:1.7

ARG DOTNET_VERSION=8.0

############################
# 1️ - RESTORE (cache NuGet)
############################
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS restore
WORKDIR /src

COPY ["FCG.API/FCG.API.csproj", "FCG.API/"]
COPY ["FCG.Application/FCG.Application.csproj", "FCG.Application/"]
COPY ["FCG.Domain/FCG.Domain.csproj", "FCG.Domain/"]
COPY ["FCG.Infra.Data/FCG.Infra.Data.csproj", "FCG.Infra.Data/"]
COPY ["FCG.Infra.Ioc/FCG.Infra.Ioc.csproj", "FCG.Infra.Ioc/"]

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "FCG.API/FCG.API.csproj"

############################
# 2 - BUILD
############################
FROM restore AS build
WORKDIR /src

COPY . .

WORKDIR /src/FCG.API
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet build "FCG.API.csproj" \
        -c Release \
        --no-restore \
        -o /app/build

############################
# 3 - PUBLISH
############################
FROM build AS publish
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "FCG.API.csproj" \
        -c Release \
        --no-restore \
        -o /app/publish \
        /p:UseAppHost=false

############################
# 4 - RUNTIME (CHISELED + ICU)
############################
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-jammy-chiseled-extra AS runtime
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "FCG.API.dll"]
