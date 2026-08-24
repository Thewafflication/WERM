[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('x86', 'x64')]
    [string] $Architecture,

    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?$')]
    [string] $Version,

    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })]
    [string] $PackagePath,

    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })]
    [string] $WpmPath,

    [string] $SourceRevision = $env:GITHUB_SHA,

    [string] $OutputDirectory = 'out\integration-results'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$resolvedPackage = [IO.Path]::GetFullPath($PackagePath)
$retainedArchiveName = [IO.Path]::GetFileNameWithoutExtension($resolvedPackage)
$installDirectory = Join-Path $env:ProgramFiles "WERM\$Version\$Architecture"
$expectedHome = $installDirectory
$resultDirectory = if ([IO.Path]::IsPathRooted($OutputDirectory)) {
    Join-Path ([IO.Path]::GetFullPath($OutputDirectory)) $Architecture
} else {
    Join-Path ([IO.Path]::GetFullPath(
        (Join-Path $repositoryRoot $OutputDirectory))) $Architecture
}
$resultPath = Join-Path $resultDirectory 'tc-0028.json'
$sentinelDirectory = Join-Path $resultDirectory 'external-state'
$sentinelPath = Join-Path $sentinelDirectory 'preserve-me.txt'
$environmentKeyPath = 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment'
$installed = $false
$launched = $false
$installOutput = @()
$removeOutput = @()

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'TC-0028 requires the disposable runner to have administrator rights.'
}
if (Test-Path -LiteralPath $installDirectory) {
    throw "TC-0028 requires a clean WERM destination: $installDirectory"
}
New-Item -ItemType Directory -Force -Path $resultDirectory, $sentinelDirectory |
    Out-Null
Set-Content -LiteralPath $sentinelPath -Value 'TC-0028 external state' `
    -Encoding ascii

if ([string]::IsNullOrWhiteSpace($SourceRevision)) {
    $SourceRevision = (& git -C $repositoryRoot rev-parse HEAD).Trim()
}

try {
    $started = [DateTimeOffset]::UtcNow
    $installOutput = @(& $WpmPath install $resolvedPackage `
        --allow-unsigned 2>&1 | ForEach-Object { [string] $_ })
    if ($LASTEXITCODE -ne 0) {
        throw "WPM install failed: $($installOutput -join [Environment]::NewLine)"
    }
    $installed = $true

    $requiredFiles = @(
        'Werm.exe',
        'Werm.exe.config',
        'Werm.Core.dll',
        'Werm.Data.dll',
        'Werm.Printing.dll',
        'database\migrations\0001-initial-schema.sql',
        'drivers\sqlite3odbc.dll',
        'drivers\dependency-manifest.json',
        'tools\install-werm-database.wsh',
        'tools\Install-WermDatabase.ps1',
        'tools\Install-SqliteOdbcDriver.ps1',
        'README.md')
    foreach ($relativePath in $requiredFiles) {
        $installedPath = Join-Path $installDirectory $relativePath
        if (-not (Test-Path -LiteralPath $installedPath -PathType Leaf)) {
            throw "Installed WPM payload is incomplete: $installedPath"
        }
    }

    $registrationScript = Join-Path $installDirectory `
        'tools\Install-SqliteOdbcDriver.ps1'
    $driverPath = Join-Path $installDirectory 'drivers\sqlite3odbc.dll'
    & $registrationScript -Action Verify -Architecture $Architecture `
        -DriverPath $driverPath -WermVersion $Version | Out-Host

    $environmentKey = [Microsoft.Win32.Registry]::LocalMachine.OpenSubKey(
        $environmentKeyPath, $false)
    try {
        $registeredHome = [Environment]::ExpandEnvironmentVariables(
            [string] $environmentKey.GetValue('WERM_HOME', ''))
    }
    finally {
        $environmentKey.Dispose()
    }
    if (-not $registeredHome.Equals(
            $expectedHome,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "WERM_HOME is $registeredHome instead of $expectedHome."
    }

    $wermProcess = Start-Process -FilePath (Join-Path $installDirectory 'Werm.exe') `
        -WindowStyle Hidden -PassThru
    try {
        Start-Sleep -Seconds 3
        if ($wermProcess.HasExited) {
            throw "Installed WERM exited during launch verification with code " +
                "$($wermProcess.ExitCode)."
        }
        $launched = $true
    }
    finally {
        if (-not $wermProcess.HasExited) {
            $wermProcess.Kill()
            [void] $wermProcess.WaitForExit(10000)
        }
        $wermProcess.Dispose()
    }

    $removeOutput = @(& $WpmPath remove $retainedArchiveName 2>&1 |
        ForEach-Object { [string] $_ })
    if ($LASTEXITCODE -ne 0) {
        throw "WPM remove failed: $($removeOutput -join [Environment]::NewLine)"
    }
    $installed = $false

    if (Test-Path -LiteralPath $installDirectory) {
        throw 'WPM removal left the versioned WERM directory behind.'
    }
    if (-not (Test-Path -LiteralPath $sentinelPath -PathType Leaf)) {
        throw 'WPM removal deleted external test state.'
    }

    $environmentKey = [Microsoft.Win32.Registry]::LocalMachine.OpenSubKey(
        $environmentKeyPath, $false)
    try {
        $remainingHome = [string] $environmentKey.GetValue('WERM_HOME', '')
    }
    finally {
        $environmentKey.Dispose()
    }
    if (-not [string]::IsNullOrWhiteSpace($remainingHome)) {
        throw "WPM removal left WERM_HOME set to $remainingHome."
    }

    $finished = [DateTimeOffset]::UtcNow
    $result = [ordered]@{
        id = 'TC-0028'
        status = 'Pass'
        sourceRevision = $SourceRevision
        architecture = $Architecture
        references = @(
            'ADR-0008',
            'ADR-0009',
            'REQ-0001',
            'REQ-0006',
            'REQ-0020',
            'REQ-0021')
        version = $Version
        package = $resolvedPackage
        packageSha256 = (Get-FileHash -LiteralPath $resolvedPackage `
            -Algorithm SHA256).Hash.ToLowerInvariant()
        startedAtUtc = $started.ToString('O')
        finishedAtUtc = $finished.ToString('O')
        installedFileCount = $requiredFiles.Count
        launchSurvivedSeconds = 3
        externalStatePreserved = $true
        installOutput = $installOutput
        removeOutput = $removeOutput
    }
    $result | ConvertTo-Json -Depth 6 | Set-Content `
        -LiteralPath $resultPath -Encoding utf8
}
finally {
    if ($installed) {
        & $WpmPath remove $retainedArchiveName 2>&1 | Out-Host
    }
}

if (-not $launched) {
    throw 'Installed WERM launch was not verified.'
}
Write-Host "TC-0028 passed for $Architecture."
Get-Item -LiteralPath $resultPath
