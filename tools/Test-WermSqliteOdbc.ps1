[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('x86', 'x64')]
    [string] $Architecture,

    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })]
    [string] $WshPath,

    [string] $SourceRevision = $env:GITHUB_SHA,

    [string] $OutputDirectory = 'out\integration-results'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'out'))
$testRoot = Join-Path $outRoot "integration-results\$Architecture"
$resolvedOutputRoot = if ([IO.Path]::IsPathRooted($OutputDirectory)) {
    [IO.Path]::GetFullPath($OutputDirectory)
} else {
    [IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputDirectory))
}
$resultDirectory = Join-Path $resolvedOutputRoot $Architecture
$resultPath = Join-Path $resultDirectory 'tc-0025.json'
$driverDirectory = Join-Path $outRoot "dependencies\sqlite-odbc\$Architecture"
$driverPath = Join-Path $driverDirectory 'sqlite3odbc.dll'
$driverManifestPath = Join-Path $driverDirectory 'dependency-manifest.json'
$installerPath = Join-Path $repositoryRoot 'tools\install-werm-database.wsh'
$odbcSqlPath = Join-Path $repositoryRoot 'tools\Invoke-WermOdbcSql.ps1'
$registrationPath = Join-Path $repositoryRoot 'tools\Install-SqliteOdbcDriver.ps1'
$windowsPowerShell = if ($Architecture -eq 'x86') {
    Join-Path $env:WINDIR 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'
} else {
    Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe'
}

foreach ($path in @(
        $WshPath,
        $driverPath,
        $driverManifestPath,
        $installerPath,
        $odbcSqlPath,
        $registrationPath,
        $windowsPowerShell)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "TC-0025 prerequisite is missing: $path"
    }
}
foreach ($path in @($testRoot, $resultDirectory)) {
    $resolved = [IO.Path]::GetFullPath($path)
    if (-not $resolved.StartsWith(
            $outRoot.TrimEnd('\') + '\',
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing an integration-test path outside $outRoot`: $resolved"
    }
}

if (Test-Path -LiteralPath $testRoot) {
    Remove-Item -LiteralPath $testRoot -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $testRoot, $resultDirectory |
    Out-Null

function Invoke-WermArchitecturePowerShell {
    param(
        [Parameter(Mandatory)]
        [string] $ScriptPath,

        [Parameter(Mandatory)]
        [string[]] $Arguments
    )

    $output = & $windowsPowerShell -NoLogo -NoProfile -NonInteractive `
        -ExecutionPolicy Bypass -File $ScriptPath @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Architecture-matched PowerShell failed with exit " +
            "$LASTEXITCODE`: $($output -join [Environment]::NewLine)"
    }
    return @($output)
}

function Invoke-WermInstallerCase {
    param(
        [Parameter(Mandatory)]
        [string] $Home,

        [Parameter(Mandatory)]
        [string] $DatabasePath,

        [Parameter(Mandatory)]
        [int] $ExpectedExit,

        [Parameter(Mandatory)]
        [string] $Name
    )

    $priorHome = $env:WERM_HOME
    try {
        $env:WERM_HOME = $Home
        $started = [DateTimeOffset]::UtcNow
        $output = & $WshPath $installerPath driver $DatabasePath `
            $script:driverName 2>&1
        $exitCode = $LASTEXITCODE
        $finished = [DateTimeOffset]::UtcNow
    }
    finally {
        if ($null -eq $priorHome) {
            Remove-Item Env:WERM_HOME -ErrorAction SilentlyContinue
        }
        else {
            $env:WERM_HOME = $priorHome
        }
    }
    if ($exitCode -ne $ExpectedExit) {
        throw "$Name returned $exitCode instead of $ExpectedExit. " +
            ($output -join [Environment]::NewLine)
    }
    if (($output -join [Environment]::NewLine) -notmatch
            "Process architecture: $($Architecture.ToUpperInvariant())") {
        throw "$Name did not run the $Architecture PowerShell worker."
    }
    return [ordered]@{
        name = $Name
        startedAtUtc = $started.ToString('O')
        finishedAtUtc = $finished.ToString('O')
        exitCode = $exitCode
        output = @($output | ForEach-Object { [string] $_ })
    }
}

if ([string]::IsNullOrWhiteSpace($SourceRevision)) {
    $SourceRevision = (& git -C $repositoryRoot rev-parse HEAD).Trim()
}
$driverManifest = Get-Content -Raw -LiteralPath $driverManifestPath |
    ConvertFrom-Json
$wshVersionOutput = @(& $WshPath --version 2>&1 | ForEach-Object { [string] $_ })
if ($LASTEXITCODE -ne 0 -or ($wshVersionOutput -join "`n") -notmatch 'Version 1\.4\.0') {
    throw 'TC-0025 requires Waughtal Shell 1.4.0.'
}

$script:driverName = & $registrationPath -Action Install `
    -Architecture $Architecture -DriverPath $driverPath -WermVersion '0.1.0'
$script:driverName = [string]($script:driverName | Select-Object -Last 1)
$cases = New-Object 'System.Collections.Generic.List[object]'
try {
    $currentDatabase = Join-Path $testRoot 'current.db'
    $cases.Add((Invoke-WermInstallerCase -Home $repositoryRoot `
        -DatabasePath $currentDatabase -ExpectedExit 0 -Name 'missing database'))
    if (-not (Test-Path -LiteralPath $currentDatabase -PathType Leaf)) {
        throw 'The missing-database case did not create a SQLite file.'
    }

    $tableCount = Invoke-WermArchitecturePowerShell -ScriptPath $odbcSqlPath `
        -Arguments @(
            '-DatabasePath', $currentDatabase,
            '-DriverName', $script:driverName,
            '-CommandText', "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name IN ('WermSchemaVersion','MaintenanceCredential','Product','Customer','CustomerProductPrice','ProductAuditEvent','ProductAuditChange')",
            '-Scalar')
    if ([int]($tableCount | Select-Object -Last 1) -ne 7) {
        throw 'The real ODBC schema inspection did not find seven WERM tables.'
    }
    $triggerCount = Invoke-WermArchitecturePowerShell -ScriptPath $odbcSqlPath `
        -Arguments @(
            '-DatabasePath', $currentDatabase,
            '-DriverName', $script:driverName,
            '-CommandText', "SELECT COUNT(*) FROM sqlite_master WHERE type = 'trigger' AND name LIKE 'TR_ProductAudit%'",
            '-Scalar')
    if ([int]($triggerCount | Select-Object -Last 1) -ne 4) {
        throw 'The real ODBC schema inspection did not find four audit triggers.'
    }
    $version = Invoke-WermArchitecturePowerShell -ScriptPath $odbcSqlPath `
        -Arguments @(
            '-DatabasePath', $currentDatabase,
            '-DriverName', $script:driverName,
            '-CommandText', 'SELECT MAX(Version) FROM WermSchemaVersion',
            '-Scalar')
    if ([int]($version | Select-Object -Last 1) -ne 1) {
        throw 'The real ODBC schema inspection did not find schema version 1.'
    }

    $currentHash = (Get-FileHash -LiteralPath $currentDatabase `
        -Algorithm SHA256).Hash.ToLowerInvariant()
    $cases.Add((Invoke-WermInstallerCase -Home $repositoryRoot `
        -DatabasePath $currentDatabase -ExpectedExit 0 -Name 'current database'))
    $repeatHash = (Get-FileHash -LiteralPath $currentDatabase `
        -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($repeatHash -ne $currentHash) {
        throw 'Repeated installation changed the current database.'
    }

    $unrecognizedDatabase = Join-Path $testRoot 'unrecognized.db'
    [void](Invoke-WermArchitecturePowerShell -ScriptPath $odbcSqlPath `
        -Arguments @(
            '-DatabasePath', $unrecognizedDatabase,
            '-DriverName', $script:driverName,
            '-CommandText', 'CREATE TABLE ForeignTable (Value INTEGER NOT NULL)'))
    $unrecognizedHash = (Get-FileHash -LiteralPath $unrecognizedDatabase `
        -Algorithm SHA256).Hash.ToLowerInvariant()
    $cases.Add((Invoke-WermInstallerCase -Home $repositoryRoot `
        -DatabasePath $unrecognizedDatabase -ExpectedExit 4 `
        -Name 'unrecognized database'))
    if ((Get-FileHash -LiteralPath $unrecognizedDatabase -Algorithm SHA256).
            Hash.ToLowerInvariant() -ne $unrecognizedHash) {
        throw 'The rejected unrecognized database was modified.'
    }

    $newerDatabase = Join-Path $testRoot 'newer.db'
    Copy-Item -LiteralPath $currentDatabase -Destination $newerDatabase
    [void](Invoke-WermArchitecturePowerShell -ScriptPath $odbcSqlPath `
        -Arguments @(
            '-DatabasePath', $newerDatabase,
            '-DriverName', $script:driverName,
            '-CommandText', "INSERT INTO WermSchemaVersion (Version, Migration, AppliedAtUtc) VALUES (2, 'future-test', '2026-08-23T00:00:00Z')"))
    $newerHash = (Get-FileHash -LiteralPath $newerDatabase `
        -Algorithm SHA256).Hash.ToLowerInvariant()
    $cases.Add((Invoke-WermInstallerCase -Home $repositoryRoot `
        -DatabasePath $newerDatabase -ExpectedExit 4 -Name 'newer database'))
    if ((Get-FileHash -LiteralPath $newerDatabase -Algorithm SHA256).
            Hash.ToLowerInvariant() -ne $newerHash) {
        throw 'The rejected newer database was modified.'
    }

    $failureHome = Join-Path $testRoot 'failure-home'
    New-Item -ItemType Directory -Force -Path `
        (Join-Path $failureHome 'tools'),
        (Join-Path $failureHome 'database\migrations') | Out-Null
    Copy-Item -LiteralPath (Join-Path $repositoryRoot `
        'tools\Install-WermDatabase.ps1') -Destination (Join-Path $failureHome 'tools')
    $failureMigration = Join-Path $failureHome `
        'database\migrations\0001-initial-schema.sql'
    Copy-Item -LiteralPath (Join-Path $repositoryRoot `
        'database\migrations\0001-initial-schema.sql') -Destination $failureMigration
    Add-Content -LiteralPath $failureMigration -Encoding utf8 `
        -Value "`n-- WERM-BATCH`nCREATE TABLE DeliberateFailure (`n"
    $failedDatabase = Join-Path $testRoot 'failed-migration.db'
    $cases.Add((Invoke-WermInstallerCase -Home $failureHome `
        -DatabasePath $failedDatabase -ExpectedExit 4 -Name 'failed migration'))
    if (Test-Path -LiteralPath $failedDatabase) {
        throw 'The failed migration left a new database file behind.'
    }

    $result = [ordered]@{
        id = 'TC-0025'
        status = 'Pass'
        sourceRevision = $SourceRevision
        architecture = $Architecture
        references = @(
            'ADR-0006',
            'ADR-0007',
            'REQ-0002',
            'REQ-0019',
            'REQ-0020',
            'REQ-0021')
        driverName = $script:driverName
        driverManifest = $driverManifest
        wshSha256 = (Get-FileHash -LiteralPath $WshPath `
            -Algorithm SHA256).Hash.ToLowerInvariant()
        wshVersion = $wshVersionOutput
        databaseSha256 = $currentHash
        tableCount = 7
        auditTriggerCount = 4
        schemaVersion = 1
        cases = $cases.ToArray()
    }
    $result | ConvertTo-Json -Depth 8 | Set-Content `
        -LiteralPath $resultPath -Encoding utf8
}
finally {
    & $registrationPath -Action Remove -Architecture $Architecture `
        -DriverPath $driverPath -WermVersion '0.1.0' | Out-Host
}

Write-Host "TC-0025 passed for $Architecture."
Get-Item -LiteralPath $resultPath
