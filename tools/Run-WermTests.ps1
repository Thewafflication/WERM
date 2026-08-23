[CmdletBinding()]
param(
    [ValidateSet('x86', 'x64')]
    [string]$Architecture = 'x64',

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [string]$ResultsPath,

    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot

if (-not $NoBuild) {
    & (Join-Path $PSScriptRoot 'Build-Werm.ps1') `
        -Architecture $Architecture -Configuration $Configuration
}

if ([string]::IsNullOrWhiteSpace($ResultsPath)) {
    $ResultsPath = Join-Path $repositoryRoot `
        "out\test-results\$Architecture\$Configuration\werm-test-results.xml"
} elseif (-not [IO.Path]::IsPathRooted($ResultsPath)) {
    $ResultsPath = Join-Path $repositoryRoot $ResultsPath
}

$testExecutable = Join-Path $repositoryRoot `
    "out\bin\Werm.Tests\$Architecture\$Configuration\Werm.Tests.exe"
if (-not (Test-Path -LiteralPath $testExecutable -PathType Leaf)) {
    throw "The controlled test executable was not found: $testExecutable"
}

$sourceRevision = $env:GITHUB_SHA
if ([string]::IsNullOrWhiteSpace($sourceRevision)) {
    $sourceRevision = (& git -C $repositoryRoot rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw 'Git failed to identify the test source revision.'
    }
}

& $testExecutable --results $ResultsPath --source-revision $sourceRevision `
    --expected-architecture $Architecture --repository-root $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    throw "WERM controlled tests failed with exit code $LASTEXITCODE."
}

Get-Item -LiteralPath $ResultsPath
