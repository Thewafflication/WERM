[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot),

    [Parameter(Mandatory)]
    [string[]]$ResultsPath,

    [string[]]$IntegrationResultsPath = @(),

    [Parameter(Mandatory)]
    [string]$ManualResultsPath,

    [string]$OutputPath = 'out\release-evidence\traceability.json'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Get-ControlledIds {
    param([string]$Text, [string]$Pattern)

    return @([regex]::Matches($Text, $Pattern) |
        ForEach-Object { $_.Value } |
        Sort-Object -Unique)
}

function Resolve-WermPath {
    param([string]$Path, [string]$Root)

    if ([IO.Path]::IsPathRooted($Path)) {
        return [IO.Path]::GetFullPath($Path)
    }
    return [IO.Path]::GetFullPath((Join-Path $Root $Path))
}

$root = [IO.Path]::GetFullPath($RepositoryRoot)
$errors = New-Object 'System.Collections.Generic.List[string]'
$requirements = @{}
$tests = @{}

foreach ($file in Get-ChildItem (Join-Path $root 'docs\requirements') -Filter 'req-*.md') {
    $text = Get-Content -LiteralPath $file.FullName -Raw
    $heading = [regex]::Match($text, '(?m)^# (REQ-\d{4}):')
    if (-not $heading.Success) {
        $errors.Add("Requirement file has no controlled heading: $($file.FullName)")
        continue
    }
    $id = $heading.Groups[1].Value
    if ($requirements.ContainsKey($id)) {
        $errors.Add("Duplicate requirement identifier: $id")
        continue
    }
    if ($text -notmatch '(?m)^\*\*Status:\*\* Accepted\s*$') {
        $errors.Add("Requirement is not Accepted: $id")
    }
    $requirements[$id] = [pscustomobject]@{
        Id = $id
        File = $file.FullName
        Text = $text
    }
}

foreach ($file in Get-ChildItem (Join-Path $root 'docs\tests') -Filter 'tc-*.md') {
    $text = Get-Content -LiteralPath $file.FullName -Raw
    $heading = [regex]::Match($text, '(?m)^# (TC-\d{4}):')
    if (-not $heading.Success) {
        $errors.Add("Test file has no controlled heading: $($file.FullName)")
        continue
    }
    $id = $heading.Groups[1].Value
    if ($tests.ContainsKey($id)) {
        $errors.Add("Duplicate test identifier: $id")
        continue
    }

    foreach ($field in @('Status', 'Level', 'Priority', 'Technique')) {
        if ($text -notmatch ('(?m)^\*\*' + [regex]::Escape($field) + ':\*\* .+')) {
            $errors.Add("$id is missing the $field field.")
        }
    }
    if ($text -notmatch '(?m)^\*\*Execution contract:\*\* ' -and
        $text -notmatch '(?m)^## Preconditions, environment, and assumptions\s*$') {
        $errors.Add("$id has neither the automated execution contract nor complete manual preconditions.")
    }
    foreach ($section in @('Objective|Purpose', 'Expected Result|Expected results and pass criteria')) {
        if ($text -notmatch ('(?m)^## (?:' + $section + ')\s*$')) {
            $errors.Add("$id is missing the controlled $section section.")
        }
    }

    $requirementIds = @(Get-ControlledIds $text 'REQ-\d{4}')
    $referenceIds = @(Get-ControlledIds $text '(?:REQ|ADR)-\d{4}')
    if ($referenceIds.Count -eq 0) {
        $errors.Add("$id has no requirement or decision reference.")
    }
    foreach ($requirementId in $requirementIds) {
        if (-not $requirements.ContainsKey($requirementId)) {
            $errors.Add("$id references unknown requirement $requirementId.")
        }
    }

    $tests[$id] = [pscustomobject]@{
        Id = $id
        File = $file.FullName
        Requirements = $requirementIds
        References = $referenceIds
        IsAutomated = $text -match '(?m)^\*\*Execution contract:\*\* '
    }
}

foreach ($test in $tests.Values) {
    foreach ($requirementId in $test.Requirements) {
        if ($requirements.ContainsKey($requirementId) -and
            $requirements[$requirementId].Text -notmatch [regex]::Escape($test.Id)) {
            $errors.Add("$requirementId does not contain the required back-reference to $($test.Id).")
        }
    }
}

foreach ($requirement in $requirements.Values) {
    $coverage = @($tests.Values | Where-Object {
        $_.Requirements -contains $requirement.Id
    })
    if ($coverage.Count -eq 0) {
        $errors.Add("Accepted requirement has no controlled verification: $($requirement.Id)")
    }
}

$resultRecords = New-Object 'System.Collections.Generic.List[object]'
$automatedIds = @()
$sourceRevisions = @()
foreach ($path in $ResultsPath) {
    $resolved = Resolve-WermPath $path $root
    if (-not (Test-Path -LiteralPath $resolved -PathType Leaf)) {
        $errors.Add("Missing automated result: $resolved")
        continue
    }
    [xml]$document = Get-Content -LiteralPath $resolved -Raw
    $architecture = [string]$document.testsuite.'process-architecture'
    $sourceRevision = [string]$document.testsuite.'source-revision'
    $sourceRevisions += $sourceRevision
    $caseIds = @{}
    foreach ($case in $document.testsuite.testcase) {
        $id = [string]$case.id
        if ($caseIds.ContainsKey($id)) {
            $errors.Add("Duplicate result $id in $resolved")
            continue
        }
        $caseIds[$id] = $true
        $automatedIds += $id
        if (-not $tests.ContainsKey($id)) {
            $errors.Add("Automated result has no controlled specification: $id")
        } else {
            $actualRequirements = @(([string]$case.requirement).Split(',') |
                Where-Object { $_ } | Sort-Object -Unique)
            $expectedRequirements = @($tests[$id].References | Sort-Object -Unique)
            if (($actualRequirements -join ',') -ne ($expectedRequirements -join ',')) {
                $errors.Add("$id requirement metadata differs between result and specification.")
            }
        }
        $status = if ($null -ne $case.SelectSingleNode('failure')) { 'Fail' } else { 'Pass' }
        $resultRecords.Add([pscustomobject]@{
            Id = $id
            Configuration = "$architecture Debug"
            Status = $status
            Evidence = $resolved
        })
    }
}

foreach ($path in $IntegrationResultsPath) {
    $resolved = Resolve-WermPath $path $root
    if (-not (Test-Path -LiteralPath $resolved -PathType Leaf)) {
        $errors.Add("Missing integration result: $resolved")
        continue
    }
    $integration = Get-Content -LiteralPath $resolved -Raw |
        ConvertFrom-Json
    $id = [string] $integration.id
    $sourceRevision = [string] $integration.sourceRevision
    $sourceRevisions += $sourceRevision
    $automatedIds += $id
    if (-not $tests.ContainsKey($id)) {
        $errors.Add("Integration result has no controlled specification: $id")
        continue
    }
    if (-not $tests[$id].IsAutomated) {
        $errors.Add("Integration result is not declared automated: $id")
    }
    $actualReferences = @($integration.references |
        ForEach-Object { [string] $_ } | Sort-Object -Unique)
    $expectedReferences = @($tests[$id].References | Sort-Object -Unique)
    if (($actualReferences -join ',') -ne ($expectedReferences -join ',')) {
        $errors.Add("$id reference metadata differs between integration result and specification.")
    }
    $status = [string] $integration.status
    if ($status -notin @('Pass', 'Fail')) {
        $errors.Add("Integration result has an invalid status: $id")
    }
    $resultRecords.Add([pscustomobject]@{
        Id = $id
        Configuration = "$([string] $integration.architecture) integration"
        Status = $status
        Evidence = $resolved
    })
}

if (@($sourceRevisions | Sort-Object -Unique).Count -ne 1) {
    $errors.Add('Automated result files do not identify one source revision.')
}

$resolvedManual = Resolve-WermPath $ManualResultsPath $root
if (-not (Test-Path -LiteralPath $resolvedManual -PathType Leaf)) {
    $errors.Add("Missing manual result inventory: $resolvedManual")
} else {
    $manual = Get-Content -LiteralPath $resolvedManual -Raw | ConvertFrom-Json
    $manualIds = @{}
    foreach ($result in $manual.results) {
        $id = [string]$result.id
        if ($manualIds.ContainsKey($id)) {
            $errors.Add("Duplicate manual result: $id")
            continue
        }
        $manualIds[$id] = $true
        if (-not $tests.ContainsKey($id)) {
            $errors.Add("Manual result has no controlled specification: $id")
        }
        if ([string]$result.status -notin @(
            'Pass', 'Fail', 'Blocked', 'Inconclusive', 'Not run', 'Not applicable')) {
            $errors.Add("Manual result has an invalid status: $id")
        }
        if ([string]$result.status -ne 'Pass' -and
            [string]::IsNullOrWhiteSpace([string]$result.rationale)) {
            $errors.Add("Non-passing manual result has no rationale: $id")
        }
        $resultRecords.Add([pscustomobject]@{
            Id = $id
            Configuration = 'Controlled manual environment'
            Status = [string]$result.status
            Evidence = $resolvedManual
        })
    }
}

$automatedUnique = @($automatedIds | Sort-Object -Unique)
foreach ($test in $tests.Values) {
    $hasResult = if ($test.IsAutomated) {
        $automatedUnique -contains $test.Id
    } else {
        @($resultRecords | Where-Object { $_.Id -eq $test.Id }).Count -gt 0
    }
    if (-not $hasResult) {
        $errors.Add("Controlled test has no result record: $($test.Id)")
    }
}

$output = Resolve-WermPath $OutputPath $root
$outputDirectory = Split-Path -Parent $output
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
$record = [ordered]@{
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    requirementCount = $requirements.Count
    testSpecificationCount = $tests.Count
    resultCount = $resultRecords.Count
    sourceRevision = [string]($sourceRevisions | Sort-Object -Unique |
        Select-Object -First 1)
    errors = @($errors)
}
$record | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $output -Encoding utf8

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ -ErrorAction Continue }
    throw "WERM traceability validation failed with $($errors.Count) error(s)."
}

Write-Host "Traceability passed: $($requirements.Count) requirements, $($tests.Count) tests, $($resultRecords.Count) results."
Get-Item -LiteralPath $output
