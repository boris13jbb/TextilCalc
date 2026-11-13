[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$projectPath = Join-Path $projectRoot 'TextilCalc.App\TextilCalc.App.csproj'
$coreProjectPath = Join-Path $projectRoot 'TextilCalc.Core\TextilCalc.Core.csproj'
$publishDirectory = Join-Path $projectRoot 'artifacts\windows\publish'
$outputDirectory = Join-Path $projectRoot 'artifacts\windows'
$installerScript = Join-Path $PSScriptRoot 'TextilCalc.iss'
$targetFramework = 'net10.0-windows10.0.19041.0'
$project = [xml](Get-Content -LiteralPath $projectPath -Raw)
$appVersion = $project.Project.PropertyGroup.ApplicationDisplayVersion |
    Where-Object { $_ } |
    Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($appVersion)) {
    throw 'ApplicationDisplayVersion no está configurado en TextilCalc.App.csproj.'
}

$isccCandidates = @(
    (Get-Command 'ISCC.exe' -ErrorAction SilentlyContinue).Source
    (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')
    (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe')
    (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe')
) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }

$isccPath = $isccCandidates | Select-Object -First 1
if (-not $isccPath) {
    throw 'Inno Setup 6 no está instalado. Ejecute: winget install --id JRSoftware.InnoSetup --exact'
}

if (Test-Path -LiteralPath $publishDirectory) {
    Remove-Item -LiteralPath $publishDirectory -Recurse -Force
}

New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null

$env:NUGET_PACKAGES = Join-Path $env:USERPROFILE '.nuget\packages'

dotnet restore $projectPath `
    -p:TargetFramework=$targetFramework `
    --runtime win-x64 `
    --force

if ($LASTEXITCODE -ne 0) {
    throw "La restauración de Windows falló con código $LASTEXITCODE."
}

# La restauración con un TFM global afecta las referencias; se restaura Core con su TFM propio.
dotnet restore $coreProjectPath --force

if ($LASTEXITCODE -ne 0) {
    throw "La restauración de TextilCalc.Core falló con código $LASTEXITCODE."
}

dotnet publish $projectPath `
    --framework $targetFramework `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --no-restore `
    --output $publishDirectory `
    -p:WindowsPackageType=None `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=true

if ($LASTEXITCODE -ne 0) {
    throw "La publicación de Windows falló con código $LASTEXITCODE."
}

& $isccPath `
    "/DSourceDir=$publishDirectory" `
    "/DOutputDir=$outputDirectory" `
    "/DAppVersion=$appVersion" `
    $installerScript

if ($LASTEXITCODE -ne 0) {
    throw "La creación del instalador falló con código $LASTEXITCODE."
}

$installerPath = Join-Path $outputDirectory "TextilCalc-Setup-$appVersion-x64.exe"
if (-not (Test-Path -LiteralPath $installerPath)) {
    throw "No se encontró el instalador esperado: $installerPath"
}

Write-Host "Instalador generado correctamente: $installerPath"
