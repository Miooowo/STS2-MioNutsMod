#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"

function Get-RepoRoot {
    return (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
}

function Get-ChangedPaths {
    param([string]$RepoRoot)

    $lines = (& git -C $RepoRoot status --porcelain) | Where-Object { $_ -and $_.Length -ge 4 }
    $paths = @()
    foreach ($line in $lines) {
        $raw = $line.Substring(3).Trim()
        if ($raw -match " -> ") {
            $raw = ($raw -split " -> ")[-1].Trim()
        }
        if ($raw) {
            $paths += $raw.Replace("\", "/")
        }
    }
    return $paths
}

function Is-CodePath {
    param([string]$Path)
    return $Path -match "^STS2-MioNutsModCode/" -or
           $Path -match "\.(cs|csproj|sln|props|targets)$"
}

function Is-ResourcePath {
    param([string]$Path)
    return $Path -match "^STS2-MioNutsMod/localization/" -or
           $Path -match "^STS2-MioNutsMod/images/" -or
           $Path -match "\.import$"
}

$repoRoot = Get-RepoRoot
$stateFile = Join-Path $PSScriptRoot ".session-state.json"

$changedPaths = Get-ChangedPaths -RepoRoot $repoRoot
$touchesCode = ($changedPaths | Where-Object { Is-CodePath $_ }).Count -gt 0
$touchesResources = ($changedPaths | Where-Object { Is-ResourcePath $_ }).Count -gt 0

$state = @{}
if (Test-Path $stateFile) {
    $obj = Get-Content -Raw $stateFile | ConvertFrom-Json
    foreach ($prop in $obj.PSObject.Properties) {
        $state[$prop.Name] = $prop.Value
    }
}

if (-not $state.ContainsKey("sessionStartHead")) {
    $state["sessionStartHead"] = (& git -C $repoRoot rev-parse HEAD).Trim()
}

$state["touchedCode"] = [bool]($state["touchedCode"] -or $touchesCode)
$state["touchedResources"] = [bool]($state["touchedResources"] -or $touchesResources)
$state["lastChangedFiles"] = $changedPaths
$state["lastUpdatedUtc"] = [DateTime]::UtcNow.ToString("o")

($state | ConvertTo-Json -Depth 8) | Set-Content -NoNewline -Encoding UTF8 $stateFile
Write-Output '{ "permission": "allow" }'
