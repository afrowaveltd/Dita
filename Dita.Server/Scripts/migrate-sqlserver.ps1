#Requires -Version 7
<#
.SYNOPSIS
	Adds a new EF Core migration for the SQL Server provider and applies it to the database.

.DESCRIPTION
	Runs 'dotnet ef migrations add' targeting SqlServerAppDbContext, placing the generated files
	in Storage/Migrations/SqlServer/, then immediately applies all pending migrations via 'dotnet ef database update'.

.PARAMETER MigrationName
	The name of the migration to create. Defaults to a timestamp-based name if not supplied.

.PARAMETER ConnectionString
	Optional SQL Server connection string. If not supplied the DITA_SQLSERVER_CS environment variable
	is used, or a LocalDB fallback.

.EXAMPLE
	.\migrate-sqlserver.ps1
	.\migrate-sqlserver.ps1 -MigrationName AddUserTable
	.\migrate-sqlserver.ps1 -MigrationName AddUserTable -ConnectionString "Server=myserver;Database=Dita;Trusted_Connection=True;"
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
$projectRoot = Split-Path $scriptDir -Parent
$outputDir   = "Storage/Migrations/SqlServer"

Write-Host ""
Write-Host "=== Dita — SQL Server Migration ===" -ForegroundColor Cyan
Write-Host "  Project   : $projectRoot"
Write-Host "  Migration : $MigrationName"
Write-Host "  Output    : $outputDir"
Write-Host ""

# ── Set connection string env var if provided ────────────────────────────────
if ($ConnectionString -ne "") {
	$env:DITA_SQLSERVER_CS = $ConnectionString
	Write-Host "  Using supplied connection string." -ForegroundColor DarkGray
} elseif ($env:DITA_SQLSERVER_CS) {
	Write-Host "  Using DITA_SQLSERVER_CS from environment." -ForegroundColor DarkGray
} else {
	$env:DITA_SQLSERVER_CS = "Server=(localdb)\mssqllocaldb;Database=DitaDev;Trusted_Connection=True;"
	Write-Host "  No connection string set — using fallback LocalDB: $env:DITA_SQLSERVER_CS" -ForegroundColor Yellow
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
		--context SqlServerAppDbContext `
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
		--context SqlServerAppDbContext `
		--verbose
	if ($LASTEXITCODE -ne 0) { throw "dotnet ef database update failed (exit $LASTEXITCODE)." }
} finally {
	Pop-Location
}

Write-Host ""
Write-Host "Done. SQL Server migration '$MigrationName' applied successfully." -ForegroundColor Cyan
