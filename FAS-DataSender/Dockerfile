FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Agro.SensorSimulator.Worker.csproj", "."]
RUN dotnet restore "Agro.SensorSimulator.Worker.csproj"
COPY . .
RUN dotnet build "Agro.SensorSimulator.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Agro.SensorSimulator.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Agro.SensorSimulator.Worker.dll"]
