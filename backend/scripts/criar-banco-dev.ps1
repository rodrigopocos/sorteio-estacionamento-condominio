# Cria sorteio-dev.db via EF (requer .NET SDK + dotnet-ef)
$ErrorActionPreference = "Stop"
$infra = Join-Path $PSScriptRoot "..\src\SorteioEstacionamento.Infrastructure"
Push-Location $infra
try {
    dotnet ef database update
    $db = Join-Path $infra "sorteio-dev.db"
    Write-Host "Banco criado/atualizado: $db"
}
finally {
    Pop-Location
}
