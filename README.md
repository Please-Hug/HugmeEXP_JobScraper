# JobScraper System

![Development Status](https://img.shields.io/badge/Status-In%20Development-yellow)
![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![MySQL](https://img.shields.io/badge/Database-MySQL-orange)
![RabbitMQ](https://img.shields.io/badge/Messaging-RabbitMQ-red)

채용공고 스크래핑 및 관리를 위한 시스템입니다.

> ⚠️ **개발 중인 프로젝트입니다.** 일부 기능이 완전히 구현되지 않았을 수 있습니다.

## 📋 구현 상태

| 컴포넌트 | 상태 | 설명 |
|---------|------|------|
| 🏗️ Core Architecture | ✅ 완료 | 도메인 모델, 인터페이스 |
| 🗄️ Database Layer | ✅ 완료 | Entity Framework, MySQL 연동 |
| 🔄 Messaging System | ✅ 완료 | RabbitMQ 기반 비동기 메시징 |
| 🌐 REST API Server | ✅ 완료 | 14개 엔드포인트, Swagger UI |
| 🤖 Bot Service | ✅ 완료 | Worker, 메시지 처리 |
| 🕷️ Web Scrapers | 🚧 진행중 | Wanted, Jumpit 스크래퍼 구현 필요 |
| 📊 Admin Dashboard | ❌ 미구현 | 관리자 웹 인터페이스 |
| 🔍 Search & Filter | ❌ 미구현 | 고급 검색 기능 |

## 🏗️ 아키텍처

```
┌─────────────────┐    RabbitMQ    ┌─────────────────┐
│   JobScraper    │ ─────────────► │   JobScraper    │
│     Server      │                │      Bot        │
│   (API Server)  │ ◄───────────── │   (Scraper)     │
└─────────────────┘    HTTP API    └─────────────────┘
         │                                   │
         ▼                                   ▼
┌─────────────────┐                ┌─────────────────┐
│     MySQL       │                │   External      │
│   Database      │                │   Job Sites     │
│                 │                │ (Wanted, Jumpit)│
└─────────────────┘                └─────────────────┘
```

## 📁 프로젝트 구조

### Core 프로젝트 (`JobScraper.Core`)
- **Models**: `JobListing`, `JobDetail`, `Skill` 도메인 모델
- **Interfaces**: 서비스 및 리포지토리 인터페이스
- **Commands**: `ScrapingCommand` 메시징용 명령 객체
- **Results**: `ScrapingResult` 스크래핑 결과 객체
- **Enums**: `CommandType` 등 열거형

### Infrastructure 프로젝트들
- **`JobScraper.Infrastructure.Data`**: Entity Framework 기반 데이터 액세스
  - Entity 모델, DbContext, Repository 구현
  - MySQL 데이터베이스 연동
  - 다대다 관계 지원 (JobDetail ↔ Skill)

- **`JobScraper.Infrastructure.Messaging`**: RabbitMQ 메시징
  - `IQueueClient` 인터페이스 및 `RabbitMQClient` 구현
  - 비동기 명령 전송/수신

- **`JobScraper.Infrastructure.Http`**: HTTP 클라이언트
  - 외부 API 호출용 HTTP 클라이언트

### Server 프로젝트 (`JobScraper.Server`)
REST API 서버로 다음 기능을 제공합니다:

#### 📋 API 엔드포인트

**채용공고 관리** (`/api/joblisting`)
- `GET /api/joblisting` - 모든 채용공고 조회
- `GET /api/joblisting/{id}` - 특정 채용공고 조회
- `GET /api/joblisting/by-url?url=...` - URL로 채용공고 조회
- `GET /api/joblisting/by-source/{source}` - 소스별 채용공고 조회
- `POST /api/joblisting` - 새 채용공고 생성
- `PUT /api/joblisting/{id}` - 채용공고 수정
- `DELETE /api/joblisting/{id}` - 채용공고 삭제

**채용 상세정보 관리** (`/api/jobdetail`)
- `GET /api/jobdetail/{id}` - 상세정보 조회
- `POST /api/jobdetail` - 상세정보 생성
- `POST /api/jobdetail/with-skills` - 스킬과 함께 상세정보 생성
- `PUT /api/jobdetail/{id}` - 상세정보 수정
- `POST /api/jobdetail/{jobDetailId}/skills/{skillId}` - 스킬 추가
- `DELETE /api/jobdetail/{jobDetailId}/skills/{skillId}` - 스킬 제거

**스킬 관리** (`/api/skill`)
- `GET /api/skill` - 모든 스킬 조회
- `GET /api/skill/{id}` - 특정 스킬 조회
- `GET /api/skill/by-name/{name}` - 이름으로 스킬 조회
- `POST /api/skill/get-or-create` - 스킬 조회/생성
- `POST /api/skill/get-or-create-batch` - 여러 스킬 일괄 처리

**스크래핑 제어** (`/api/scraping`) ⚠️ *스크래퍼 구현 필요*
- `POST /api/scraping/start-job-listings` - 채용공고 목록 스크래핑 시작
- `POST /api/scraping/start-job-detail` - 특정 채용공고 상세정보 스크래핑
- `POST /api/scraping/start-bulk-scraping` - 다중 소스 일괄 스크래핑

**결과 수신** (`/api/result`)
- `POST /api/result/scraping-result` - Bot이 스크래핑 결과 전송
- `GET /api/result/health` - 서버 상태 확인

### Bot 프로젝트 (`JobScraper.Bot`)
백그라운드 스크래핑 서비스로 다음 기능을 제공합니다:

- **Worker 서비스**: RabbitMQ에서 스크래핑 명령 수신 ✅
- **Scrapers**: Wanted, Jumpit 등 채용사이트 스크래퍼 🚧 *구현 필요*
- **결과 전송**: HTTP API를 통해 Server로 결과 전송 ✅

## 🚀 시작하기

### 사전 요구사항

- .NET 9.0 SDK
- MySQL Server (localhost:3307)
- RabbitMQ Server (localhost:5672)

### 설정

1. **프로젝트 클론**
   ```bash
   git clone <repository-url>
   cd JobScraperSystem
   ```

2. **데이터베이스 설정**
   
   `JobScraper.Server/appsettings.json` 생성:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=3307;Database=job_scrap;Uid=your_username;Pwd=your_password;"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*"
   }
   ```

   `JobScraper.Bot/appsettings.json` 생성:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "ResultEndpoint": "http://localhost:5058/api/Result/scraping-result"
   }
   ```

3. **데이터베이스 마이그레이션**
   ```bash
   cd JobScraper.Server
   dotnet ef database update --project ../JobScraper.Infrastructure.Data
   ```

### 실행

1. **Server 실행**
   ```bash
   cd JobScraper.Server
   dotnet run
   ```
   - Swagger UI: `https://localhost:5001/swagger`

2. **Bot 실행**
   ```bash
   cd JobScraper.Bot
   dotnet run
   ```

## 🔄 사용 예시

> ⚠️ **참고**: 스크래퍼가 아직 완전히 구현되지 않았으므로 실제 데이터 수집은 제한적일 수 있습니다.

### 스크래핑 명령 전송

**Wanted에서 "백엔드 개발자" 검색:**
```bash
curl -X POST "https://localhost:5001/api/scraping/start-job-listings" \
  -H "Content-Type: application/json" \
  -d '{
    "source": "wanted",
    "searchParameters": {
      "keyword": "백엔드 개발자",
      "page": 1
    }
  }'
```

**다중 소스 일괄 스크래핑:**
```bash
curl -X POST "https://localhost:5001/api/scraping/start-bulk-scraping" \
  -H "Content-Type: application/json" \
  -d '{
    "sources": ["wanted", "jumpit"],
    "searchParameters": {
      "keyword": "프론트엔드 개발자"
    }
  }'
```

### 데이터 조회

**모든 채용공고 조회:**
```bash
curl -X GET "https://localhost:5001/api/joblisting"
```

**특정 스킬의 채용공고 조회:**
```bash
curl -X GET "https://localhost:5001/api/skill/by-name/React"
```

## 🗄️ 데이터베이스 스키마

### 주요 테이블

- **`JobListings`**: 채용공고 기본 정보
- **`JobDetails`**: 채용공고 상세 정보
- **`Skills`**: 기술 스킬 목록
- **`JobDetailEntitySkillEntity`**: JobDetail-Skill 다대다 관계

### 관계

- `JobListings` (1) ↔ (1) `JobDetails`
- `JobDetails` (M) ↔ (N) `Skills`

## 🛠️ 기술 스택

- **Backend**: ASP.NET Core 9.0
- **Database**: MySQL with Entity Framework Core
- **Messaging**: RabbitMQ
- **API Documentation**: OpenAPI/Swagger
- **HTTP Client**: HttpClient
- **Logging**: Microsoft.Extensions.Logging
- **Compression**: Brotli + Gzip

## 📊 주요 기능

### 🔍 스크래핑 기능
- **다중 소스 지원**: Wanted, Jumpit 등 (구현 필요)
- **비동기 처리**: RabbitMQ 기반 메시징 ✅
- **중복 방지**: URL 기반 중복 체크 ✅
- **오류 처리**: 실패한 작업에 대한 로깅 및 재시도 ✅

### 📈 데이터 관리
- **스킬 정규화**: 자동 스킬 생성 및 매핑 ✅
- **다대다 관계**: 하나의 채용공고에 여러 스킬 연결 ✅
- **N+1 문제 해결**: 배치 쿼리 최적화 ✅
- **압축 지원**: 30-80% 응답 크기 감소 ✅

### 🔧 개발자 경험
- **Swagger UI**: API 문서 및 테스트 환경 ✅
- **타입 안전성**: .NET의 강력한 타입 시스템 활용 ✅
- **의존성 주입**: 클린 아키텍처 구현 ✅
- **로깅**: 구조화된 로깅으로 디버깅 지원 ✅

## 🚦 환경별 설정

### 개발 환경
- Hot reload 지원
- 상세한 로깅
- Swagger UI 활성화

### 프로덕션 환경
- 압축 활성화
- 최적화된 로깅
- 보안 헤더 추가

## 🔮 향후 개발 계획

### Phase 1 - 스크래퍼 완성 🚧
- [ ] wanted.co.kr 스크래퍼 구현
- [ ] jumpit.saramin.co.kr 스크래퍼 구현
- [ ] 스크래핑 에러 핸들링 강화
- [ ] 율분 제한 (Rate limiting) 구현

### Phase 2 - 기능 확장 ❌
- [ ] 관리자 대시보드 웹 인터페이스
- [ ] 고급 검색 및 필터링
- [ ] 채용공고 알림 기능
- [ ] 데이터 분석 및 시각화

### Phase 3 - 최적화 ❌
- [ ] 캐싱 시스템 구현
- [ ] 성능 모니터링
- [ ] 자동 테스트 커버리지 확대
- [ ] CI/CD 파이프라인 구축