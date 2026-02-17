# Office Manager - 공유오피스 관리 시스템 (WPF + Visual Studio 스타일)

**Visual Studio Dark Theme UI/UX + SMS/이메일 자동 알림 시스템**이 통합된 공유오피스 관리 프로그램입니다.

---

## 🎨 주요 특징

### 1️⃣ Visual Studio 스타일 UI/UX
- **어두운 테마**: Visual Studio Dark Theme (#1E1E1E, #252526)
- **파란 액센트**: VS Blue (#007ACC)
- **전문적인 사이드바**: 메뉴 구조와 아이콘
- **DataGrid 최적화**: 가독성 높은 테이블 디자인

### 2️⃣ SMS/이메일 자동 알림 시스템
#### 📅 미수금 알림 (월 3회)
- **월초 (1일)**: 미수금 업체에 납입 안내
- **월중순 (15일)**: 미수금 업체에 재안내
- **월말 (말일)**: 최종 납입 독촉

#### 💰 월세 납입 알림 (월 3회)
- **7일 전**: 월세 납입 사전 안내
- **3일 전**: 월세 납입 리마인드
- **당일**: 월세 납입 당일 안내

### 3️⃣ 핵심 기능
- ✅ 업체 관리 (상주/바상주, 계약일자, 월세)
- ✅ 입금/지출 관리 (월별 집계)
- ✅ 미수금 자동 계산 및 추적
- ✅ Excel 입출력 (업체, 입금, 지출 데이터)
- ✅ 대시보드 통계 (총 입금액, 지출액, 순이익)
- ✅ 자동 알림 스케줄러 (백그라운드 실행)

---

## 📦 기술 스택

- **프레임워크**: .NET 6.0 WPF
- **패턴**: MVVM (Model-View-ViewModel)
- **데이터베이스**: SQLite (System.Data.SQLite 1.0.118)
- **Excel**: ClosedXML 0.102.2
- **이메일**: System.Net.Mail (SMTP)
- **SMS**: HTTP API 연동 (알리고, 카카오 알림톡 등)

---

## 🚀 빌드 및 실행

### 요구사항
- **Windows 10 이상**
- **Visual Studio 2022**
- **.NET 6.0 SDK** ([다운로드](https://dotnet.microsoft.com/download/dotnet/6.0))

### 빌드 방법

#### 1️⃣ Visual Studio에서 빌드
```bash
# 프로젝트 열기
OfficeManagerWPF.csproj를 Visual Studio 2022에서 열기

# NuGet 패키지 복원
도구 → NuGet 패키지 관리자 → 솔루션용 NuGet 패키지 관리 → 복원

# 빌드 (F6)
빌드 → 솔루션 빌드

# 실행 (F5)
디버그 → 디버깅 시작
```

#### 2️⃣ 명령줄에서 빌드
```bash
# NuGet 복원
dotnet restore

# 빌드
dotnet build --configuration Release

# 실행
dotnet run
```

#### 3️⃣ 단일 실행 파일 배포
```bash
# Self-contained 단일 파일로 배포
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# 출력 경로: bin/Release/net6.0-windows/win-x64/publish/OfficeManager.exe
```

---

## 📋 프로젝트 구조

```
OfficeManagerWPF/
├─ Models/                          # 데이터 모델
│  ├─ Company.cs                    # 업체 모델
│  ├─ Payment.cs                    # 입금 모델
│  └─ Expense.cs                    # 지출 모델
│
├─ Services/                        # 비즈니스 로직
│  ├─ DatabaseService.cs            # SQLite 데이터베이스 서비스
│  ├─ ExcelService.cs               # Excel 입출력 서비스
│  ├─ SmsService.cs                 # SMS 발송 서비스
│  ├─ EmailService.cs               # 이메일 발송 서비스
│  └─ NotificationSchedulerService.cs  # 자동 알림 스케줄러
│
├─ ViewModels/                      # MVVM 뷰모델
│  ├─ ViewModelBase.cs              # Base ViewModel
│  ├─ RelayCommand.cs               # Command 구현
│  └─ MainViewModel.cs              # 메인 ViewModel
│
├─ Views/                           # UI 화면
│  ├─ MainWindow.xaml               # 메인 화면 (대시보드)
│  ├─ CompanyManagementWindow.xaml  # 업체 관리
│  ├─ PaymentExpenseWindow.xaml     # 입금/지출 관리
│  ├─ UnpaidManagementWindow.xaml   # 미수금 관리
│  ├─ NotificationSettingsWindow.xaml  # 알림 설정
│  └─ NotificationLogsWindow.xaml   # 알림 내역
│
├─ Properties/                      # 앱 설정
│  ├─ Settings.settings             # 사용자 설정
│  └─ Settings.Designer.cs          # 설정 코드
│
├─ App.xaml                         # 앱 전역 리소스 (VS 테마)
├─ App.xaml.cs                      # 앱 시작/종료 로직
└─ OfficeManagerWPF.csproj          # 프로젝트 파일
```

---

## 💾 데이터베이스 구조

**위치**: `C:\Users\[사용자명]\AppData\Local\OfficeManager\OfficeManager.db`

### Companies 테이블
| 컬럼 | 타입 | 설명 |
|------|------|------|
| Id | INTEGER PRIMARY KEY | 업체 ID |
| Name | TEXT | 업체명 |
| Type | TEXT | 상주/바상주 |
| ContractDate | TEXT | 계약일자 |
| MonthlyFee | REAL | 월세 |
| ContactPerson | TEXT | 담당자 |
| ContactPhone | TEXT | 전화번호 |
| ContactEmail | TEXT | 이메일 |
| Notes | TEXT | 비고 |
| IsActive | INTEGER | 활성 여부 |

### Payments 테이블
| 컬럼 | 타입 | 설명 |
|------|------|------|
| Id | INTEGER PRIMARY KEY | 입금 ID |
| CompanyId | INTEGER | 업체 ID (FK) |
| PaymentDate | TEXT | 입금일자 |
| Amount | REAL | 입금액 |
| Period | TEXT | 대상기간 (yyyy-MM) |
| Notes | TEXT | 비고 |

### Expenses 테이블
| 컬럼 | 타입 | 설명 |
|------|------|------|
| Id | INTEGER PRIMARY KEY | 지출 ID |
| ExpenseDate | TEXT | 지출일자 |
| Category | TEXT | 카테고리 |
| Amount | REAL | 지출액 |
| Description | TEXT | 설명 |
| Period | TEXT | 대상기간 (yyyy-MM) |
| Notes | TEXT | 비고 |

---

## 📧 SMS/이메일 설정

### 1️⃣ Gmail 이메일 설정
1. Google 계정 → **보안** → **2단계 인증** 활성화
2. **앱 비밀번호** 생성 ([링크](https://myaccount.google.com/apppasswords))
3. 알림 설정 화면에서 입력:
   - SMTP 서버: `smtp.gmail.com`
   - 포트: `587`
   - 발신 이메일: `your-email@gmail.com`
   - 비밀번호: `앱 비밀번호 16자리`

### 2️⃣ SMS API 설정
- **알리고 SMS**: https://smartsms.aligo.in
- **카카오 알림톡**: https://business.kakao.com
- **네이버 클라우드 SMS**: https://www.ncloud.com

알림 설정 화면에서 API 키와 발신번호를 입력하세요.

---

## ⚙️ 자동 알림 스케줄링

### 알림 발송 일정
- **실행 시간**: 매일 오전 9시 (프로그램 실행 시 자동 시작)
- **미수금 알림**: 월 1일, 15일, 말일
- **월세 알림**: 계약일 기준 7일 전, 3일 전, 당일

### 알림 비활성화 방법
```csharp
// Properties/Settings.settings에서 변경
AutoNotificationEnabled = false
```

---

## 🎨 UI/UX 색상 팔레트

| 색상 | Hex Code | 용도 |
|------|----------|------|
| VS 배경 | #1E1E1E | 메인 배경 |
| VS 사이드바 | #252526 | 사이드바 배경 |
| VS 액센트 | #007ACC | 버튼, 강조 |
| VS 텍스트 | #D4D4D4 | 주 텍스트 |
| VS 테두리 | #3F3F46 | 테두리 |
| 수입 녹색 | #4EC9B0 | 입금액 |
| 지출 빨강 | #F48771 | 지출액 |
| 이익 파랑 | #569CD6 | 순이익 |

---

## 📝 사용 방법

### 1️⃣ 업체 등록
1. 좌측 메뉴 → **업체 관리** 클릭
2. **업체 추가** 버튼 클릭
3. 업체 정보 입력 (이름, 타입, 계약일, 월세, 연락처 등)
4. **저장** 클릭

### 2️⃣ 입금 기록
1. 좌측 메뉴 → **입금/지출 관리** 클릭
2. **입금** 탭 선택
3. 업체 선택, 입금일, 금액, 대상기간 입력
4. **추가** 클릭

### 3️⃣ 미수금 확인
1. 좌측 메뉴 → **미수금 관리** 클릭
2. 월별 미수금 자동 계산
3. 업체별 납입 현황 확인

### 4️⃣ Excel 내보내기
1. **입금/지출 관리** 화면 우측 상단
2. **Excel 내보내기** 버튼 클릭
3. `office_manager_YYYYMMDD.xlsx` 파일 생성

### 5️⃣ 알림 설정
1. 좌측 메뉴 → **알림 설정** 클릭
2. SMS/이메일 API 설정 입력
3. **저장** 클릭
4. **테스트 발송** 버튼으로 확인

---

## 🔧 문제 해결

### NuGet 패키지 복원 오류
```bash
# Visual Studio에서
도구 → 옵션 → NuGet 패키지 관리자 → 패키지 소스 → 확인

# 명령줄에서
dotnet nuget add source https://api.nuget.org/v3/index.json
dotnet restore
```

### .NET 런타임 오류
- [.NET 6.0 Desktop Runtime 다운로드](https://dotnet.microsoft.com/download/dotnet/6.0)

### SQLite 오류
- `System.Data.SQLite.Core` 패키지가 자동으로 네이티브 DLL을 포함합니다.
- 수동 설치 필요 없음

---

## 📊 비교: Flutter 웹 vs WPF 데스크톱

| 항목 | Flutter 웹 | WPF 데스크톱 |
|------|-----------|--------------|
| 플랫폼 | 웹 브라우저 (크로스플랫폼) | Windows 전용 |
| 성능 | 보통 (브라우저 엔진) | 매우 빠름 (네이티브) |
| 오프라인 | LocalStorage (제한적) | SQLite (완전 지원) |
| SMS/이메일 | 백엔드 필요 | 직접 API 호출 가능 |
| 배포 | 웹 서버 필요 | .exe 파일 단독 실행 |
| UI/UX | Material Design | Visual Studio 스타일 |
| 자동 알림 | 백엔드 스케줄러 필요 | Windows Task Scheduler |

---

## 📄 라이선스

이 프로젝트는 교육 및 상업적 용도로 자유롭게 사용 가능합니다.

---

## 👨‍💻 개발 정보

- **프로젝트명**: Office Manager WPF
- **버전**: 2.0.0 (Visual Studio Theme + Auto Notification)
- **개발 환경**: .NET 6.0, Visual Studio 2022
- **데이터베이스**: SQLite 3
- **UI 프레임워크**: WPF (Windows Presentation Foundation)

---

## 📞 지원

문제가 발생하거나 기능 요청이 있으시면 이슈를 등록해주세요.

**주요 개선사항 (v2.0.0)**:
- ✅ Visual Studio Dark Theme 적용
- ✅ SMS/이메일 자동 알림 시스템
- ✅ 미수금 월 3회 자동 안내
- ✅ 월세 납입 3회 자동 안내
- ✅ 백그라운드 스케줄러 서비스
- ✅ 알림 설정 화면 추가
- ✅ 알림 발송 내역 화면 추가
