#   cd <repo-root>
#   $env:DOCKER_IMAGE_PREFIX = "dockerhub-username"
#   $env:IMAGE_TAG = "latest" 
#   .\Docker\build-and-push.ps1

param(
    [string]$ImagePrefix = $env:DOCKER_IMAGE_PREFIX,
    [string]$Tag = $(if ($env:IMAGE_TAG) { $env:IMAGE_TAG } else { "latest" })
)

$ErrorActionPreference = "Stop"
$Root = if ($PSScriptRoot) { Split-Path $PSScriptRoot -Parent } else { (Get-Location).Path }

if (-not $ImagePrefix) {
    Write-Host "Set DOCKER_IMAGE_PREFIX (Docker Hub username)." -ForegroundColor Red
    exit 1
}

$services = @(
    @{ Name = "identity-service"; Dockerfile = "Identity/Verendar.Identity/Dockerfile" },
    @{ Name = "vehicle-service"; Dockerfile = "Vehicle/Verendar.Vehicle/Dockerfile" },
    @{ Name = "media-service"; Dockerfile = "Media/Verendar.Media/Dockerfile" },
    @{ Name = "notification-service"; Dockerfile = "Notification/Verendar.Notification/Dockerfile" },
    @{ Name = "ai-service"; Dockerfile = "Ai/Verendar.Ai/Dockerfile" }
)

Push-Location $Root
try {
    $maxPushRetries = 3
    foreach ($s in $services) {
        $img = "${ImagePrefix}/verendar-$($s.Name):${Tag}"
        docker build -t $img -f $s.Dockerfile .
        if ($LASTEXITCODE -ne 0) { throw "Build failed" }
        $pushOk = $false
        for ($r = 1; $r -le $maxPushRetries; $r++) {
            docker push $img
            if ($LASTEXITCODE -eq 0) { $pushOk = $true; break }
            if ($r -lt $maxPushRetries) { Write-Host "Push retry $r/$maxPushRetries in 10s..."; Start-Sleep -Seconds 10 }
        }
        if (-not $pushOk) { throw "Push failed: $img" }
    }
    Write-Host "Done." -ForegroundColor Green
} finally {
    Pop-Location
}
