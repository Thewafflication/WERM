[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('x86', 'x64')]
    [string] $Architecture,

    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+$')]
    [string] $Version = '0.1.0',

    [string] $OutputDirectory = 'out\release-evidence\version-resources'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -TypeDefinition @'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

public sealed class WermNativeVersionResource
{
    [StructLayout(LayoutKind.Sequential)]
    private struct VS_FIXEDFILEINFO
    {
        public UInt32 Signature;
        public UInt32 StrucVersion;
        public UInt32 FileVersionMS;
        public UInt32 FileVersionLS;
        public UInt32 ProductVersionMS;
        public UInt32 ProductVersionLS;
        public UInt32 FileFlagsMask;
        public UInt32 FileFlags;
        public UInt32 FileOS;
        public UInt32 FileType;
        public UInt32 FileSubtype;
        public UInt32 FileDateMS;
        public UInt32 FileDateLS;
    }

    [DllImport("version.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern UInt32 GetFileVersionInfoSize(
        string fileName,
        out UInt32 handle);

    [DllImport("version.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetFileVersionInfo(
        string fileName,
        UInt32 handle,
        UInt32 length,
        byte[] data);

    [DllImport("version.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool VerQueryValue(
        byte[] block,
        string subBlock,
        out IntPtr value,
        out UInt32 length);

    public UInt32 FileFlagsMask { get; private set; }
    public UInt32 FileFlags { get; private set; }
    public UInt32 FileOS { get; private set; }
    public UInt32 FileType { get; private set; }
    public UInt32 FileSubtype { get; private set; }
    public UInt16 Language { get; private set; }
    public UInt16 CodePage { get; private set; }

    public static WermNativeVersionResource Read(string path)
    {
        UInt32 ignored;
        UInt32 size = GetFileVersionInfoSize(path, out ignored);
        if (size == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        byte[] data = new byte[size];
        if (!GetFileVersionInfo(path, 0, size, data))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        IntPtr pointer;
        UInt32 length;
        if (!VerQueryValue(data, "\\", out pointer, out length) ||
            length < Marshal.SizeOf(typeof(VS_FIXEDFILEINFO)))
            throw new InvalidOperationException("VERSIONINFO fixed data is missing.");
        VS_FIXEDFILEINFO fixedInfo = (VS_FIXEDFILEINFO)Marshal.PtrToStructure(
            pointer, typeof(VS_FIXEDFILEINFO));
        if (fixedInfo.Signature != 0xFEEF04BD)
            throw new InvalidOperationException("VERSIONINFO signature is invalid.");

        if (!VerQueryValue(data, "\\VarFileInfo\\Translation", out pointer, out length) ||
            length < 4)
            throw new InvalidOperationException("VERSIONINFO translation is missing.");

        return new WermNativeVersionResource {
            FileFlagsMask = fixedInfo.FileFlagsMask,
            FileFlags = fixedInfo.FileFlags,
            FileOS = fixedInfo.FileOS,
            FileType = fixedInfo.FileType,
            FileSubtype = fixedInfo.FileSubtype,
            Language = unchecked((UInt16)Marshal.ReadInt16(pointer, 0)),
            CodePage = unchecked((UInt16)Marshal.ReadInt16(pointer, 2))
        };
    }
}
'@

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$applicationDirectory = Join-Path $repositoryRoot `
    "out\bin\Werm.App\$Architecture\$Configuration"
$expectedMachine = if ($Architecture -eq 'x64') { 0x8664 } else { 0x014c }
$expectedFiles = [ordered]@{
    'Werm.exe' = 'WERM'
    'Werm.Core.dll' = 'WERM Core'
    'Werm.Data.dll' = 'WERM Data'
    'Werm.Printing.dll' = 'WERM Word Printing'
}
$records = New-Object 'System.Collections.Generic.List[object]'

foreach ($entry in $expectedFiles.GetEnumerator()) {
    $path = Join-Path $applicationDirectory $entry.Key
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Release binary is missing: $path"
    }
    $versionInfo = (Get-Item -LiteralPath $path).VersionInfo
    $requiredValues = [ordered]@{
        CompanyName = $versionInfo.CompanyName
        FileDescription = $versionInfo.FileDescription
        FileVersion = $versionInfo.FileVersion
        InternalName = $versionInfo.InternalName
        LegalCopyright = $versionInfo.LegalCopyright
        OriginalFilename = $versionInfo.OriginalFilename
        ProductName = $versionInfo.ProductName
        ProductVersion = $versionInfo.ProductVersion
    }
    foreach ($value in $requiredValues.GetEnumerator()) {
        if ([string]::IsNullOrWhiteSpace([string] $value.Value)) {
            throw "$($entry.Key) has an empty $($value.Key) version string."
        }
    }
    if ($versionInfo.CompanyName -ne 'Waughtal' -or
            $versionInfo.FileDescription -ne $entry.Value -or
            $versionInfo.OriginalFilename -ne $entry.Key -or
            $versionInfo.ProductName -ne
                'Waughtal Enterprise Resource Management' -or
            $versionInfo.Comments -ne
                'https://github.com/Thewafflication/WERM' -or
            $versionInfo.FileVersion -ne "$Version.0" -or
            $versionInfo.ProductVersion -notin @($Version, "$Version.0")) {
        throw "$($entry.Key) version strings disagree with the release identity."
    }

    $bytes = [IO.File]::ReadAllBytes($path)
    $peOffset = [BitConverter]::ToInt32($bytes, 0x3c)
    if ([BitConverter]::ToUInt32($bytes, $peOffset) -ne 0x00004550) {
        throw "$($entry.Key) has an invalid PE signature."
    }
    $machine = [BitConverter]::ToUInt16($bytes, $peOffset + 4)
    if ($machine -ne $expectedMachine) {
        throw "$($entry.Key) machine type does not match $Architecture."
    }

    $native = [WermNativeVersionResource]::Read($path)
    $expectedType = if ($entry.Key.EndsWith('.exe')) { 1 } else { 2 }
    if ($native.FileFlagsMask -ne 0x3f -or
            $native.FileOS -ne 0x00040004 -or
            $native.FileType -ne $expectedType -or
            $native.FileSubtype -ne 0 -or
            $native.Language -ne 0x0409 -or
            $native.CodePage -ne 1200) {
        throw "$($entry.Key) fixed VERSIONINFO fields are invalid: " +
            "mask=$($native.FileFlagsMask), flags=$($native.FileFlags), " +
            "os=$($native.FileOS), type=$($native.FileType), " +
            "subtype=$($native.FileSubtype), language=$($native.Language), " +
            "codepage=$($native.CodePage)."
    }
    $expectedFlags = if ($Configuration -eq 'Debug') { 1 } else { 0 }
    if ($native.FileFlags -ne $expectedFlags) {
        throw "$($entry.Key) VERSIONINFO flags are $($native.FileFlags), " +
            "expected $expectedFlags for $Configuration."
    }

    $records.Add([ordered]@{
        file = $entry.Key
        architecture = $Architecture
        configuration = $Configuration
        machine = ('0x{0:x4}' -f $machine)
        fileVersion = $versionInfo.FileVersion
        productVersion = $versionInfo.ProductVersion
        company = $versionInfo.CompanyName
        description = $versionInfo.FileDescription
        copyright = $versionInfo.LegalCopyright
        repository = $versionInfo.Comments
        language = ('0x{0:x4}' -f $native.Language)
        codePage = $native.CodePage
        fileType = $native.FileType
        fileFlags = $native.FileFlags
        sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.
            ToLowerInvariant()
    })
}

$resolvedOutput = if ([IO.Path]::IsPathRooted($OutputDirectory)) {
    [IO.Path]::GetFullPath($OutputDirectory)
} else {
    [IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputDirectory))
}
New-Item -ItemType Directory -Force -Path $resolvedOutput | Out-Null
$outputPath = Join-Path $resolvedOutput `
    "$Architecture-$($Configuration.ToLowerInvariant()).json"
[ordered]@{
    verifiedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    version = $Version
    architecture = $Architecture
    configuration = $Configuration
    files = $records.ToArray()
} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $outputPath -Encoding utf8

Write-Host "Windows version resources passed for $Architecture $Configuration."
Get-Item -LiteralPath $outputPath
