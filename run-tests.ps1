# Testes unitários — obrigatório antes do deploy local
# Uso: .\run-tests.ps1   (na raiz do repositório)
# Sai com código 1 se algum teste falhar.

Set-Location $PSScriptRoot
$ErrorActionPreference = "Stop"
$failed = $false

$solutions = @(
    "FAS-Usuarios/FAS.sln",
    "FAS-Propriedades/FAS.sln",
    "FAS-DataReceiver/Agro.DataReceiver.sln"
)

Write-Host "Executando testes unitarios (deploy local exige que passem)..." -ForegroundColor Cyan
foreach ($sln in $solutions) {
    if (-not (Test-Path $sln)) { continue }
    Write-Host ""
    Write-Host "  Test: $sln" -ForegroundColor Gray
    try {
        dotnet test $sln --configuration Release --verbosity minimal --nologo
        if ($LASTEXITCODE -ne 0) { $failed = $true }
    } catch {
        Write-Host "  Erro: $_" -ForegroundColor Red
        $failed = $true
    }
}

Write-Host ""
if ($failed) {
    Write-Host "Falha: um ou mais projetos de teste falharam. Corrija antes do deploy local." -ForegroundColor Red
    exit 1
}
Write-Host "Todos os testes passaram. Pode prosseguir com o deploy local (ex.: docker compose up -d)." -ForegroundColor Green
exit 0
