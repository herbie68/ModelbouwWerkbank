param(
    [string]$Configuration = "Release",
    [string]$InnoCompilerPath = "",
    [switch]$SkipPublish
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$projectPath = Join-Path $repoRoot "Modelbouwer\Modelbouwer.csproj"
$publishDir = Join-Path $repoRoot "Builds\Publish"
$installerDir = Join-Path $repoRoot "Builds\Installer"
$scriptPath = Join-Path $PSScriptRoot "Modelbuilder.iss"

New-Item -ItemType Directory -Force $publishDir, $installerDir | Out-Null

if (-not $SkipPublish) {
    dotnet publish $projectPath `
        --configuration $Configuration `
        --output $publishDir `
        /p:PublishSingleFile=true `
        /p:SelfContained=false `
        /p:PublishReadyToRun=true
}

if (-not (Test-Path (Join-Path $publishDir "Modelbouwer.exe"))) {
    throw "Publish output was not found. Run without -SkipPublish first."
}

if ([string]::IsNullOrWhiteSpace($InnoCompilerPath)) {
    $candidates = @(
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe",
        "C:\Users\hnijk\OneDrive\DevOps\hnsoftwaredevelopment\InnoSetup\Inno-All-in-One-Setup-master\IsPack_5_5_2\isfiles-unicode\ISCC.exe",
        "C:\Users\hnijk\OneDrive\DevOps\hnsoftwaredevelopment\InnoSetup\Inno-All-in-One-Setup-master\IsPack_5_5_2\isfiles\ISCC.exe"
    )

    $InnoCompilerPath = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if (-not (Test-Path $InnoCompilerPath)) {
    throw "ISCC.exe was not found. Install Inno Setup 6 or pass -InnoCompilerPath."
}

& $InnoCompilerPath $scriptPath

if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup compiler failed with exit code $LASTEXITCODE."
}

Get-ChildItem $installerDir -Filter "ModelbuilderSetup-*.exe" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
