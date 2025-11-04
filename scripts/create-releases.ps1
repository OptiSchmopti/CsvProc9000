param(
[Parameter(Position=0,mandatory=$true)][string]$version)

$projects = @(
    "../src/CsvProc9000/CsvProc9000.csproj",
    "../src/CsvProc9000.UI.Wpf/CsvProc9000.UI.Wpf.csproj"
)

$excludePatterns = @(
    "appsettings.Development.json",
    "*.pdb",
    "aspnetcorev2_inprocess.dll"
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

$versionSuffix = "-v" + ($version -replace '\.', '_')

foreach ($projPath in $projects) {
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($projPath)

    $outputDir = Join-Path $scriptDir "$projName$versionSuffix"

    Write-Host "`n--- Publishing $projName ---" -ForegroundColor Cyan

    if (Test-Path $outputDir) {
        Remove-Item $outputDir -Recurse -Force
    }

    dotnet publish $projPath -c Release -p:Version=$version -o $outputDir
    
    foreach ($pattern in $excludePatterns) {
        Get-ChildItem -Path $outputDir -Recurse -Filter $pattern | Remove-Item -Force
    }

    $zipPath = "$outputDir.zip"
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }

    Write-Host "Zipping $projName..."
    Compress-Archive -Path "$outputDir\*" -DestinationPath $zipPath

    Write-Host "✅ DONE: $zipPath" -ForegroundColor Green
}
