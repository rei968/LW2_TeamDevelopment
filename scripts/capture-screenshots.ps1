# CineSphere — Automated Headless Browser Screenshot Capture Script
# Usage: powershell -ExecutionPolicy Bypass -File scripts/capture-screenshots.ps1

$ErrorActionPreference = "Stop"

$workspace = Split-Path -Parent $PSScriptRoot
$screenshotsDir = Join-Path $workspace "docs\screenshots"
if (-not (Test-Path $screenshotsDir)) {
  New-Item -ItemType Directory -Force -Path $screenshotsDir | Out-Null
}

$chromePaths = @(
  "C:\Program Files\Google\Chrome\Application\chrome.exe",
  "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
  "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
  "C:\Program Files\Microsoft\Edge\Application\msedge.exe"
)

$browserPath = $chromePaths | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $browserPath) {
  Write-Error "Neither Google Chrome nor Microsoft Edge was found on the system."
  exit 1
}

Write-Host "[CineSphere QA] Using browser: $browserPath"

$tempUserDir = Join-Path ([System.IO.Path]::GetTempPath()) "cinesphere_headless_profile"
if (-not (Test-Path $tempUserDir)) {
  New-Item -ItemType Directory -Force -Path $tempUserDir | Out-Null
}

# 1. Capture CineSphere Catalog UI (index.html)
$catalogTarget = Join-Path $screenshotsDir "03_cinesphere_ui_catalog.png"
$catalogUrl = "file:///" + ($workspace -replace "\\", "/") + "/index.html"

Write-Host "[CineSphere QA] Capturing Catalog UI -> $catalogTarget"
Start-Process -FilePath $browserPath -ArgumentList @(
  "--headless=new",
  "--disable-gpu",
  "--no-sandbox",
  "--user-data-dir=$tempUserDir",
  "--window-size=1280,900",
  "--screenshot=$catalogTarget",
  $catalogUrl
) -Wait -NoNewWindow

# 2. Capture CineSphere QA Test Suite Runner (tests.html)
$testsTarget = Join-Path $screenshotsDir "09_qa_testsuite_full_pass.png"
$testsUrl = "file:///" + ($workspace -replace "\\", "/") + "/tests.html"

Write-Host "[CineSphere QA] Capturing Test Suite Results -> $testsTarget"
Start-Process -FilePath $browserPath -ArgumentList @(
  "--headless=new",
  "--disable-gpu",
  "--no-sandbox",
  "--user-data-dir=$tempUserDir",
  "--window-size=1280,1050",
  "--virtual-time-budget=3000",
  "--screenshot=$testsTarget",
  $testsUrl
) -Wait -NoNewWindow

Write-Host "[CineSphere QA] Verification:"
Get-Item $catalogTarget, $testsTarget | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
Write-Host "[CineSphere QA] Screenshots successfully captured!"
