# GitHub Pages Public Repository 설정 가이드

## 1. 새로운 Public Repository 생성

1. GitHub에서 새 Repository 생성:
   - **Repository name**: `adHelp-updates`
   - **Visibility**: Public ✅
   - **Initialize with README**: 체크 해제 (우리가 직접 업로드)

2. 로컬에서 설정:
   ```bash
   cd D:\repos\adHelp\public-updates
   git init
   git add .
   git commit -m "Initial commit: Update server setup"
   git branch -M main
   git remote add origin https://github.com/opti12/adHelp-updates.git
   git push -u origin main
   ```

## 2. GitHub Pages 활성화

1. adHelp-updates Repository → Settings → Pages
2. Source: Deploy from a branch
3. Branch: main
4. Folder: / (root)
5. Save

## 3. 접근 URL 확인

- **UpdateInfo.xml**: `https://opti12.github.io/adHelp-updates/UpdateInfo.xml`
- **웹페이지**: `https://opti12.github.io/adHelp-updates/`

## 4. 릴리스 프로세스

### 새 버전 릴리스 시:

1. **메인 프로젝트 (Private)**에서:
   - 버전 업데이트
   - 빌드 및 서명
   - GitHub Release 생성 (adHelp.exe 업로드)

2. **업데이트 서버 (Public)**에서:
   - UpdateInfo.xml 수정 (버전, 체크섬)
   - 커밋 및 푸시

## 5. 보안 고려사항

- ✅ 소스 코드는 Private Repository에서 안전하게 보관
- ✅ 업데이트 정보만 Public으로 노출
- ✅ 실제 파일은 Private Repository의 Releases에서 다운로드
- ✅ SHA256 체크섬으로 파일 무결성 검증

## 장점

- 무료 (Public Repository)
- 간단한 설정
- 높은 가용성 (GitHub Pages)
- 소스 코드 보안 유지
