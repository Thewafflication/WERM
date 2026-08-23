[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9a-fA-F]{40}$')]
    [string]$SourceRevision,

    [Parameter(Mandatory)]
    [string]$X86ResultsPath,

    [Parameter(Mandatory)]
    [string]$X64ResultsPath,

    [Parameter(Mandatory)]
    [string]$X86PackagePath,

    [Parameter(Mandatory)]
    [string]$X64PackagePath,

    [Parameter(Mandatory)]
    [string]$ManualResultsPath,

    [string]$WpmVersion = 'Unknown',

    [string]$OutputPath = 'out\release-evidence\werm-0.1.0-test-report.md',

    [string]$JsonOutputPath = 'out\release-evidence\werm-0.1.0-test-report.json'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repositoryRoot = Split-Path -Parent $PSScriptRoot

function Resolve-ReportPath {
    param([string]$Path)

    if ([IO.Path]::IsPathRooted($Path)) {
        return [IO.Path]::GetFullPath($Path)
    }
    return [IO.Path]::GetFullPath((Join-Path $repositoryRoot $Path))
}

function Get-TestStatus {
    param([object[]]$Records)

    $statuses = @($Records | Select-Object -ExpandProperty Status -Unique)
    foreach ($candidate in @('Fail', 'Blocked', 'Inconclusive', 'Not run', 'Not applicable')) {
        if ($statuses -contains $candidate) {
            return $candidate
        }
    }
    return 'Pass'
}

$x86Results = Resolve-ReportPath $X86ResultsPath
$x64Results = Resolve-ReportPath $X64ResultsPath
$x86Package = Resolve-ReportPath $X86PackagePath
$x64Package = Resolve-ReportPath $X64PackagePath
$manualResults = Resolve-ReportPath $ManualResultsPath
foreach ($path in @($x86Results, $x64Results, $x86Package, $x64Package, $manualResults)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required report input is missing: $path"
    }
}

[xml]$x86 = Get-Content -LiteralPath $x86Results -Raw
[xml]$x64 = Get-Content -LiteralPath $x64Results -Raw
foreach ($document in @($x86, $x64)) {
    if ([string]$document.testsuite.'source-revision' -ne $SourceRevision) {
        throw 'Test evidence source revision does not match the requested report baseline.'
    }
}

$manual = Get-Content -LiteralPath $manualResults -Raw | ConvertFrom-Json
$records = New-Object 'System.Collections.Generic.List[object]'
foreach ($configuration in @(
    [pscustomobject]@{ Name = 'x86 Debug'; Document = $x86; Evidence = $x86Results },
    [pscustomobject]@{ Name = 'x64 Debug'; Document = $x64; Evidence = $x64Results })) {
    foreach ($case in $configuration.Document.testsuite.testcase) {
        $records.Add([pscustomobject]@{
            Id = [string]$case.id
            Configuration = $configuration.Name
            Status = if ($null -ne $case.SelectSingleNode('failure')) { 'Fail' } else { 'Pass' }
            Evidence = $configuration.Evidence
        })
    }
}
foreach ($result in $manual.results) {
    $records.Add([pscustomobject]@{
        Id = [string]$result.id
        Configuration = 'Controlled manual environment'
        Status = [string]$result.status
        Evidence = $manualResults
    })
}

$statusNames = @('Pass', 'Fail', 'Blocked', 'Inconclusive', 'Not run', 'Not applicable')
$counts = [ordered]@{}
foreach ($status in $statusNames) {
    $counts[$status] = @($records | Where-Object { $_.Status -eq $status }).Count
}
$overall = if (@($records | Where-Object { $_.Status -ne 'Pass' }).Count -eq 0) {
    'Pass'
} else {
    'Fail'
}

$testRequirements = @{}
foreach ($file in Get-ChildItem (Join-Path $repositoryRoot 'docs\tests') -Filter 'tc-*.md') {
    $text = Get-Content -LiteralPath $file.FullName -Raw
    $testId = [regex]::Match($text, '(?m)^# (TC-\d{4}):').Groups[1].Value
    $testRequirements[$testId] = @([regex]::Matches($text, 'REQ-\d{4}') |
        ForEach-Object { $_.Value } | Sort-Object -Unique)
}

$coverageRows = New-Object 'System.Collections.Generic.List[object]'
foreach ($file in Get-ChildItem (Join-Path $repositoryRoot 'docs\requirements') -Filter 'req-*.md') {
    $text = Get-Content -LiteralPath $file.FullName -Raw
    $requirementId = [regex]::Match($text, '(?m)^# (REQ-\d{4}):').Groups[1].Value
    $testIds = @($testRequirements.Keys | Where-Object {
        $testRequirements[$_] -contains $requirementId
    } | Sort-Object)
    $requirementRecords = @($records | Where-Object { $testIds -contains $_.Id })
    $coverageRows.Add([pscustomobject]@{
        Requirement = $requirementId
        Tests = ($testIds -join ', ')
        Status = Get-TestStatus $requirementRecords
        Evidence = if (@($requirementRecords | Where-Object {
            $_.Configuration -eq 'Controlled manual environment'
        }).Count -gt 0) { 'Automated XML and manual inventory' } else { 'Automated XML' }
    })
}

$x86Digest = (Get-FileHash -LiteralPath $x86Package -Algorithm SHA256).Hash.ToLowerInvariant()
$x64Digest = (Get-FileHash -LiteralPath $x64Package -Algorithm SHA256).Hash.ToLowerInvariant()
$timestamps = @(
    [DateTimeOffset]::Parse([string]$x86.testsuite.timestamp),
    [DateTimeOffset]::Parse([string]$x64.testsuite.timestamp),
    [DateTimeOffset]$manual.recordedAtUtc) | Sort-Object

$lines = New-Object 'System.Collections.Generic.List[string]'
$lines.Add('# WERM 0.1.0 Candidate Test Report')
$lines.Add('')
$lines.Add('**Content type:** Generated controlled test report')
$lines.Add('')
$lines.Add('**Report status:** Draft — required manual gates are blocked')
$lines.Add('')
$lines.Add("**Software baseline:** 0.1.0 candidate at ``$SourceRevision``")
$lines.Add('')
$lines.Add("**Test baseline:** ``$SourceRevision``")
$lines.Add('')
$lines.Add("**Execution period:** $($timestamps[0].ToUniversalTime().ToString('O')) through $($timestamps[-1].ToUniversalTime().ToString('O'))")
$lines.Add('')
$lines.Add('**Approval:** Not approved; see the release-readiness record')
$lines.Add('')
$lines.Add('## Purpose and Scope')
$lines.Add('')
$lines.Add('This report combines the x86/x64 Debug controlled executions, RC package')
$lines.Add('identity, and explicit manual-result inventory. It does not claim that')
$lines.Add('blocked ODBC, Waughtal Shell, GUI, Word/printer, or clean-install gates pass.')
$lines.Add('')
$lines.Add('## Tested Configuration')
$lines.Add('')
$lines.Add('| Attribute | Value |')
$lines.Add('| --- | --- |')
$lines.Add("| Source revision | ``$SourceRevision`` |")
$lines.Add("| Architectures | x86 Debug and x64 Debug controlled tests; x86/x64 Release WPM packages |")
$lines.Add("| Operating system | $([Environment]::OSVersion.VersionString) |")
$lines.Add('| Toolchain | .NET Framework 4.8 MSBuild; custom controlled runner |')
$lines.Add("| WPM | $WpmVersion |")
$lines.Add('| External dependencies | Actual Word, SQLite ODBC, WSH, printer, and stock remain manual preconditions |')
$lines.Add('')
$lines.Add('## Result Summary')
$lines.Add('')
$lines.Add('| Status | Count |')
$lines.Add('| --- | ---: |')
foreach ($status in $statusNames) {
    $lines.Add("| $status | $($counts[$status]) |")
}
$lines.Add('')
$lines.Add("**Overall result:** $overall")
$lines.Add('')
$lines.Add('The overall result is Fail because WSP permits only Pass results to satisfy')
$lines.Add('required release gates; blocked results are not waived by automated passes.')
$lines.Add('')
$lines.Add('## Requirement Coverage')
$lines.Add('')
$lines.Add('| Requirement | Verification | Configuration | Status | Evidence |')
$lines.Add('| --- | --- | --- | --- | --- |')
foreach ($row in $coverageRows) {
    $lines.Add("| ``$($row.Requirement)`` | $($row.Tests) | Release matrix | $($row.Status) | $($row.Evidence) |")
}
$lines.Add('')
$lines.Add('## Detailed Results')
$lines.Add('')
$lines.Add('| Test | Configuration | Status | Evidence |')
$lines.Add('| --- | --- | --- | --- |')
foreach ($record in $records) {
    $relativeEvidence = if ($record.Configuration -eq 'x86 Debug') {
        'x86/werm-test-results.xml'
    } elseif ($record.Configuration -eq 'x64 Debug') {
        'x64/werm-test-results.xml'
    } else {
        [IO.Path]::GetFileName($record.Evidence)
    }
    $lines.Add("| ``$($record.Id)`` | $($record.Configuration) | $($record.Status) | ``$relativeEvidence`` |")
}
$lines.Add('')
$lines.Add('## Deviations and Unresolved Issues')
$lines.Add('')
foreach ($result in $manual.results | Where-Object { $_.status -ne 'Pass' }) {
    $lines.Add("- ``$($result.id)`` — **$($result.status):** $($result.rationale)")
}
$lines.Add('')
$lines.Add('## Conclusion')
$lines.Add('')
$lines.Add('The automated candidate behavior passes in both supported architectures,')
$lines.Add('but the complete 0.1.0 release gate fails until every blocked manual test is')
$lines.Add('executed successfully on the approved deployment and physical environment.')
$lines.Add('')
$lines.Add('## Evidence Inventory')
$lines.Add('')
$lines.Add('| Evidence | Purpose | SHA-256 | Retention |')
$lines.Add('| --- | --- | --- | --- |')
$lines.Add("| x86/werm-test-results.xml | x86 controlled XML | $((Get-FileHash $x86Results -Algorithm SHA256).Hash.ToLowerInvariant()) | CI artifact |")
$lines.Add("| x64/werm-test-results.xml | x64 controlled XML | $((Get-FileHash $x64Results -Algorithm SHA256).Hash.ToLowerInvariant()) | CI artifact |")
$lines.Add("| $([IO.Path]::GetFileName($x86Package)) | x86 WPM package | $x86Digest | CI artifact / candidate store |")
$lines.Add("| $([IO.Path]::GetFileName($x64Package)) | x64 WPM package | $x64Digest | CI artifact / candidate store |")
$lines.Add("| $([IO.Path]::GetFileName($manualResults)) | Manual statuses and rationale | $((Get-FileHash $manualResults -Algorithm SHA256).Hash.ToLowerInvariant()) | Source baseline |")

$resolvedOutput = Resolve-ReportPath $OutputPath
$resolvedJson = Resolve-ReportPath $JsonOutputPath
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $resolvedOutput) | Out-Null
$lines | Set-Content -LiteralPath $resolvedOutput -Encoding utf8

$jsonRecord = [ordered]@{
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    sourceRevision = $SourceRevision
    overallResult = $overall
    counts = $counts
    packages = @(
        [ordered]@{ architecture = 'x86'; path = $x86Package; sha256 = $x86Digest },
        [ordered]@{ architecture = 'x64'; path = $x64Package; sha256 = $x64Digest })
    coverage = $coverageRows.ToArray()
    results = $records.ToArray()
}
$jsonRecord | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath $resolvedJson -Encoding utf8

Get-Item -LiteralPath $resolvedOutput
Get-Item -LiteralPath $resolvedJson
