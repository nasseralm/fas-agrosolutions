# Popula o MongoDB com leituras históricas (24h) para o gráfico de umidade.
# Requer: container mongodb rodando (docker compose up -d).
# Uso: .\scripts\run-seed-mongo-historical.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

Set-Location $repoRoot
$scriptPath = Join-Path $scriptDir "seed-mongo-historical.js"

if (-not (Test-Path $scriptPath)) {
    Write-Error "Arquivo não encontrado: $scriptPath"
}

Write-Host "Copiando script para o container mongodb..."
docker cp $scriptPath mongodb:/tmp/seed-mongo-historical.js

Write-Host "Executando seed no MongoDB (agrosolutions)..."
docker exec mongodb mongosh agrosolutions --file /tmp/seed-mongo-historical.js

Write-Host "Concluído. Atualize o dashboard para ver o gráfico de histórico 24h."
docker exec mongodb rm /tmp/seed-mongo-historical.js 2>$null
