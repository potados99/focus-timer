# FocusTimer

[**일에 집중하지 못하는 당신을 위한 프로그램 [Focus]**](https://tumblbug.com/worldmoment_focus)

## 개요

Focus Timer는 Windows 테스크탑 환경에서 실행되는 간단한 타이머 유틸리티입니다.

## 빌드 및 배포

### 배포 파이프라인

새로운 태그가 [GitHub](https://github.com/potados99/focus-timer)에 올라오면 [potados-bot](https://github.com/apps/potados-bot)에 의해 [새로운 릴리즈](https://github.com/potados99/focus-timer/releases)가 생기고, [워크플로우 파일](./.github/workflows/release.yml)에 의해 [배포 스크립트](./release.ps1)가 실행됩니다.

배포 스크립트는 솔루션을 빌드한 다음 별도의 [static hosting 전용 Git 저장소](https://github.com/potados99/distribution)에 ClickOnce 배포를 수행하며, 방금 만들어진 새로운 릴리즈에 빌드 결과물을 아티팩트로 첨부하여 올리기도 합니다.

### 파이프라인을 통해 자동으로 배포하는 방법

위에서 언급한 배포 파이프라인은 새로운 태그에 의해 시작됩니다. 다음 명령으로 새로운 태그를 만들고 푸시합니다:

```
$ git tag v1.0.0 # 사용할 버전 이름
$ git push --tags
```

### 개발 환경에서 커맨드 라인으로 빌드하는 방법

#### 빌드만 하기

[배포 스크립트](./release.ps1)를 사용합니다.

예시:
```powershell
./release.ps1 -Tag "v2.0.0"
```

> 주의: 로컬 개발 환경에서 사용하는 방법으로, 추후 변동이 있을 수 있습니다.
> 
> OS: Windows Server 2022 Datacenter 21H2    
> IDE: JetBrains Rider 2023.1.4    
> Runtime: Microsoft .NET SDK 7.0.302 (x64) from Visual Studio

