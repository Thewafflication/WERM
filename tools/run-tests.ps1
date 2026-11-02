[CmdletBinding()]
param(
    [ValidateSet('x86','x64')]
    [string]$Architecture = 'x64',
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Debug',
    [string]$ResultsPath,
    [switch]$NoBuild
)
# Wrapper around Run-WermTests.ps1
& "$PSScriptRoot/Run-WermTests.ps1" -Architecture $Architecture -Configuration $Configuration -ResultsPath $ResultsPath -NoBuild:$NoBuild