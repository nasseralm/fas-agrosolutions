@echo off
REM Sobe todos os containers (SQL, FAS-Usuarios, FAS-Propriedades, FAS-DataReceiver, Mongo, Redis, Kafka)
REM Uso: up.bat

cd /d "%~dp0"

echo Subindo stack (docker compose)...
docker compose up -d

if errorlevel 1 (
    echo Erro ao subir os containers.
    exit /b 1
)

echo.
echo Containers em execucao:
docker compose ps

echo.
echo Endpoints:
echo   SQL Server:       localhost:1433 (sa / Your_strong_Passw0rd!)
echo   FAS-Usuarios:     http://localhost:8082
echo   FAS-Propriedades: http://localhost:8081
echo   FAS-DataReceiver: http://localhost:8080
echo   MongoDB:          localhost:27017
echo   Redis:            localhost:6379
echo   Kafka:            localhost:9092
echo.
echo Para ver os logs: docker compose logs -f
