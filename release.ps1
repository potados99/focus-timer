# From https://janjones.me/posts/clickonce-installer-build-publish-github/.

    [CmdletBinding(PositionalBinding = $false)]
param (
    [switch]$OnlyBuild = $false
)

$appName = "FocusTimer" # 👈 Replace with your application project name.
$projDir = "FocusTimer" # 👈 Replace with your project directory (where .csproj resides).
$distRepo = "https://github.com/potados99/distribution.git"

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

Write-Output "Working directory: $pwd"

# Find MSBuild.
$msBuildPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe `
    -prerelease | select-object -first 1
Write-Output "MSBuild: $( (Get-Command $msBuildPath).Path )"

# Load current Git tag.
$tag = $( git describe --tags ) # v2.0.0-beta1
Write-Output "Tag: $tag"

# Parse tag into a three-number-and-prerelease version.
$splitted = $tag.Split('-')
$version = $splitted[0].TrimStart('v')
$splitted = @("") + ($splitted | Select-Object -Skip 1) # replace first one with empty string.
$preReleaseNum = $splitted[$splitted.Length - 1] -replace "[^0-9]", ''
if ( [string]::IsNullOrEmpty($preReleaseNum))
{
    $preReleaseNum = "0"
}
$version = "$version.$preReleaseNum" # 2.0.0.1
Write-Output "Version: $version"

# Clean output directory.
$publishDir = "bin/publish"
$outDir = "$projDir/$publishDir"
if (Test-Path $outDir)
{
    Remove-Item -Path $outDir -Recurse
}

# Publish the application.
Push-Location $projDir
try
{
    Write-Output "Restoring:"
    dotnet restore -r win-x64
    Write-Output "Publishing:"
    $msBuildVerbosityArg = "/v:m"
    if ($env:CI)
    {
        $msBuildVerbosityArg = ""
    }
    & $msBuildPath /target:publish /p:PublishProfile=ClickOnceProfile `
        /p:ApplicationVersion=$version `
        /p:Configuration=Release `
        /p:PublishDir=$publishDir `
        /p:PublishUrl=$publishDir `
        $msBuildVerbosityArg

    # Measure publish size.
    $publishSize = (Get-ChildItem -Path "$publishDir/Application Files" -Recurse |
            Measure-Object -Property Length -Sum).Sum / 1Mb
    Write-Output ("Published size: {0:N2} MB" -f $publishSize)
}
finally
{
    Pop-Location
}

if ($OnlyBuild)
{
    Write-Output "Build finished."
    exit
}

# Clone distribution repository.
git clone $distRepo --depth 1 "distribution"

# Enter the repository.
Push-Location "distribution/$appName"

# Use CRLF only.
# It will prevent hashes from being changed due to line-end converts.
git config core.eol native
git config core.autocrlf false

try
{
    # Remove previous application files.
    Write-Output "Removing previous files..."
    if (Test-Path "Application Files")
    {
        Remove-Item -Path "Application Files" -Recurse
    }
    if (Test-Path "$appName.application")
    {
        Remove-Item -Path "$appName.application"
    }

    # Copy new application files.
    Write-Output "Copying new files..."
    Copy-Item -Path "../../$outDir/Application Files", "../../$outDir/$appName.application" `
        -Destination . -Recurse

    # Stage and commit.
    Write-Output "Staging..."
    git add -A
    Write-Output "Committing..."
    git commit -m "chore(release): $appName v$version"

    # Push.
    git push
}
finally
{
    Pop-Location
}