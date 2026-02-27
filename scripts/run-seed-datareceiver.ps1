# Executa o seed do DataReceiver (view Talhoes + tabela Dispositivos + mapeamento SENS-001..004 -> talhoes 1..4).
# Requer: stack no ar (fas-sqlserver com DB AgroSolutions) e migrations do FAS-Propriedades já aplicadas (talhoes do produtor demo).
# Uso: .\scripts\run-seed-datareceiver.ps1

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not (Test-Path "$root\scripts\seed-datareceiver-demo.sql")) {
    Write-Error "Arquivo scripts\seed-datareceiver-demo.sql nao encontrado."
    exit 1
}

$sqlFile = Resolve-Path "$root\scripts\seed-datareceiver-demo.sql"
$container = "fas-sqlserver"
$pass = "Your_strong_Passw0rd!"

# Verifica se o container existe
$running = docker ps -a --filter "name=$container" --format "{{.Names}}"
if (-not $running) {
    Write-Error "Container $container nao encontrado. Suba o stack antes: docker compose up -d"
    exit 1
}

Write-Host "Executando seed DataReceiver (Talhoes + Dispositivos) no AgroSolutions..."
$sql = Get-Content $sqlFile -Raw
# sqlcmd no Linux aceita script pelo stdin; -d AgroSolutions para usar o DB correto
$sql | docker exec -i $container /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $pass -C -d AgroSolutions 2>&1 | Out-Host
if ($LASTEXITCODE -eq 0) {
    Write-Host "Seed DataReceiver concluido. SENS-001..004 mapeados para talhoes 1..4."
} else {
    Write-Host "Falha ao executar seed. Execute manualmente: Get-Content scripts\seed-datareceiver-demo.sql | docker exec -i fas-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $pass -C -d AgroSolutions"
    exit 1
}
