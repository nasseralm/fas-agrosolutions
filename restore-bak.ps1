# Restaura o backup AgroSolutions.bak no SQL Server (container fas-sqlserver).
# Requer: stack rodando (docker compose up -d) e AgroSolutions.bak na raiz do repositorio.
# Uso: .\restore-bak.ps1

$ErrorActionPreference = "Stop"
$containerName = "fas-sqlserver"
$backupPath = "/backup/AgroSolutions.bak"
$password = "Your_strong_Passw0rd!"
$dataDir = "/var/opt/mssql/data"

# sqlcmd pode estar em um dos caminhos no container
$sqlcmdPaths = @(
    "/opt/mssql-tools18/bin/sqlcmd",
    "/opt/mssql-tools/bin/sqlcmd"
)

function Get-SqlcmdPath {
    foreach ($p in $sqlcmdPaths) {
        $r = docker exec $containerName sh -c "test -x $p 2>/dev/null && echo ok" 2>$null
        if ($r -match "ok") { return $p }
    }
    return $null
}

function Invoke-SqlInContainer {
    param([string]$Query, [switch]$PipeDelimited, [switch]$NoHeader)
    $sqlcmd = Get-SqlcmdPath
    if (-not $sqlcmd) {
        Write-Host "Erro: sqlcmd nao encontrado no container." -ForegroundColor Red
        exit 1
    }
    $args = @("-S", "localhost", "-U", "sa", "-P", $password, "-C", "-Q", $Query)
    if ($PipeDelimited) { $args += @("-s", "|", "-W") }
    if ($NoHeader) { $args += @("-h", "-1") }
    docker exec $containerName $sqlcmd @args 2>&1
}

Write-Host "Verificando container $containerName..." -ForegroundColor Cyan
$running = docker inspect -f '{{.State.Running}}' $containerName 2>$null
if ($running -ne "true") {
    Write-Host "Container $containerName nao esta rodando. Execute: docker compose up -d" -ForegroundColor Red
    exit 1
}

Write-Host "Listando arquivos do backup (RESTORE FILELISTONLY)..." -ForegroundColor Cyan
$filelist = Invoke-SqlInContainer -Query "RESTORE FILELISTONLY FROM DISK = N'$backupPath'" -PipeDelimited -NoHeader

# Parse: colunas LogicalName|PhysicalName|Type|... (pipe-delimited)
$dataName = $null
$logName = $null
$lines = ($filelist -join "`n") -split "`n"
foreach ($line in $lines) {
    $line = $line.Trim()
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $cols = $line -split "\|"
    if ($cols.Count -ge 3) {
        $logical = $cols[0].Trim()
        $type = $cols[2].Trim()
        if ($type -eq "D") { $dataName = $logical }
        if ($type -eq "L") { $logName = $logical }
    }
}

if (-not $dataName -or -not $logName) {
    Write-Host "Nao foi possivel obter os nomes logicos do backup. Saida:" -ForegroundColor Yellow
    Write-Host $filelist
    Write-Host ""
    Write-Host "Execute manualmente no container (ajuste os nomes logicos):" -ForegroundColor Gray
    Write-Host "  docker exec -it $containerName /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $password -C"
    Write-Host "  RESTORE FILELISTONLY FROM DISK = N'$backupPath'"
    exit 1
}

Write-Host "  Data: $dataName | Log: $logName" -ForegroundColor Gray
Write-Host "Executando RESTORE DATABASE AgroSolutions..." -ForegroundColor Cyan

# Escapar aspas simples nos nomes logicos para T-SQL (duplicar ')
$dataNameEsc = $dataName -replace "'", "''"
$logNameEsc = $logName -replace "'", "''"

$restoreQuery = @"
RESTORE DATABASE AgroSolutions FROM DISK = N'$backupPath'
WITH REPLACE,
  MOVE N'$dataNameEsc' TO N'$dataDir/AgroSolutions.mdf',
  MOVE N'$logNameEsc' TO N'$dataDir/AgroSolutions_log.ldf';
"@

$restoreResult = Invoke-SqlInContainer -Query $restoreQuery

if ($LASTEXITCODE -ne 0) {
    Write-Host "Saida do RESTORE:" -ForegroundColor Yellow
    Write-Host $restoreResult
    Write-Host "Restore pode ter falhado (ex.: database em uso)." -ForegroundColor Red
    exit 1
}

Write-Host "Restore concluido com sucesso. Database AgroSolutions restaurado." -ForegroundColor Green
