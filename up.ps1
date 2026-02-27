# Sobe todos os containers (SQL Server, FAS-Usuarios, FAS-Propriedades, FAS-DataReceiver, Mongo, Redis, Kafka)
# Uso: .\up.ps1   ou   pwsh -File up.ps1

Set-Location $PSScriptRoot

Write-Host "Subindo stack (docker compose)..." -ForegroundColor Cyan
docker compose up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao subir os containers." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Containers em execucao:" -ForegroundColor Green
docker compose ps

Write-Host ""
Write-Host "Endpoints:" -ForegroundColor Yellow
Write-Host "  SQL Server:     localhost:1433 (sa / Your_strong_Passw0rd!)"
Write-Host "  FAS-Usuarios:    http://localhost:8082"
Write-Host "  FAS-Propriedades: http://localhost:8081"
Write-Host "  FAS-DataReceiver: http://localhost:8080"
Write-Host "  MongoDB:         localhost:27017"
Write-Host "  Redis:           localhost:6379"
Write-Host "  Kafka:           localhost:9092"
Write-Host ""
Write-Host "Para ver os logs: docker compose logs -f" -ForegroundColor Gray
