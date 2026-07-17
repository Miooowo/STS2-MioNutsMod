param(
    [ValidateSet("normal", "host", "client")]
    [string]$Mode = "normal",
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$gameRoot = "F:/steam/steamapps/common/Slay the Spire 2"
$exePath = Join-Path $gameRoot "SlayTheSpire2.exe"
$hostBat = Join-Path $gameRoot "launch_host.bat"
$clientBat = Join-Path $gameRoot "launch_client.bat"

if (-not (Test-Path $gameRoot)) {
    throw "Game directory not found: $gameRoot"
}

$target = $exePath
if ($Mode -eq "host") {
    $target = $hostBat
} elseif ($Mode -eq "client") {
    $target = $clientBat
}

if (-not (Test-Path $target)) {
    throw "Launch target not found: $target"
}

Write-Host "Launching target: $target"
if ($DryRun) {
    Write-Host "DryRun mode, nothing was launched."
    exit 0
}

Start-Process -FilePath $target -WorkingDirectory $gameRoot
Write-Host "Slay the Spire 2 started (mode: $Mode)"
