param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$vsWhere = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
$iscc = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$sln = "$PSScriptRoot\Quest.sln"
$iss = "$PSScriptRoot\Setup\setup.iss"

if (-not (Test-Path $iscc)) {
    Write-Error "Inno Setup not found at: $iscc"
}

if (-not $SkipBuild) {
    $vsPath = & $vsWhere -latest -requires Microsoft.Component.MSBuild -property installationPath
    if (-not $vsPath) { Write-Error "Visual Studio not found via vswhere" }

    $msbuild = Join-Path $vsPath "MSBuild\Current\Bin\MSBuild.exe"
    if (-not (Test-Path $msbuild)) { Write-Error "MSBuild not found at: $msbuild" }

    Write-Host "Building Quest (Release|Any CPU)..." -ForegroundColor Cyan
    & $msbuild $sln /p:Configuration=Release "/p:Platform=Any CPU" /m /v:minimal
    if ($LASTEXITCODE -ne 0) { Write-Error "MSBuild failed with exit code $LASTEXITCODE" }

    $questExe = "$PSScriptRoot\Quest\bin\Release\Quest.exe"
    if (-not (Test-Path $questExe)) {
        Write-Error "Build succeeded but Quest.exe not found at expected path: $questExe"
    }
    Write-Host "Build complete." -ForegroundColor Green
}

Write-Host "Compiling installer..." -ForegroundColor Cyan
& $iscc $iss
if ($LASTEXITCODE -ne 0) { Write-Error "ISCC failed with exit code $LASTEXITCODE" }

$outputDir = "$PSScriptRoot\Setup\bin"
$installer = Get-ChildItem $outputDir -Filter "quest-*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Write-Host "Installer ready: $($installer.FullName)" -ForegroundColor Green
