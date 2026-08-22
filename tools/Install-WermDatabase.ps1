[CmdletBinding(DefaultParameterSetName = 'Driver')]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $DatabasePath,

    [Parameter(Mandatory, ParameterSetName = 'Driver')]
    [ValidateNotNullOrEmpty()]
    [string] $DriverName,

    [Parameter(Mandatory, ParameterSetName = 'Dsn')]
    [ValidateNotNullOrEmpty()]
    [string] $Dsn
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$supportedSchemaVersion = 1
$migrationPath = Join-Path $PSScriptRoot `
    '..\database\migrations\0001-initial-schema.sql'
$resolvedMigrationPath = [IO.Path]::GetFullPath($migrationPath)
$resolvedDatabasePath = [IO.Path]::GetFullPath($DatabasePath)
$databaseDirectory = Split-Path -Parent $resolvedDatabasePath
$databaseExisted = Test-Path -LiteralPath $resolvedDatabasePath -PathType Leaf
$connection = $null

function Invoke-WermNonQuery {
    param(
        [Parameter(Mandatory)]
        [System.Data.Odbc.OdbcConnection] $Connection,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string] $CommandText,

        [System.Data.Odbc.OdbcTransaction] $Transaction
    )

    $command = $Connection.CreateCommand()
    try {
        $command.CommandText = $CommandText
        $command.CommandTimeout = 30
        if ($null -ne $Transaction) {
            $command.Transaction = $Transaction
        }
        [void] $command.ExecuteNonQuery()
    }
    finally {
        $command.Dispose()
    }
}

function Get-WermScalar {
    param(
        [Parameter(Mandatory)]
        [System.Data.Odbc.OdbcConnection] $Connection,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string] $CommandText
    )

    $command = $Connection.CreateCommand()
    try {
        $command.CommandText = $CommandText
        $command.CommandTimeout = 30
        return $command.ExecuteScalar()
    }
    finally {
        $command.Dispose()
    }
}

function Test-WermTable {
    param(
        [Parameter(Mandatory)]
        [System.Data.Odbc.OdbcConnection] $Connection,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string] $TableName
    )

    $escapedTableName = $TableName.Replace("'", "''")
    $count = Get-WermScalar -Connection $Connection -CommandText (
        "SELECT COUNT(*) FROM sqlite_master " +
        "WHERE type = 'table' AND name = '$escapedTableName'")
    return [int64] $count -eq 1
}

function Get-WermSchemaVersion {
    param(
        [Parameter(Mandatory)]
        [System.Data.Odbc.OdbcConnection] $Connection
    )

    if (-not (Test-WermTable -Connection $Connection `
            -TableName 'WermSchemaVersion')) {
        $userTableCount = Get-WermScalar -Connection $Connection -CommandText (
            "SELECT COUNT(*) FROM sqlite_master " +
            "WHERE type = 'table' AND name NOT LIKE 'sqlite_%'")
        if ([int64] $userTableCount -ne 0) {
            throw 'The existing database is non-empty but has no ' +
                'WermSchemaVersion table. It will not be modified.'
        }
        return 0
    }

    return [int] (Get-WermScalar -Connection $Connection -CommandText `
        'SELECT COALESCE(MAX(Version), 0) FROM WermSchemaVersion')
}

function Invoke-WermMigration {
    param(
        [Parameter(Mandatory)]
        [System.Data.Odbc.OdbcConnection] $Connection,

        [Parameter(Mandatory)]
        [ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })]
        [string] $Path
    )

    $migrationText = Get-Content -Raw -Encoding UTF8 -LiteralPath $Path
    $batches = [regex]::Split(
        $migrationText,
        '(?m)^\s*-- WERM-BATCH\s*$')
    $transaction = $Connection.BeginTransaction()

    try {
        foreach ($batch in $batches) {
            if (-not [string]::IsNullOrWhiteSpace($batch)) {
                Invoke-WermNonQuery -Connection $Connection `
                    -Transaction $transaction -CommandText $batch.Trim()
            }
        }
        $transaction.Commit()
    }
    catch {
        try {
            $transaction.Rollback()
        }
        catch {
            [Console]::Error.WriteLine(
                "ERROR: Migration rollback failed: $($_.Exception.Message)")
        }
        throw
    }
    finally {
        $transaction.Dispose()
    }
}

function Test-WermSchema {
    param(
        [Parameter(Mandatory)]
        [System.Data.Odbc.OdbcConnection] $Connection
    )

    $expectedTables = @(
        'WermSchemaVersion',
        'Product',
        'Customer',
        'CustomerProductPrice',
        'ProductAuditEvent',
        'ProductAuditChange'
    )

    foreach ($tableName in $expectedTables) {
        if (-not (Test-WermTable -Connection $Connection `
                -TableName $tableName)) {
            throw "Expected table is missing: $tableName"
        }
    }

    $foreignKeys = Get-WermScalar -Connection $Connection `
        -CommandText 'PRAGMA foreign_keys'
    if ([int] $foreignKeys -ne 1) {
        throw 'SQLite foreign-key enforcement is not enabled.'
    }

    $version = Get-WermSchemaVersion -Connection $Connection
    if ($version -ne $supportedSchemaVersion) {
        throw "Expected schema version $supportedSchemaVersion but found " +
            "$version."
    }
}

try {
    if (-not (Test-Path -LiteralPath $resolvedMigrationPath -PathType Leaf)) {
        throw "Migration file not found: $resolvedMigrationPath"
    }

    if (Test-Path -LiteralPath $resolvedDatabasePath -PathType Container) {
        throw "The database path identifies a directory: " +
            $resolvedDatabasePath
    }

    [void] [IO.Directory]::CreateDirectory($databaseDirectory)

    $connectionBuilder = [System.Data.Odbc.OdbcConnectionStringBuilder]::new()
    if ($PSCmdlet.ParameterSetName -eq 'Driver') {
        $connectionBuilder['Driver'] = $DriverName
        $connectionMode = "Driver $DriverName"
    }
    else {
        $connectionBuilder['DSN'] = $Dsn
        $connectionMode = "DSN $Dsn"
    }
    $connectionBuilder['Database'] = $resolvedDatabasePath

    Write-Host 'WERM database installation'
    Write-Host "  Database: $resolvedDatabasePath"
    Write-Host "  Process architecture: $([Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture)"
    Write-Host "  Connection mode: $connectionMode"

    $connection = [System.Data.Odbc.OdbcConnection]::new(
        $connectionBuilder.ConnectionString)
    $connection.ConnectionTimeout = 15
    $connection.Open()

    Invoke-WermNonQuery -Connection $connection `
        -CommandText 'PRAGMA foreign_keys = ON'
    Invoke-WermNonQuery -Connection $connection `
        -CommandText 'PRAGMA busy_timeout = 5000'

    $version = Get-WermSchemaVersion -Connection $connection
    if ($version -gt $supportedSchemaVersion) {
        throw "Database schema version $version is newer than this " +
            'installer supports.'
    }

    if ($version -lt 1) {
        Write-Host '  Applying migration 0001-initial-schema.sql...'
        Invoke-WermMigration -Connection $connection `
            -Path $resolvedMigrationPath
    }
    else {
        Write-Host '  Schema version 1 is already installed.'
    }

    Test-WermSchema -Connection $connection
    Write-Host '  Verification: passed'
    Write-Host 'WERM database schema version 1 is ready.'
    exit 0
}
catch {
    [Console]::Error.WriteLine("ERROR: $($_.Exception.Message)")

    if (-not $databaseExisted -and
            (Test-Path -LiteralPath $resolvedDatabasePath -PathType Leaf)) {
        try {
            if ($null -ne $connection) {
                $connection.Dispose()
                $connection = $null
            }
            Remove-Item -Force -LiteralPath $resolvedDatabasePath
            Write-Warning 'Removed the incomplete new database file.'
        }
        catch {
            [Console]::Error.WriteLine(
                "ERROR: Could not remove the incomplete database: " +
                $_.Exception.Message)
        }
    }

    exit 4
}
finally {
    if ($null -ne $connection) {
        $connection.Dispose()
    }
}
