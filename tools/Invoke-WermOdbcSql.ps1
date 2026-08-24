[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $DatabasePath,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $DriverName,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $CommandText,

    [switch] $Scalar
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$builder = [System.Data.Odbc.OdbcConnectionStringBuilder]::new()
$builder['Driver'] = $DriverName
$builder['Database'] = [IO.Path]::GetFullPath($DatabasePath)
$connection = [System.Data.Odbc.OdbcConnection]::new($builder.ConnectionString)
try {
    $connection.Open()
    $foreignKeys = $connection.CreateCommand()
    try {
        $foreignKeys.CommandText = 'PRAGMA foreign_keys = ON'
        [void] $foreignKeys.ExecuteNonQuery()
    }
    finally {
        $foreignKeys.Dispose()
    }

    $command = $connection.CreateCommand()
    try {
        $command.CommandText = $CommandText
        $command.CommandTimeout = 30
        if ($Scalar) {
            Write-Output $command.ExecuteScalar()
        }
        else {
            Write-Output $command.ExecuteNonQuery()
        }
    }
    finally {
        $command.Dispose()
    }
}
finally {
    $connection.Dispose()
}
