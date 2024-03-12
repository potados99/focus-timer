# 주어진 프로젝트를 빌드하여 배포하는 스크립트입니다.
# 
# 두 가지 배포를 지원합니다: ClickOnce와 Artifact
# 전자는 ClickOnce 배포용 GitHub 저장소에 빌드 결과를 업로드합니다.
# 후자는 tag와 함께 등록된 release에 빌드 결과를 업로드합니다.
#
# 태그 v2.0.0에 맞추어 빌드만 하고 싶다?
# > ./release.ps1 -Tag "v2.0.0"
#
# 태그 v2.0.0에 맞추어 빌드하고 ClickOnce 배포하고 싶다?
# > ./release.ps1 -Tag "v2.0.0" -ClickOnce
# 단, 이 때 ClickOnce 배포용 GitHub 저장소에 쓰기 권한을 가지도록 git이 설정되어 있어야 합니다.
#
# 태그 v2.0.0에 맞추고 전처리기 상수에 "HELLO"를 넣어 빌드한 다음 release에 아티팩트로 빌드 결과물을 달고 싶다?
# > ./release.ps1 -Tag "v2.0.0" -Artifact -Constants "HELLO"
# 단, 이 때 GH_TOKEN 환경변수로 해당 저장소에 권한이 있는 GITHUB_TOKEN을 제공해 주어야 합니다.
#
# 참고: 이 스크립트는 아래 링크의 것을 참고하여 만들어졌습니다.
# https://janjones.me/posts/clickonce-installer-build-publish-github/.

    [CmdletBinding(PositionalBinding = $false)]
param (
    [string]$Tag, # 버전 정보를 뽑아올 때, 릴리즈를 찾을 때에 쓰입니다. 없으면 git describe --tags 명령으로 꺼내옵니다.
    [string]$Constants, # 빌드 명령에 넘길 전처리기 상수입니다.
    [switch]$ClickOnce, # ClickOnce 배포 여부입니다.
    [switch]$Artifact # Release에 빌드 결과물을 올릴지 여부입니다.
)

$appName = "FocusTimer" # 앱 이름입니다. 아래에서 ClickOnce 배포 디렉토리의 이름으로 사용됩니다.
$projDir = "FocusTimer" # 프로젝트 폴더 이름입니다. 빌드할 프로젝트 경로를 찾을 때에 사용됩니다.
$distRepo = "https://github.com/potados99/distribution.git" # ClickOnce 배포용 저장소의 이름입니다.

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

Write-Output "Working directory: $pwd"
Write-Output "Constants: $Constants"
Write-Output "ClickOnce: $ClickOnce"

# Tag 정보를 가져옵니다.
# 파라미터로 주어진 것을 먼저 사용하고, 없으면 git에서 가져옵니다.
$tag = $Tag
if ( [string]::IsNullOrEmpty($tag))
{
    $tag = $( git describe --tags ) # v2.0.0-beta1
}
Write-Output "Tag: $tag"

# Tag를 파싱하여 네 자리 버전 스트링을 구해옵니다.
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

# 프로젝트 내에서 빌드 결과물을 저장할 디렉토리를 정합니다.
$publishDir = "bin/publish"

# 솔루션 내에서 빌드 결과물이 저장될 디렉토리를 정합니다.
# $publishDir는 프로젝트 폴더 내부에 있으므로, 프로젝트 폴더 경로를 더해줍니다.
$outDir = "$projDir/$publishDir"

# 빌드하기 전에 output 디렉토리를 비웁니다.
if (Test-Path $outDir)
{
    Remove-Item -Path $outDir -Recurse
}

# ClickOnce 배포를 한다?
# 1. ClickOnce 배포용 프로필을 사용하여 publish합니다.
# 2. 그렇게 publish된 결과를 배포용 저장소에 업로드합니다.
if ($ClickOnce)
{
    # MSBuild의 경로를 찾습니다.
    $msBuildPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe `
    -prerelease | select-object -first 1
    Write-Output "MSBuild: $( (Get-Command $msBuildPath).Path )"

    # ClickOnce 배포용 프로필을 사용하여 publish합니다.
    # 아니 도대체 왜 dotnet publish가 아니라 msbuild를 쓰는 거지?
    # 그것은 dotnet이 ClickOnce를 "제대로" 지원하지 않기 때문입니다.
    # 빌드는 되는데 파일이 몇 개 빠진다든가... 그런 문제가 있습니다.
    Write-Output "Publishing ClickOnce..."
    dotnet restore -r win-x64
    & $msBuildPath /target:publish `
        /p:PublishProfile=ClickOnceProfile `
        /p:Configuration=Release `
        /p:ApplicationVersion=$version `
        /p:PublishDir=$publishDir `
        /p:PublishUrl=$publishDir `
        /p:DefineConstants="$Constants"
    
    Write-Output "Setting up distribution repository for ClickOnce..."
    # ClickOnce 배포용 저장소를 클론합니다.
    # 이전 커밋 기록 없이 현재 것만 가져옵니다.
    git clone $distRepo --depth 1 "distribution"

    # 배포용 저장소 폴더에 들어갑니다.
    Push-Location "distribution"
    
    # 배포용 저장소의 이 앱 폴더에 들어갑니다.
    Push-Location $appName

    # CRLF만 사용하도록 바꿉니다.
    # Git의 line ending 자동 변환으로 인한 해시값 변경을 방지합니다.
    git config core.eol native
    git config core.autocrlf false

    # 이전 버전을 지웁니다. 싹
    Write-Output "Removing previous deployed versions..."
    if (Test-Path "Application Files")
    {
        Remove-Item -Path "Application Files" -Recurse
    }
    if (Test-Path "$appName.application")
    {
        Remove-Item -Path "$appName.application"
    }
    if (Test-Path "setup.exe")
    {
        Remove-Item -Path "setup.exe"
    }

    # 새 빌드를 복사합니다.
    Write-Output "Copying new files..."
    Copy-Item -Path "../../$outDir/Application Files", "../../$outDir/$appName.application", "../../$outDir/setup.exe" `
        -Destination . -Recurse

    # 스테이지 & 커밋 & 푸시합니다.
    Write-Output "Staging..."
    git add -A
    Write-Output "Committing..."
    git commit -m "chore(release): $appName v$version"
    Write-Output "Pushing..."
    git push

    # 배포용 저장소의 이 앱 폴더에서 나갑니다.
    Pop-Location
    
    # 배포용 저장소 폴더에서 나갑니다.
    Pop-Location
}

# Release에 빌드 결과물을 올린다?
# 1. 로컬에 publish합니다.
# 2. 그렇게 publish된 결과물을 릴리즈에 업로드합니다.
if ($Artifact)
{
    # publish 합니다.
    Write-Output "Publishing to local..."
    dotnet publish `
        --configuration Release `
        --property:ApplicationVersion=$version `
        --property:PublishDir=$publishDir `
        --property:PublishUrl=$publishDir `
        --property:DefineConstants="$Constants"

    # 우리가 빼올 exe 파일의 이름입니다.
    $productExeName = "$appName.exe"

    # artifact의 이름을 정합니다.
    if ($Constants -ne "")
    {
        # 전처리기 상수가 있으면, 그것을 포함한 이름으로 설정합니다.
        $artifactName = "$appName`_$tag`_$Constants.exe"
    }
    else
    {
        # 없으면 그냥 태그 이름으로 설정합니다.
        $artifactName = "$appName`_$tag.exe"
    }
    
    # 빌드 결과물인 exe 파일을 복사합니다.
    Write-Output "Extracting single executable: $productExeName"
    Copy-Item -Path "$outDir/$productExeName" -Destination $artifactName

    # 현재 태그에 딸린 릴리즈에 artifact를 올립니다.
    Write-Output "Uploading build artifacts: $artifactName"
    gh release upload $tag $artifactName
}