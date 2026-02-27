# syntax=docker/dockerfile:1.7

ARG DOTNET_VERSION=8.0

############################
# 1 - RESTORE (cache NuGet)
############################
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS restore
WORKDIR /src

COPY ["FAS.API/FAS.API.csproj", "FAS.API/"]
COPY ["FAS.Application/FAS.Application.csproj", "FAS.Application/"]
COPY ["FAS.Domain/FAS.Domain.csproj", "FAS.Domain/"]
COPY ["FAS.Infra.Data/FAS.Infra.Data.csproj", "FAS.Infra.Data/"]
COPY ["FAS.Infra.Ioc/FAS.Infra.Ioc.csproj", "FAS.Infra.Ioc/"]

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "FAS.API/FAS.API.csproj"

############################
# 2 - BUILD
############################
FROM restore AS build
WORKDIR /src

COPY . .

WORKDIR /src/FAS.API
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet build "FAS.API.csproj" \
        -c Release \
        --no-restore \
        -o /app/build

############################
# 3 - PUBLISH
############################
FROM build AS publish
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "FAS.API.csproj" \
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

ENTRYPOINT ["dotnet", "FAS.API.dll"]

