# Office Manager - 공유오피스 관리 시스템 (WPF)

## 📋 프로젝트 개요

공유오피스의 상주/비상주 업체를 관리하고, 입금/지출 내역을 추적하며, 미수금을 관리하는 Windows 데스크톱 애플리케이션입니다.

## ✨ 주요 기능

### 1. 대시보드
- 월별 입금/지출/순이익 통계
- 활성 업체 수 및 미수금 현황
- 빠른 작업 메뉴

### 2. 업체 관리
- 업체 추가/수정/삭제
- 상주/비상주 구분 관리
- 계약일자, 월이용료, 담당자 정보 관리
- 검색 및 필터 기능

### 3. 입금/지출 관리
- 월별 입금 내역 관리
- 월별 지출 내역 관리
- Excel 파일 가져오기/내보내기
- 입금액/지출액 통계

### 4. 미수금 관리
- 월별 미수금 현황 자동 계산
- 미납 업체 목록 표시
- 미수금액 및 건수 통계

### 5. Excel 연동
- 업체/입금/지출 데이터 Excel 내보내기
- Excel 파일에서 데이터 가져오기
- 3개 시트 자동 생성 (업체목록, 입금내역, 지출내역)

## 🛠️ 기술 스택

- **.NET 6.0** - 최신 .NET 플랫폼
- **WPF (Windows Presentation Foundation)** - Windows 데스크톱 UI
- **MVVM 패턴** - Model-View-ViewModel 아키텍처
- **SQLite** - 로컬 데이터베이스 (System.Data.SQLite)
- **ClosedXML** - Excel 파일 처리 라이브러리

## 📦 필수 요구사항

### 개발 환경
- **Windows 10 이상**
- **Visual Studio 2022** (Community Edition 이상)
  - .NET Desktop Development 워크로드 설치 필요
- **.NET 6.0 SDK** (Visual Studio 설치 시 포함)

### 실행 환경
- **Windows 10 이상**
- **.NET 6.0 Runtime** (없으면 자동 설치 안내)

## 🚀 빌드 및 실행 방법

### 1. Visual Studio에서 빌드

1. **Visual Studio 2022 실행**
2. **프로젝트 열기**:
   - `파일` → `프로젝트/솔루션 열기`
   - `OfficeManagerWPF.csproj` 선택
3. **NuGet 패키지 복원**:
   - Visual Studio가 자동으로 패키지 복원
   - 수동 복원: `도구` → `NuGet 패키지 관리자` → `솔루션용 NuGet 패키지 관리`
4. **빌드**:
   - `F6` 키 또는 `빌드` → `솔루션 빌드`
5. **실행**:
   - `F5` 키 또는 `디버그` → `디버깅 시작`

### 2. 명령줄에서 빌드

```bash
# 프로젝트 디렉토리로 이동
cd OfficeManagerWPF

# NuGet 패키지 복원
dotnet restore

# 빌드
dotnet build --configuration Release

# 실행
dotnet run --configuration Release
```

### 3. 독립 실행 파일 생성 (배포용)

```bash
# 자체 포함 실행 파일 생성 (런타임 포함)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# 생성된 파일 위치: bin/Release/net6.0-windows/win-x64/publish/OfficeManager.exe
```

## 📂 프로젝트 구조

```
OfficeManagerWPF/
├── Models/                      # 데이터 모델
│   ├── Company.cs               # 업체 모델
│   ├── Payment.cs               # 입금 모델
│   └── Expense.cs               # 지출 모델
├── Services/                    # 비즈니스 로직
│   ├── DatabaseService.cs       # SQLite 데이터베이스 관리
│   └── ExcelService.cs          # Excel 입출력 처리
├── ViewModels/                  # MVVM ViewModel
│   ├── ViewModelBase.cs         # ViewModel 기본 클래스
│   ├── RelayCommand.cs          # Command 구현
│   └── MainViewModel.cs         # 메인 화면 ViewModel
├── Views/                       # XAML UI
│   ├── MainWindow.xaml          # 메인 대시보드
│   ├── CompanyManagementWindow.xaml    # 업체 관리
│   ├── PaymentExpenseWindow.xaml       # 입금/지출 관리
│   └── UnpaidManagementWindow.xaml     # 미수금 관리
├── Resources/                   # 리소스 파일
├── App.xaml                     # 애플리케이션 진입점
└── OfficeManagerWPF.csproj      # 프로젝트 파일
```

## 💾 데이터베이스

### 저장 위치
```
%LocalAppData%\OfficeManager\OfficeManager.db
```
일반적인 경로: `C:\Users\[사용자명]\AppData\Local\OfficeManager\OfficeManager.db`

### 테이블 구조

**Companies** (업체)
- Id (INTEGER PRIMARY KEY)
- Name (TEXT)
- Type (TEXT) - "상주" 또는 "비상주"
- ContractDate (TEXT)
- MonthlyFee (REAL)
- ContactPerson (TEXT)
- PhoneNumber (TEXT)
- Email (TEXT)
- Notes (TEXT)
- IsActive (INTEGER)

**Payments** (입금)
- Id (INTEGER PRIMARY KEY)
- CompanyId (INTEGER)
- CompanyName (TEXT)
- PaymentDate (TEXT)
- Amount (REAL)
- Period (TEXT) - "yyyy-MM" 형식
- PaymentMethod (TEXT)
- Notes (TEXT)
- IsConfirmed (INTEGER)

**Expenses** (지출)
- Id (INTEGER PRIMARY KEY)
- ExpenseDate (TEXT)
- Category (TEXT)
- Amount (REAL)
- Description (TEXT)
- Period (TEXT) - "yyyy-MM" 형식
- Notes (TEXT)

## 📋 Excel 파일 형식

### 내보내기 파일 구조
- **업체목록 시트**: 모든 등록 업체 정보
- **입금내역 시트**: 최근 12개월 입금 내역
- **지출내역 시트**: 최근 12개월 지출 내역

### 가져오기 형식
내보낸 Excel 파일과 동일한 형식으로 데이터를 가져올 수 있습니다.

## 🎨 UI 디자인

### 디자인 컨셉
- **미니멀 프리미엄** 스타일
- 깔끔한 흰색 배경 (#F5F5F5)
- 카드 기반 레이아웃
- 부드러운 모서리 (CornerRadius: 8-12)
- 미세한 그림자 효과

### 색상 팔레트
- **배경**: #F5F5F5 (연한 회색)
- **카드**: #FFFFFF (흰색)
- **텍스트**: #333333 (진한 회색)
- **보조 텍스트**: #999999 (회색)
- **입금**: #4CAF50 (초록색)
- **지출**: #F44336 (빨간색)
- **미수금**: #FF9800 (주황색)
- **강조**: #2196F3 (파란색)

## 🔧 문제 해결

### 빌드 오류

**NuGet 패키지 복원 실패**
```bash
# Visual Studio에서
도구 → NuGet 패키지 관리자 → 패키지 관리자 콘솔
PM> Update-Package -reinstall

# 명령줄에서
dotnet restore --force
```

**SQLite DLL 오류**
- `System.Data.SQLite.Core` 패키지가 자동으로 네이티브 DLL을 포함합니다
- 수동 설치가 필요한 경우: NuGet에서 재설치

**Excel 관련 오류**
- `ClosedXML` 패키지가 자동으로 의존성을 처리합니다
- `DocumentFormat.OpenXml` 패키지도 자동 설치됩니다

### 실행 오류

**데이터베이스 접근 오류**
- SQLite DB 파일의 권한을 확인하세요
- `%LocalAppData%\OfficeManager` 폴더 권한 확인

**.NET 런타임 없음**
- [.NET 6.0 Runtime 다운로드](https://dotnet.microsoft.com/download/dotnet/6.0)
- Desktop Runtime 설치 필요

## 📝 라이선스

이 프로젝트는 개인 및 상업적 용도로 자유롭게 사용할 수 있습니다.

## 🤝 기여

버그 리포트나 기능 제안은 언제든지 환영합니다!

## 📧 연락처

프로젝트 관련 문의사항이 있으시면 연락 주세요.

---

**개발 환경**: Visual Studio 2022, .NET 6.0, WPF  
**테스트 환경**: Windows 10/11  
**마지막 업데이트**: 2025-02-14
