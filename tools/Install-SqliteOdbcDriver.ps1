[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Install', 'Remove', 'Verify')]
    [string] $Action,

    [Parameter(Mandatory)]
    [ValidateSet('x86', 'x64')]
    [string] $Architecture,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $DriverPath,

    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?$')]
    [string] $WermVersion = '0.1.0'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$driverVersion = '0.99991'
$driverName = "WERM $WermVersion SQLite3 ODBC Driver $driverVersion ($Architecture)"
$resolvedDriverPath = [IO.Path]::GetFullPath($DriverPath)
$driverKeyPath = "SOFTWARE\ODBC\ODBCINST.INI\$driverName"
$driversKeyPath = 'SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers'
$registryView = if ($Architecture -eq 'x64') {
    [Microsoft.Win32.RegistryView]::Registry64
} else {
    [Microsoft.Win32.RegistryView]::Registry32
}

if ($Action -ne 'Remove' -and
        -not (Test-Path -LiteralPath $resolvedDriverPath -PathType Leaf)) {
    throw "SQLite ODBC driver was not found: $resolvedDriverPath"
}

$baseKey = [Microsoft.Win32.RegistryKey]::OpenBaseKey(
    [Microsoft.Win32.RegistryHive]::LocalMachine,
    $registryView)
try {
    if ($Action -eq 'Install') {
        $driverKey = $baseKey.CreateSubKey($driverKeyPath, $true)
        $driversKey = $baseKey.CreateSubKey($driversKeyPath, $true)
        try {
            $driverKey.SetValue('Driver', $resolvedDriverPath,
                [Microsoft.Win32.RegistryValueKind]::String)
            $driverKey.SetValue('Setup', $resolvedDriverPath,
                [Microsoft.Win32.RegistryValueKind]::String)
            $driverKey.SetValue('UsageCount', 1,
                [Microsoft.Win32.RegistryValueKind]::DWord)
            $driversKey.SetValue($driverName, 'Installed',
                [Microsoft.Win32.RegistryValueKind]::String)
        }
        finally {
            $driverKey.Dispose()
            $driversKey.Dispose()
        }
    }
    elseif ($Action -eq 'Remove') {
        $driverKey = $baseKey.OpenSubKey($driverKeyPath, $false)
        if ($null -eq $driverKey) {
            Write-Host "SQLite ODBC driver is not registered: $driverName"
            return
        }
        try {
            $registeredPath = [IO.Path]::GetFullPath(
                [string] $driverKey.GetValue('Driver', ''))
        }
        finally {
            $driverKey.Dispose()
        }
        if (-not $registeredPath.Equals(
                $resolvedDriverPath,
                [StringComparison]::OrdinalIgnoreCase)) {
            throw "Refusing to remove $driverName because its registered " +
                "path is $registeredPath, not $resolvedDriverPath."
        }
        $baseKey.DeleteSubKeyTree($driverKeyPath, $false)
        $driversKey = $baseKey.OpenSubKey($driversKeyPath, $true)
        if ($null -ne $driversKey) {
            try {
                $driversKey.DeleteValue($driverName, $false)
            }
            finally {
                $driversKey.Dispose()
            }
        }
    }

    if ($Action -ne 'Remove') {
        $driverKey = $baseKey.OpenSubKey($driverKeyPath, $false)
        $driversKey = $baseKey.OpenSubKey($driversKeyPath, $false)
        try {
            if ($null -eq $driverKey -or $null -eq $driversKey) {
                throw "SQLite ODBC driver registration is incomplete: $driverName"
            }
            $registeredPath = [string] $driverKey.GetValue('Driver', '')
            $registeredState = [string] $driversKey.GetValue($driverName, '')
            if (-not $registeredPath.Equals(
                    $resolvedDriverPath,
                    [StringComparison]::OrdinalIgnoreCase) -or
                    $registeredState -ne 'Installed') {
                throw "SQLite ODBC driver registration verification failed: $driverName"
            }
        }
        finally {
            if ($null -ne $driverKey) { $driverKey.Dispose() }
            if ($null -ne $driversKey) { $driversKey.Dispose() }
        }
    }
}
finally {
    $baseKey.Dispose()
}

Write-Host "$Action passed for $driverName"
Write-Output $driverName
