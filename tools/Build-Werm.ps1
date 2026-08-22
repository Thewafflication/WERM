[CmdletBinding()]
param(
    [ValidateSet('x86', 'x64')]
    [string]$Architecture = 'x64',

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [string]$MSBuildPath,

    [switch]$Clean
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($MSBuildPath)) {
    $command = Get-Command msbuild.exe -ErrorAction SilentlyContinue |
        Select-Object -First 1 -ExpandProperty Source
    if ($command) {
        $MSBuildPath = $command
    } else {
        $vswhere = Join-Path ${env:ProgramFiles(x86)} `
            'Microsoft Visual Studio\Installer\vswhere.exe'
        if (Test-Path -LiteralPath $vswhere -PathType Leaf) {
            $MSBuildPath = & $vswhere -latest -products * `
                -requires Microsoft.Component.MSBuild `
                -find 'MSBuild\**\Bin\MSBuild.exe' |
                Select-Object -First 1
        }
    }
}

if ([string]::IsNullOrWhiteSpace($MSBuildPath) -or
    -not (Test-Path -LiteralPath $MSBuildPath -PathType Leaf)) {
    throw 'MSBuild was not found. Install Visual Studio with .NET desktop build tools.'
}

$solution = Join-Path $repositoryRoot 'Werm.sln'
$target = if ($Clean) { 'Rebuild' } else { 'Build' }
$arguments = @(
    $solution,
    '/m',
    "/t:$target",
    "/p:Configuration=$Configuration",
    "/p:Platform=$Architecture",
    '/p:ContinuousIntegrationBuild=true',
    '/verbosity:minimal',
    '/nologo'
)

& $MSBuildPath @arguments
if ($LASTEXITCODE -ne 0) {
    throw "WERM $Architecture $Configuration build failed with exit code $LASTEXITCODE."
}

Get-Item -LiteralPath (Join-Path $repositoryRoot `
    "out\bin\Werm.App\$Architecture\$Configuration\Werm.exe")
