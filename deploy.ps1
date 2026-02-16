#Requires -Version 5.1
<#
.SYNOPSIS
    adHelp 배포 파일 준비 스크립트

.DESCRIPTION
    1. AssemblyInfo.cs에서 버전 자동 추출
    2. MSBuild Release 빌드 (AnyCPU)
    3. SHA256 체크섬 생성
    4. adHelp-updates 폴더에 배포 파일(exe, xml, readme) 생성 및 복사
    5. 사용자가 수동으로 업로드하도록 안내

.EXAMPLE
    .\deploy.ps1
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ─────────────────────────────────────────────
# 설정
# ─────────────────────────────────────────────
$Script:ProjectRoot = $PSScriptRoot
$Script:CsprojPath = Join-Path $ProjectRoot "adHelp\adHelp.csproj"
$Script:AssemblyInfoPath = Join-Path $ProjectRoot "adHelp\Properties\AssemblyInfo.cs"
$Script:SolutionPath = Join-Path $ProjectRoot "adHelp.sln"
$Script:ReleaseBinDir = Join-Path $ProjectRoot "adHelp\bin\Release"
$Script:ExeName = "adHelp.exe"

# 업데이트 배포용 폴더 설정 (상위 폴더의 adHelp-updates)
$Script:UpdatesRepoName = "opti12/adHelp-updates"
$Script:UpdatesDir = Join-Path (Split-Path $ProjectRoot -Parent) "adHelp-updates"
# 다운로드 URL 구성용 브랜치 (기본값 main)
$Script:UpdatesBranch = "main" 

# ─────────────────────────────────────────────
# 유틸리티 함수
# ─────────────────────────────────────────────
function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "━━━ $Message" -ForegroundColor Cyan
}

function Write-Ok {
    param([string]$Message)
    Write-Host "  ✓ $Message" -ForegroundColor Green
}

function Write-Fail {
    param([string]$Message)
    Write-Host "  ✗ $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "  → $Message" -ForegroundColor Yellow
}

# ─────────────────────────────────────────────
# 1. 사전 조건 확인
# ─────────────────────────────────────────────
function Test-Prerequisites {
    Write-Step "사전 조건 확인"

    # AssemblyInfo.cs 존재 확인
    if (-not (Test-Path $Script:AssemblyInfoPath)) {
        Write-Fail "AssemblyInfo.cs를 찾을 수 없습니다: $Script:AssemblyInfoPath"
        exit 1
    }
    Write-Ok "AssemblyInfo.cs 확인"
}

# ─────────────────────────────────────────────
# 2. 버전 추출
# ─────────────────────────────────────────────
function Get-AppVersion {
    Write-Step "버전 정보 추출"

    $content = Get-Content $Script:AssemblyInfoPath -Raw
    if ($content -match '\[assembly:\s*AssemblyVersion\("(\d+\.\d+\.\d+\.\d+)"\)\]') {
        $version = $Matches[1]
        Write-Ok "AssemblyVersion: $version"
        return $version
    }
    else {
        Write-Fail "AssemblyInfo.cs에서 AssemblyVersion을 찾을 수 없습니다."
        exit 1
    }
}

# ─────────────────────────────────────────────
# 3. MSBuild 탐지 및 빌드
# ─────────────────────────────────────────────
function Find-MSBuild {
    $searchPaths = @(
        "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\18\Insiders\MSBuild\Current\Bin\amd64\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    )

    foreach ($path in $searchPaths) {
        if (Test-Path $path) {
            return $path
        }
    }
    return $null
}

function Invoke-Build {
    Write-Step "Release 빌드"

    $msbuild = Find-MSBuild
    if (-not $msbuild) {
        Write-Fail "MSBuild를 찾을 수 없습니다. Visual Studio가 설치되어 있는지 확인하세요."
        exit 1
    }
    Write-Ok "MSBuild: $msbuild"

    # NuGet 패키지 복원 (MSBuild 사용)
    Write-Info "NuGet 패키지 복원 중..."
    try {
        & $msbuild $Script:CsprojPath /t:Restore /p:Configuration=Release /v:quiet 2>&1 | Out-Null
        Write-Ok "NuGet 패키지 복원 완료"
    }
    catch {
        Write-Info "NuGet restore 건너뜀 (이미 복원된 패키지 사용)"
    }

    # Release 빌드
    Write-Info "Release 빌드 중..."
    # Platform 속성은 공백 없이 'AnyCPU'여야 함 (csproj 구성과 일치)
    $buildLog = & $msbuild $Script:CsprojPath /p:Configuration=Release /p:Platform="AnyCPU" /v:minimal 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Fail "빌드 실패!"
        Write-Host $buildLog
        exit 1
    }

    $exePath = Join-Path $Script:ReleaseBinDir $Script:ExeName
    if (-not (Test-Path $exePath)) {
        Write-Fail "빌드된 exe를 찾을 수 없습니다: $exePath"
        exit 1
    }

    $fileInfo = Get-Item $exePath
    Write-Ok "빌드 성공: $exePath ($([math]::Round($fileInfo.Length / 1KB)) KB)"
    return $exePath
}

# ─────────────────────────────────────────────
# 4. SHA256 체크섬 계산
# ─────────────────────────────────────────────
function Get-FileChecksum {
    param([string]$FilePath)
    
    Write-Step "SHA256 체크섬 계산"

    $hash = (Get-FileHash -Path $FilePath -Algorithm SHA256).Hash
    Write-Ok "SHA256: $hash"
    return $hash
}

# ─────────────────────────────────────────────
# 5. 배포 파일 준비 (adHelp-updates 폴더)
# ─────────────────────────────────────────────
function Prepare-UpdatesFolder {
    param(
        [string]$Version,
        [string]$ExePath,
        [string]$Checksum
    )

    Write-Step "배포 파일 준비 (adHelp-updates 폴더)"

    # 폴더 생성
    if (-not (Test-Path $Script:UpdatesDir)) {
        Write-Info "폴더가 없어 생성합니다: $Script:UpdatesDir"
        New-Item -Path $Script:UpdatesDir -ItemType Directory -Force | Out-Null
    }
    else {
        Write-Info "폴더 확인: $Script:UpdatesDir"
    }

    # 1. exe 파일 복사
    $destExePath = Join-Path $Script:UpdatesDir $Script:ExeName
    Copy-Item -Path $ExePath -Destination $destExePath -Force
    Write-Ok "exe 파일 복사 완료"

    # 2. UpdateInfo.xml 생성
    # GitHub Raw 파일 링크 (수동 업로드 시 main 브랜치에 올린다고 가정)
    $downloadUrl = "https://github.com/$($Script:UpdatesRepoName)/raw/$($Script:UpdatesBranch)/$($Script:ExeName)"
    
    $xmlPath = Join-Path $Script:UpdatesDir "UpdateInfo.xml"
    $xmlContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<item>
  <version>$Version</version>
  <url>$downloadUrl</url>
  <changelog>https://github.com/opti12/adHelp/compare/v$Version...HEAD</changelog>
  <mandatory>false</mandatory>
  <args>/silent</args>
  <checksum algorithm="SHA256">$Checksum</checksum>
  <executable>$($Script:ExeName)</executable>
  <minVersion>1.0.0.0</minVersion>
</item>
"@

    $xmlContent | Set-Content -Path $xmlPath -Encoding UTF8 -NoNewline
    Write-Ok "UpdateInfo.xml 생성됨"
    
    # 로컬 adHelp 리포의 UpdateInfo.xml도 동기화
    $localXmlPath = Join-Path $Script:ProjectRoot "UpdateInfo.xml"
    $xmlContent | Set-Content -Path $localXmlPath -Encoding UTF8 -NoNewline

    # 3. README.md 복사 (프로젝트 루트의 README.md)
    $sourceReadme = Join-Path $Script:ProjectRoot "README.md"
    $destReadme = Join-Path $Script:UpdatesDir "README.md"
    
    if (Test-Path $sourceReadme) {
        Copy-Item -Path $sourceReadme -Destination $destReadme -Force
        Write-Ok "README.md 복사 완료 (위치: $sourceReadme)"
    }
    else {
        Write-Info "소스 README.md가 없어 복사를 건너뜁니다."
    }
}

# ─────────────────────────────────────────────
# 메인 실행
# ─────────────────────────────────────────────
function Main {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════╗" -ForegroundColor Magenta
    Write-Host "║    adHelp 배포 파일 준비 스크립트         ║" -ForegroundColor Magenta
    Write-Host "║       (빌드 + 파일 생성 only)             ║" -ForegroundColor Magenta
    Write-Host "╚═══════════════════════════════════════════╝" -ForegroundColor Magenta

    # 1. 사전 조건 확인
    Test-Prerequisites

    # 2. 버전 추출
    $version = Get-AppVersion

    # 3. 배포 확인
    Write-Host ""
    Write-Host "  ┌─────────────────────────────────────┐" -ForegroundColor White
    Write-Host "  │  배포할 버전: v$version              " -ForegroundColor White
    Write-Host "  └─────────────────────────────────────┘" -ForegroundColor White
    Write-Host ""
    
    $confirm = Read-Host "  이 버전으로 배포 준비를 하시겠습니까? (Y/N)"
    if ($confirm -notin @('Y', 'y', 'yes', 'Yes')) {
        Write-Info "작업이 취소되었습니다."
        return
    }

    # 4. Release 빌드
    $exePath = Invoke-Build

    # 5. 체크섬 계산
    $checksum = Get-FileChecksum -FilePath $exePath

    # 6. 배포 파일 생성 (adHelp-updates 폴더)
    Prepare-UpdatesFolder -Version $version -ExePath $exePath -Checksum $checksum

    # 완료
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║       배포 파일 준비가 완료되었습니다!    ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Info "파일 위치: $Script:UpdatesDir"
    Write-Host "  1. adHelp.exe"
    Write-Host "  2. UpdateInfo.xml"
    Write-Host "  3. README.md"
    Write-Host ""
    Write-Host "  [안내] 위 폴더로 이동하여 Git Commit & Push를 수동으로 진행해주세요." -ForegroundColor Cyan
    Write-Host ""
}

# 실행
Main
