[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('x86', 'x64')]
    [string] $Architecture,

    [string] $OutputDirectory = 'out\dependencies\sqlite-odbc'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$driverVersion = '0.99991'
$driverRevision = '539531394dcedf415de574daa95367a93f5eb41d'
$driverArchiveHash = '13f29b7ed20ee2408c2e0812a0ca43e33021a6548ede99b95e04e414718b3c0a'
$sqliteVersion = '3.43.2'
$sqliteArchiveHash = 'a17ac8792f57266847d57651c5259001d1e4e4b46be96ec0d985c953925b2a1c'

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$controlledOutputRoot = [IO.Path]::GetFullPath(
    (Join-Path $repositoryRoot 'out'))
$cacheDirectory = Join-Path $controlledOutputRoot 'dependency-cache'
$buildRoot = Join-Path $controlledOutputRoot 'dependency-build'
$buildDirectory = Join-Path $buildRoot "sqlite-odbc-$Architecture"
$resolvedOutputRoot = if ([IO.Path]::IsPathRooted($OutputDirectory)) {
    [IO.Path]::GetFullPath($OutputDirectory)
} else {
    [IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputDirectory))
}
$architectureOutput = Join-Path $resolvedOutputRoot $Architecture

function Assert-WermChildPath {
    param(
        [Parameter(Mandatory)]
        [string] $Parent,

        [Parameter(Mandatory)]
        [string] $Child
    )

    $resolvedParent = [IO.Path]::GetFullPath($Parent).TrimEnd('\')
    $resolvedChild = [IO.Path]::GetFullPath($Child)
    if (-not $resolvedChild.StartsWith(
            $resolvedParent + '\',
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing an output path outside $resolvedParent`: $resolvedChild"
    }
}

function Get-WermVerifiedArchive {
    param(
        [Parameter(Mandatory)]
        [Uri] $Uri,

        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [ValidatePattern('^[0-9a-f]{64}$')]
        [string] $Sha256
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Invoke-WebRequest -UseBasicParsing -Uri $Uri -OutFile $Path
    }
    $actualHash = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).
        Hash.ToLowerInvariant()
    if ($actualHash -ne $Sha256) {
        throw "Dependency digest mismatch for $Path. Expected $Sha256; " +
            "received $actualHash."
    }
}

Assert-WermChildPath -Parent $controlledOutputRoot -Child $cacheDirectory
Assert-WermChildPath -Parent $controlledOutputRoot -Child $buildDirectory
Assert-WermChildPath -Parent $controlledOutputRoot -Child $architectureOutput

New-Item -ItemType Directory -Force -Path @(
    $cacheDirectory,
    $buildRoot,
    $resolvedOutputRoot
) | Out-Null

$driverArchive = Join-Path $cacheDirectory "sqliteodbc-$driverRevision.zip"
$sqliteArchive = Join-Path $cacheDirectory 'sqlite-amalgamation-3430200.zip'
Get-WermVerifiedArchive `
    -Uri "https://github.com/softace/sqliteodbc/archive/$driverRevision.zip" `
    -Path $driverArchive `
    -Sha256 $driverArchiveHash
Get-WermVerifiedArchive `
    -Uri 'https://www.sqlite.org/2023/sqlite-amalgamation-3430200.zip' `
    -Path $sqliteArchive `
    -Sha256 $sqliteArchiveHash

foreach ($directory in @($buildDirectory, $architectureOutput)) {
    Assert-WermChildPath -Parent $controlledOutputRoot -Child $directory
    if (Test-Path -LiteralPath $directory) {
        Remove-Item -LiteralPath $directory -Recurse -Force
    }
    New-Item -ItemType Directory -Path $directory | Out-Null
}

$driverSourceRoot = Join-Path $buildDirectory 'driver-source'
$sqliteSourceRoot = Join-Path $buildDirectory 'sqlite-source'
Expand-Archive -LiteralPath $driverArchive -DestinationPath $driverSourceRoot
Expand-Archive -LiteralPath $sqliteArchive -DestinationPath $sqliteSourceRoot
$driverSource = Get-ChildItem -LiteralPath $driverSourceRoot -Directory |
    Select-Object -First 1
$sqliteSource = Get-ChildItem -LiteralPath $sqliteSourceRoot -Recurse `
    -Filter sqlite3.c -File | Select-Object -First 1
if ($null -eq $driverSource -or $null -eq $sqliteSource) {
    throw 'A pinned SQLite ODBC dependency archive has an unexpected layout.'
}

foreach ($name in @(
        'sqlite3odbc.c',
        'sqlite3odbc.h',
        'sqlite3odbc.def',
        'sqlite3odbc.rc',
        'sqliteodbc.ico',
        'resource.h.in')) {
    Copy-Item -LiteralPath (Join-Path $driverSource.FullName $name) `
        -Destination $buildDirectory
}
foreach ($name in @('sqlite3.c', 'sqlite3.h', 'sqlite3ext.h')) {
    Copy-Item -LiteralPath (Join-Path $sqliteSource.DirectoryName $name) `
        -Destination $buildDirectory
}

# Upstream 0.99991 predates a warning-clean MSVC x64 build. This portability
# patch preserves the 0xDEADBEEF sentinel width without changing behavior.
$driverSourcePath = Join-Path $buildDirectory 'sqlite3odbc.c'
$driverText = (Get-Content -Raw -LiteralPath $driverSourcePath).Replace(
    '(SQLHDESC) DEAD_MAGIC',
    '(SQLHDESC) (UINT_PTR) DEAD_MAGIC')
Set-Content -LiteralPath $driverSourcePath -Value $driverText -Encoding ascii

# The first four legacy module-definition directives are unsupported by the
# current Microsoft linker. The export list remains byte-for-byte upstream.
$definitionPath = Join-Path $buildDirectory 'sqlite3odbc.def'
$portableDefinitions = Get-Content -LiteralPath $definitionPath |
    Where-Object { $_ -notmatch '^(DESCRIPTION|SEGMENTS|DLL_TEXT|INIT_TEXT)' }
Set-Content -LiteralPath $definitionPath `
    -Value $portableDefinitions -Encoding ascii

$resourceHeader = (Get-Content -Raw -LiteralPath (
        Join-Path $buildDirectory 'resource.h.in')).
    Replace('"--VERS--"', "`"$driverVersion`"").
    Replace('--VERS_C--', '0,99991')
Set-Content -LiteralPath (Join-Path $buildDirectory 'resource3.h') `
    -Value $resourceHeader -Encoding ascii

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path -LiteralPath $vswhere -PathType Leaf)) {
    throw 'Visual Studio Installer vswhere.exe was not found.'
}
$visualStudio = & $vswhere -latest -products * `
    -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 `
    -property installationPath
if ([string]::IsNullOrWhiteSpace($visualStudio)) {
    throw 'Visual C++ x86/x64 build tools were not found.'
}
$vcvars = Join-Path $visualStudio 'VC\Auxiliary\Build\vcvarsall.bat'
$environmentLines = & cmd.exe /d /s /c `
    "call `"$vcvars`" $Architecture >nul && set"
if ($LASTEXITCODE -ne 0) {
    throw "vcvarsall failed for $Architecture."
}
foreach ($line in $environmentLines) {
    $separator = $line.IndexOf('=')
    if ($separator -gt 0 -and
            -not $line.Substring(0, $separator).Equals(
                'Path', [StringComparison]::OrdinalIgnoreCase)) {
        Set-Item -LiteralPath ('Env:' + $line.Substring(0, $separator)) `
            -Value $line.Substring($separator + 1)
    }
}
$compilerPathLine = $environmentLines |
    Where-Object {
        $_ -match '^path=' -and $_ -match '\\VC\\Tools\\MSVC\\'
    } |
    Select-Object -First 1
if ($null -eq $compilerPathLine) {
    throw 'Visual C++ environment did not provide a compiler PATH.'
}
$env:Path = $compilerPathLine.Substring($compilerPathLine.IndexOf('=') + 1)

Push-Location $buildDirectory
try {
    $defines = @(
        '/DWIN32',
        '/D_WINDOWS',
        '/D_CRT_SECURE_NO_WARNINGS',
        '/DHAVE_SQLITE3COLUMNTABLENAME=1',
        '/DHAVE_SQLITE3COLUMNDATABASENAME=1',
        '/DHAVE_SQLITE3COLUMNORIGINNAME=1',
        '/DHAVE_SQLITE3LOADEXTENSION=1',
        '/DHAVE_SQLITE3PREPAREV2=1',
        '/DHAVE_SQLITE3VFS=1',
        '/DHAVE_SQLITE3PROFILE=1',
        '/DHAVE_SQLITE3CLOSEV2=1',
        '/DHAVE_SQLITE3STRNICMP=1',
        '/DHAVE_SQLITE3TABLECOLUMNMETADATA=1',
        '/DHAVE_LONG_LONG=1',
        '/DHAVE_SQLROWOFFSET=1',
        '/DHAVE_SQLLEN=1',
        '/DHAVE_SQLULEN=1',
        '/DHAVE_SQLROWCOUNT=1',
        '/DHAVE_SQLSETPOSIROW=1',
        '/DPTRDIFF_T=ptrdiff_t',
        '/DSQLITE_ENABLE_COLUMN_METADATA=1',
        '/DSQLITE_THREADSAFE=1',
        '/DSQLITE_OS_WIN=1',
        '/DSQLITE_ASCII=1',
        '/DSQLITE_SOUNDEX=1',
        '/DWITHOUT_SHELL=1'
    )
    & cl.exe /nologo /c /O2 /MT /W1 /WX /I. `
        $defines sqlite3odbc.c sqlite3.c
    if ($LASTEXITCODE -ne 0) {
        throw 'SQLite ODBC C compilation failed.'
    }
    & rc.exe /nologo /i . /fo sqlite3odbc.res sqlite3odbc.rc
    if ($LASTEXITCODE -ne 0) {
        throw 'SQLite ODBC resource compilation failed.'
    }
    $machine = if ($Architecture -eq 'x64') { 'X64' } else { 'X86' }
    & link.exe /nologo /dll "/machine:$machine" /def:sqlite3odbc.def `
        /out:sqlite3odbc.dll sqlite3odbc.obj sqlite3.obj sqlite3odbc.res `
        odbc32.lib odbccp32.lib kernel32.lib user32.lib comdlg32.lib `
        legacy_stdio_definitions.lib
    if ($LASTEXITCODE -ne 0) {
        throw 'SQLite ODBC driver link failed.'
    }
}
finally {
    Pop-Location
}

$driverPath = Join-Path $buildDirectory 'sqlite3odbc.dll'
Copy-Item -LiteralPath $driverPath -Destination $architectureOutput
Copy-Item -LiteralPath (Join-Path $driverSource.FullName 'license.terms') `
    -Destination (Join-Path $architectureOutput 'sqliteodbc-license.terms')

$manifest = [ordered]@{
    driverVersion = $driverVersion
    driverRevision = $driverRevision
    driverArchiveSha256 = $driverArchiveHash
    sqliteVersion = $sqliteVersion
    sqliteArchiveSha256 = $sqliteArchiveHash
    architecture = $Architecture
    runtimeLinkage = 'static-msvc-crt'
    visualCppToolsVersion = $env:VCToolsVersion
    windowsSdkVersion = $env:WindowsSDKVersion
    portabilityAdjustments = @(
        'Preserve DEAD_MAGIC through UINT_PTR on x64',
        'Remove unsupported legacy module-definition directives')
    driverSha256 = (Get-FileHash -LiteralPath (
        Join-Path $architectureOutput 'sqlite3odbc.dll') -Algorithm SHA256).Hash.
        ToLowerInvariant()
}
$manifest | ConvertTo-Json | Set-Content -LiteralPath (
    Join-Path $architectureOutput 'dependency-manifest.json') -Encoding utf8

Get-Item -LiteralPath (Join-Path $architectureOutput 'sqlite3odbc.dll')
