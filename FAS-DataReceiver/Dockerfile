FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Agro.DataReceiver.csproj", "."]
RUN dotnet restore "Agro.DataReceiver.csproj"
COPY . .
RUN dotnet build "Agro.DataReceiver.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Agro.DataReceiver.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Agro.DataReceiver.dll"]
