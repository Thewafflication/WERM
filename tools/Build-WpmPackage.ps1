[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('x86', 'x64')]
    [string]$Architecture,

    [Parameter(Mandatory)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration,

    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?$')]
    [string]$Version,

    [string]$WpmPath,

    [string]$OutputDirectory = 'out\packages'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($WpmPath)) {
    $WpmPath = Get-Command wpm.exe -ErrorAction SilentlyContinue |
        Select-Object -First 1 -ExpandProperty Source
}
if ([string]::IsNullOrWhiteSpace($WpmPath) -or
    -not (Test-Path -LiteralPath $WpmPath -PathType Leaf)) {
    throw 'WPM was not found. Supply -WpmPath or install Waughtal Package Manager.'
}

$applicationDirectory = Join-Path $repositoryRoot `
    "out\bin\Werm.App\$Architecture\$Configuration"
$executable = Join-Path $applicationDirectory 'Werm.exe'
if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) {
    throw "Build WERM before packaging it. Missing: $executable"
}

$staging = Join-Path $repositoryRoot `
    "out\package-stage\$Architecture-$Configuration"
$payload = Join-Path $staging 'payload'
$metadataDirectory = Join-Path $staging '.wpm'
$resolvedOutputDirectory = if ([IO.Path]::IsPathRooted($OutputDirectory)) {
    [IO.Path]::GetFullPath($OutputDirectory)
} else {
    [IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputDirectory))
}

if (Test-Path -LiteralPath $staging) {
    Remove-Item -LiteralPath $staging -Recurse -Force
}
New-Item -ItemType Directory -Force -Path @(
    $payload,
    $metadataDirectory,
    $resolvedOutputDirectory,
    (Join-Path $payload 'database\migrations'),
    (Join-Path $payload 'tools'),
    (Join-Path $payload 'docs')
) | Out-Null

$applicationFiles = Get-ChildItem -LiteralPath $applicationDirectory -File |
    Where-Object { $_.Extension -in '.exe', '.config', '.dll', '.pdb' }
Copy-Item -LiteralPath $applicationFiles.FullName -Destination $payload
Copy-Item -Path (Join-Path $repositoryRoot 'database\migrations\*.sql') `
    -Destination (Join-Path $payload 'database\migrations')
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'tools\install-werm-database.wsh') `
    -Destination (Join-Path $payload 'tools')
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'tools\Install-WermDatabase.ps1') `
    -Destination (Join-Path $payload 'tools')
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'docs\database-installation.md') `
    -Destination (Join-Path $payload 'docs')
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'docs\word-template-contract.md') `
    -Destination (Join-Path $payload 'docs')
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'docs\workstation-configuration.md') `
    -Destination (Join-Path $payload 'docs')
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'docs\package-readme.md') `
    -Destination (Join-Path $payload 'README.md')

$sourceRevision = $env:GITHUB_SHA
if ([string]::IsNullOrWhiteSpace($sourceRevision)) {
    $sourceRevision = (& git -C $repositoryRoot rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw 'Git failed to identify the package source revision.'
    }
}

$isDebug = ($Configuration -eq 'Debug').ToString().ToLowerInvariant()
$metadata = @(
    'name=werm',
    "version=$Version",
    "arch=$Architecture",
    "debug=$isDebug",
    'description=Waughtal Enterprise Resource Management label printing application',
    'maintainer=Jordan Waughtal',
    'homepage=https://github.com/Thewafflication/WERM',
    'repository=https://github.com/Thewafflication/WERM',
    'license=',
    "source-revision=$sourceRevision"
)
Set-Content -LiteralPath (Join-Path $metadataDirectory 'package.txt') `
    -Value $metadata -Encoding ascii
Set-Content -LiteralPath (Join-Path $metadataDirectory 'wpmignore.txt') `
    -Value ".wpm/`n" -Encoding ascii

$installDirectory = "%ProgramFiles%\WERM\$Version\$Architecture"
$installScript = @(
    '@echo off',
    'setlocal',
    ('set "WERM_DEST={0}"' -f $installDirectory),
    'if not exist "%WERM_DEST%" mkdir "%WERM_DEST%" || exit /b 1',
    'xcopy "%~dp0..\payload\*" "%WERM_DEST%\" /E /I /Q /Y >nul || exit /b 1',
    'reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" /v WERM_HOME /t REG_EXPAND_SZ /d "%WERM_DEST%" /f >nul || exit /b 1',
    'exit /b 0'
)
$removeScript = @(
    '@echo off',
    'setlocal',
    ('set "WERM_DEST={0}"' -f $installDirectory),
    'if exist "%WERM_DEST%" rmdir /S /Q "%WERM_DEST%" || exit /b 1',
    'set "WERM_CURRENT="',
    'for /f "tokens=2,*" %%A in (''reg query "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" /v WERM_HOME 2^>nul'') do set "WERM_CURRENT=%%B"',
    'if /I "%WERM_CURRENT%"=="%WERM_DEST%" reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" /v WERM_HOME /f >nul 2>&1',
    'exit /b 0'
)
Set-Content -LiteralPath (Join-Path $metadataDirectory 'install.cmd') `
    -Value $installScript -Encoding ascii
Set-Content -LiteralPath (Join-Path $metadataDirectory 'remove.cmd') `
    -Value $removeScript -Encoding ascii

& $WpmPath build $staging $resolvedOutputDirectory
if ($LASTEXITCODE -ne 0) {
    throw "WPM failed to build the $Architecture $Configuration package."
}

$debugFlavor = if ($Configuration -eq 'Debug') { '-debug' } else { '' }
$packagePath = Join-Path $resolvedOutputDirectory `
    "werm-$Architecture$debugFlavor-$Version.zip"
if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf)) {
    throw "WPM did not produce the expected package: $packagePath"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($packagePath)
try {
    $entries = @{}
    foreach ($entry in $archive.Entries) {
        $entries[$entry.FullName.Replace('\', '/')] = $entry.Length
    }

    foreach ($requiredEntry in @(
        'payload/Werm.exe',
        'payload/Werm.Data.dll',
        'payload/Werm.Printing.dll',
        'payload/database/migrations/0001-initial-schema.sql',
        'payload/tools/install-werm-database.wsh',
        'payload/docs/word-template-contract.md',
        'payload/docs/workstation-configuration.md',
        'payload/README.md',
        '.wpm/package.txt',
        '.wpm/install.cmd',
        '.wpm/remove.cmd',
        '.wpm/index.csv')) {
        if (-not $entries.ContainsKey($requiredEntry) -or
            $entries[$requiredEntry] -eq 0) {
            throw "WPM package is missing a required entry: $requiredEntry"
        }
    }
} finally {
    $archive.Dispose()
}

Get-Item -LiteralPath $packagePath
