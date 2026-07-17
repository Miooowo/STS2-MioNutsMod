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

function Deny-And-Exit {
    param(
        [string]$UserMessage,
        [string]$AgentMessage
    )
    $payload = @{
        permission = "deny"
        user_message = $UserMessage
        agent_message = $AgentMessage
    } | ConvertTo-Json -Compress
    Write-Output $payload
    exit 2
}

$repoRoot = Get-RepoRoot
$stateFile = Join-Path $PSScriptRoot ".session-state.json"

$state = @{}
if (Test-Path $stateFile) {
    $obj = Get-Content -Raw $stateFile | ConvertFrom-Json
    foreach ($prop in $obj.PSObject.Properties) {
        $state[$prop.Name] = $prop.Value
    }
}

$changedPaths = Get-ChangedPaths -RepoRoot $repoRoot
$touchesCode = ($changedPaths | Where-Object { Is-CodePath $_ }).Count -gt 0
$touchesResources = ($changedPaths | Where-Object { Is-ResourcePath $_ }).Count -gt 0
$sessionTouchedCode = [bool]($touchesCode -or ($state["touchedCode"] -eq $true))
$sessionTouchedResources = [bool]($touchesResources -or ($state["touchedResources"] -eq $true))

if (-not $sessionTouchedCode -and -not $sessionTouchedResources) {
    Write-Output '{ "permission": "allow" }'
    exit 0
}

if ($sessionTouchedResources) {
    & dotnet publish | Out-String | Write-Host
    if ($LASTEXITCODE -ne 0) {
        Deny-And-Exit "检测到本地化/图片等资源改动，dotnet publish 失败，请先修复后再结束会话。" "Workflow hook blocked session end because dotnet publish failed."
    }
} else {
    & dotnet build | Out-String | Write-Host
    if ($LASTEXITCODE -ne 0) {
        Deny-And-Exit "检测到代码改动，dotnet build 失败，请先修复后再结束会话。" "Workflow hook blocked session end because dotnet build failed."
    }
}

$sessionStartHead = $state["sessionStartHead"]
if (-not $sessionStartHead) {
    $sessionStartHead = (& git -C $repoRoot rev-parse HEAD).Trim()
}
$currentHead = (& git -C $repoRoot rev-parse HEAD).Trim()
if ($currentHead -eq $sessionStartHead) {
    Deny-And-Exit "本次会话检测到代码/资源改动，但尚未产生新提交。请先执行一次 git commit。" "Workflow hook requires at least one commit after code/resource modifications."
}

$committedFiles = (& git -C $repoRoot diff --name-only "$sessionStartHead..$currentHead") | ForEach-Object { $_.Replace("\", "/") }
$changelogTouched = ($committedFiles | Where-Object { $_ -eq "CHANGELOG.md" }).Count -gt 0
$commitMessages = (& git -C $repoRoot log --format=%B "$sessionStartHead..$currentHead") -join "`n"
$skipChangelog = $commitMessages -match "\[no-changelog\]"

if (-not $changelogTouched -and -not $skipChangelog) {
    Deny-And-Exit "请更新 CHANGELOG.md（若本次改动确实无需记录，请在提交信息中添加 [no-changelog] 作为例外标记）。" "Workflow hook requires changelog update for notable code/resource changes."
}

if (Test-Path $stateFile) {
    Remove-Item $stateFile -Force
}

Write-Output '{ "permission": "allow" }'
