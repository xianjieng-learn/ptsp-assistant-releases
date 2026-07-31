param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+(\.\d+){1,3}$')]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path $_ -PathType Leaf })]
    [string]$InstallerPath,

    [string]$Repository = 'xianjieng-learn/ptsp-assistant-releases',
    [switch]$Mandatory
)

$ErrorActionPreference = 'Stop'

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Perintah '$Name' tidak ditemukan. Pasang GitHub CLI dan Git terlebih dahulu."
    }
}

Require-Command gh
Require-Command git

gh auth status | Out-Null

$installer = Get-Item (Resolve-Path $InstallerPath)
$expectedName = "PTSP-Assistant-Setup-v$Version-FULL-SHARING.exe"
if ($installer.Name -ne $expectedName) {
    throw "Nama installer harus '$expectedName', tetapi ditemukan '$($installer.Name)'."
}

$hash = (Get-FileHash $installer.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
$size = $installer.Length
$tag = "v$Version"
$notesPath = Join-Path $PSScriptRoot "..\releases\$tag.md"
$manifestPath = Join-Path $PSScriptRoot "..\stable\latest.json"

if (-not (Test-Path $notesPath)) {
    throw "Catatan rilis tidak ditemukan: $notesPath"
}
if (-not (Test-Path $manifestPath)) {
    throw "Manifest stable tidak ditemukan: $manifestPath"
}

$releaseExists = $true
try {
    gh release view $tag --repo $Repository | Out-Null
} catch {
    $releaseExists = $false
}

if ($releaseExists) {
    gh release upload $tag $installer.FullName --repo $Repository --clobber
} else {
    gh release create $tag $installer.FullName --repo $Repository --title "PTSP Assistant $tag" --notes-file $notesPath
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$manifest.suiteVersion = $Version
$manifest.extensionVersion = $Version
$manifest.publishedAt = (Get-Date).ToString('o')
$manifest.mandatory = [bool]$Mandatory
$manifest.installer.name = $expectedName
$manifest.installer.url = "https://github.com/$Repository/releases/download/$tag/$expectedName"
$manifest.installer.sha256 = $hash
$manifest.installer.size = $size
$manifest.components.extension = $Version
$manifest | ConvertTo-Json -Depth 20 | Set-Content $manifestPath -Encoding UTF8

Push-Location (Join-Path $PSScriptRoot '..')
try {
    git add stable/latest.json "releases/$tag.md"
    git commit -m "Publish PTSP Assistant $tag"
    git push
} finally {
    Pop-Location
}

Write-Host "Release $tag berhasil dipublikasikan." -ForegroundColor Green
Write-Host "SHA-256: $hash"
Write-Host "Size: $size bytes"
