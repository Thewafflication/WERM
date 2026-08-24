[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration,

    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+$')]
    [string] $Version = '0.1.0'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'out'))
$outputDirectory = Join-Path $outRoot "version-resources\$Configuration"
if (-not $outputDirectory.StartsWith(
        $outRoot.TrimEnd('\') + '\',
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing a version-resource path outside $outRoot."
}
if (Test-Path -LiteralPath $outputDirectory) {
    Remove-Item -LiteralPath $outputDirectory -Recurse -Force
}
New-Item -ItemType Directory -Path $outputDirectory | Out-Null

$resourceCompiler = Get-ChildItem `
    "${env:ProgramFiles(x86)}\Windows Kits\10\bin" `
    -Recurse -Filter rc.exe -File -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -match '\\x64\\rc\.exe$' } |
    Sort-Object FullName -Descending |
    Select-Object -First 1
if ($null -eq $resourceCompiler) {
    throw 'The Windows SDK resource compiler was not found.'
}

$parts = $Version.Split('.')
$numericVersion = "$($parts[0]),$($parts[1]),$($parts[2]),0"
$fileVersion = "$Version.0"
$flags = if ($Configuration -eq 'Debug') { 'VS_FF_DEBUG' } else { '0x0L' }
$artifacts = @(
    [ordered]@{
        Project = 'Werm.App'; File = 'Werm.exe'; Internal = 'Werm'
        Description = 'WERM'; Type = 'VFT_APP'
    },
    [ordered]@{
        Project = 'Werm.Core'; File = 'Werm.Core.dll'; Internal = 'Werm.Core'
        Description = 'WERM Core'; Type = 'VFT_DLL'
    },
    [ordered]@{
        Project = 'Werm.Data'; File = 'Werm.Data.dll'; Internal = 'Werm.Data'
        Description = 'WERM Data'; Type = 'VFT_DLL'
    },
    [ordered]@{
        Project = 'Werm.Printing'; File = 'Werm.Printing.dll'
        Internal = 'Werm.Printing'; Description = 'WERM Word Printing'
        Type = 'VFT_DLL'
    },
    [ordered]@{
        Project = 'Werm.Tests'; File = 'Werm.Tests.exe'; Internal = 'Werm.Tests'
        Description = 'WERM Controlled Tests'; Type = 'VFT_APP'
    })

foreach ($artifact in $artifacts) {
    $resourcePath = Join-Path $outputDirectory "$($artifact.Project).rc"
    $compiledPath = Join-Path $outputDirectory "$($artifact.Project).res"
    $source = @"
#define VS_FFI_FILEFLAGSMASK 0x0000003fL
#define VS_FF_DEBUG 0x00000001L
#define VOS_NT_WINDOWS32 0x00040004L
#define VFT_APP 0x00000001L
#define VFT_DLL 0x00000002L
#define VFT2_UNKNOWN 0x00000000L

1 VERSIONINFO
FILEVERSION $numericVersion
PRODUCTVERSION $numericVersion
FILEFLAGSMASK VS_FFI_FILEFLAGSMASK
FILEFLAGS $flags
FILEOS VOS_NT_WINDOWS32
FILETYPE $($artifact.Type)
FILESUBTYPE VFT2_UNKNOWN
BEGIN
    BLOCK "StringFileInfo"
    BEGIN
        BLOCK "040904B0"
        BEGIN
            VALUE "CompanyName", "Waughtal\0"
            VALUE "FileDescription", "$($artifact.Description)\0"
            VALUE "FileVersion", "$fileVersion\0"
            VALUE "InternalName", "$($artifact.Internal)\0"
            VALUE "LegalCopyright", "Copyright (c) 2026 Waughtal\0"
            VALUE "OriginalFilename", "$($artifact.File)\0"
            VALUE "ProductName", "Waughtal Enterprise Resource Management\0"
            VALUE "ProductVersion", "$Version\0"
            VALUE "Comments", "https://github.com/Thewafflication/WERM\0"
        END
    END
    BLOCK "VarFileInfo"
    BEGIN
        VALUE "Translation", 0x0409, 1200
    END
END
"@
    Set-Content -LiteralPath $resourcePath -Value $source -Encoding ascii
    & $resourceCompiler.FullName /nologo "/fo$compiledPath" $resourcePath
    if ($LASTEXITCODE -ne 0 -or
            -not (Test-Path -LiteralPath $compiledPath -PathType Leaf)) {
        throw "Version resource compilation failed for $($artifact.Project)."
    }
}

Get-ChildItem -LiteralPath $outputDirectory -Filter '*.res' -File
