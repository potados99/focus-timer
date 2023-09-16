# 주어진 프로젝트를 빌드하여 배포하는 스크립트입니다.
# 두 가지 배포를 지원합니다: ClickOnce와 Artifact
# 전자는 ClickOnce 배포용 GitHub 저장소에 빌드 결과를 업로드합니다.
# 후자는 tag와 함께 등록된 release에 빌드 결과를 업로드합니다.
#
# 이 스크립트는 아래 링크의 것을 참고하여 만들어졌습니다.
# From https://janjones.me/posts/clickonce-installer-build-publish-github/.

    [CmdletBinding(PositionalBinding = $false)]
param (
    [string]$Tag,
    [string]$Constants,
    [switch]$ClickOnce,
    [switch]$Artifact
)

$appName = "FocusTimer" # 앱 이름입니다. 아래에서 ClickOnce 배포 디렉토리의 이름으로 사용됩니다.
$projDir = "FocusTimer" # 프로젝트 폴더 이름입니다. 빌드할 프로젝트 경로를 찾을 때에 사용됩니다.
$distRepo = "https://github.com/potados99/distribution.git" # ClickOnce 배포용 저장소의 이름입니다.

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

Write-Output "Working directory: $pwd"
Write-Output "Constants: $Constants"
Write-Output "ClickOnce: $ClickOnce"
Write-Output "Artifact: $Artifact"

# MSBuild의 경로를 찾습니다.
$msBuildPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe `
    -prerelease | select-object -first 1
Write-Output "MSBuild: $( (Get-Command $msBuildPath).Path )"

# Tag 정보를 가져옵니다.
# 파라미터로 주어진 것을 먼저 사용하고, 없으면 git에서 가져옵니다.
$tag = $Tag
if ([string]::IsNullOrEmpty($tag))
{
    $tag = $( git describe --tags ) # v2.0.0-beta1
}
Write-Output "Tag: $tag"

# Tag를 파싱하여 네 자리 버전 스트링을 구해옵니다.
$splitted = $tag.Split('-')
$version = $splitted[0].TrimStart('v')
$splitted = @("") + ($splitted | Select-Object -Skip 1) # replace first one with empty string.
$preReleaseNum = $splitted[$splitted.Length - 1] -replace "[^0-9]", ''
if ([string]::IsNullOrEmpty($preReleaseNum))
{
    $preReleaseNum = "0"
}
$version = "$version.$preReleaseNum" # 2.0.0.1
Write-Output "Version: $version"

# 빌드하기 전에 output 디렉토리를 비웁니다.
$publishDir = "bin/publish"
$outDir = "$projDir/$publishDir"
if (Test-Path $outDir)
{
    Remove-Item -Path $outDir -Recurse
}

# 빌드합니다.
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
    & $msBuildPath /target:publish `
        /p:PublishProfile=ClickOnceProfile `
        /p:Configuration=Release `
        /p:ApplicationVersion=$version `
        /p:PublishDir=$publishDir `
        /p:PublishUrl=$publishDir `
        /p:DefineConstants="$Constants"
        $msBuildVerbosityArg

    # 빌드 결과물의 크기를 측정합니다.
    $publishSize = (Get-ChildItem -Path "$publishDir/Application Files" -Recurse |
            Measure-Object -Property Length -Sum).Sum / 1Mb
    Write-Output ("Published size: {0:N2} MB" -f $publishSize)
}
finally
{
    Pop-Location
}

if ($ClickOnce)
{
    # ClickOnce 배포용 저장소를 클론합니다.
    # 이전 커밋 기록 없이 현재 것만 가져옵니다.
    git clone $distRepo --depth 1 "distribution"

    # 현재 프로젝트 디렉토리에 들어갑니다.
    Push-Location "distribution/$appName"

    # CRLF만 사용하도록 바꿉니다.
    # Git의 line ending 자동 변환으로 인한 해시값 변경을 방지합니다.
    git config core.eol native
    git config core.autocrlf false

    try
    {
        # 이전 버전을 지웁니다. 싹
        Write-Output "Removing previous files..."
        if (Test-Path "Application Files")
        {
            Remove-Item -Path "Application Files" -Recurse
        }
        if (Test-Path "$appName.application")
        {
            Remove-Item -Path "$appName.application"
        }

        # 새 빌드를 복사합니다.
        Write-Output "Copying new files..."
        Copy-Item -Path "../../$outDir/Application Files", "../../$outDir/$appName.application" `
        -Destination . -Recurse

        # 스테이지 & 커밋합니다.
        Write-Output "Staging..."
        git add -A
        Write-Output "Committing..."
        git commit -m "chore(release): $appName v$version"

        # 올립니다.
        git push
    }
    finally
    {
        Pop-Location
    }
}

if ($Artifact)
{
    $artifactName = "build_$tag.zip"
    
    # 빌드 결과물을 압축합니다.
    Compress-Archive "$projDir/bin/Release/net6.0-windows/win-x64/*" $artifactName
    
    # 현재 태그에 딸린 릴리즈에 artifact를 올립니다.
    gh release upload $tag $artifactName
}
