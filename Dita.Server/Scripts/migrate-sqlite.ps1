#Requires -Version 7
<#
.SYNOPSIS
	Adds a new EF Core migration for the SQLite provider and applies it to the database.

.DESCRIPTION
	Runs 'dotnet ef migrations add' targeting SqliteAppDbContext, placing the generated files
	in Storage/Migrations/SQLite/, then immediately applies all pending migrations via 'dotnet ef database update'.

.PARAMETER MigrationName
	The name of the migration to create. Defaults to a timestamp-based name if not supplied.

.PARAMETER ConnectionString
	Optional SQLite connection string. If not supplied the DITA_SQLITE_CS environment variable
	is used, or a local fallback of 'Data Source=dita_dev.db'.

.EXAMPLE
	.\migrate-sqlite.ps1
	.\migrate-sqlite.ps1 -MigrationName AddUserTable
	.\migrate-sqlite.ps1 -MigrationName AddUserTable -ConnectionString "Data Source=C:\data\dita.db"
#>
[CmdletBinding()]
param(
	[string] $MigrationName = ("Migration_" + (Get-Date -Format "yyyyMMdd_HHmmss")),
	[string] $ConnectionString = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── Resolve project root ─────────────────────────────────────────────────────
$scriptDir   = $PSScriptRoot
$projectRoot = Split-Path $scriptDir -Parent   # Dita.Server/
$outputDir   = "Storage/Migrations/SQLite"

Write-Host ""
Write-Host "=== Dita — SQLite Migration ===" -ForegroundColor Cyan
Write-Host "  Project   : $projectRoot"
Write-Host "  Migration : $MigrationName"
Write-Host "  Output    : $outputDir"
Write-Host ""

# ── Set connection string env var if provided ────────────────────────────────
if ($ConnectionString -ne "") {
	$env:DITA_SQLITE_CS = $ConnectionString
	Write-Host "  Using supplied connection string." -ForegroundColor DarkGray
} elseif ($env:DITA_SQLITE_CS) {
	Write-Host "  Using DITA_SQLITE_CS from environment." -ForegroundColor DarkGray
} else {
	$env:DITA_SQLITE_CS = "Data Source=dita_dev.db"
	Write-Host "  No connection string set — using fallback: $env:DITA_SQLITE_CS" -ForegroundColor Yellow
}

# ── Check dotnet ef is available ─────────────────────────────────────────────
if (-not (Get-Command "dotnet-ef" -ErrorAction SilentlyContinue) -and
	-not (dotnet ef --version 2>$null)) {
	Write-Error "dotnet-ef tool is not installed. Run: dotnet tool install --global dotnet-ef"
	exit 1
}

# ── Add migration ─────────────────────────────────────────────────────────────
Write-Host "Step 1/2 — Adding migration '$MigrationName'..." -ForegroundColor Green
Push-Location $projectRoot
try {
	dotnet ef migrations add $MigrationName `
		--context SqliteAppDbContext `
		--output-dir $outputDir `
		--verbose
	if ($LASTEXITCODE -ne 0) { throw "dotnet ef migrations add failed (exit $LASTEXITCODE)." }
} finally {
	Pop-Location
}

# ── Apply migrations ──────────────────────────────────────────────────────────
Write-Host ""
Write-Host "Step 2/2 — Applying pending migrations..." -ForegroundColor Green
Push-Location $projectRoot
try {
	dotnet ef database update `
		--context SqliteAppDbContext `
		--verbose
	if ($LASTEXITCODE -ne 0) { throw "dotnet ef database update failed (exit $LASTEXITCODE)." }
} finally {
	Pop-Location
}

Write-Host ""
Write-Host "Done. SQLite migration '$MigrationName' applied successfully." -ForegroundColor Cyan
