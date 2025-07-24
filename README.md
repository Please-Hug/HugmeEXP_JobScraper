# JobScraper System

![Development Status](https://img.shields.io/badge/Status-In%20Development-yellow)
![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![MySQL](https://img.shields.io/badge/Database-MySQL-orange)
![RabbitMQ](https://img.shields.io/badge/Messaging-RabbitMQ-red)

**⚠️ 주의: 학습용 프로젝트로만 사용하세요 ⚠️**
> 이 프로젝트는 교육 및 학습 목적으로만 만들어졌습니다. 
> C# 기반 데이터 처리 파이프라인, API 통합, 스크래핑 봇 기술을 데모합니다.
> 실제 서비스에 사용하지 마세요.

채용공고 스크래핑 및 관리를 위한 시스템입니다.

> ⚠️ **개발 중인 프로젝트입니다.** 일부 기능이 완전히 구현되지 않았을 수 있습니다.

## 📋 구현 상태

| 컴포넌트 | 상태    | 설명                                    |
|---------|-------|---------------------------------------|
| 🏗️ Core Architecture | ✅ 완료  | 도메인 모델, 인터페이스                         |
| 🗄️ Database Layer | ✅ 완료  | Entity Framework, MySQL 연동, 다대다 관계 지원 |
| 🔄 Messaging System | ✅ 완료  | RabbitMQ 기반 비동기 메시징                   |
| 🌐 REST API Server | ✅ 완료  | 14개 엔드포인트, Swagger UI                 |
| 🤖 Bot Service | ✅ 완료  | Worker, 메시지 처리, 외부 사이트 스크래핑           |
| 🕷️ Web Scrapers | ✅ 완료 | 스크래퍼 구현 완료                            |

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
│                 │                │                 │
└─────────────────┘                └─────────────────┘
```

## 📁 프로젝트 구조 및 역할

### JobScraper.Core
- 도메인 모델: JobListing, JobDetail, Skill 등
- 서비스/리포지토리 인터페이스
- 메시징 명령 객체, 결과 객체, 열거형

### JobScraper.Infrastructure.Data
- Entity Framework 기반 데이터 액세스
- DbContext, Repository, MySQL 연동, 다대다 관계

### JobScraper.Infrastructure.Messaging
- RabbitMQ 메시징
- IQueueClient, RabbitMQClient 구현
- 비동기 명령 전송/수신

### JobScraper.Infrastructure.Http
- 외부 API 호출용 HTTP 클라이언트


### 주요 테이블

- **`JobListings`**: 채용공고 기본 정보
- **`JobDetails`**: 채용공고 상세 정보
- **`Skills`**: 기술 스킬 목록
- **`JobDetailEntitySkillEntity`**: JobDetail-Skill 다대다 관계

### 관계

- `JobListings` (1) ↔ (1) `JobDetails`
- `JobDetails` (M) ↔ (N) `Skills`
- `JobDetails` (M) ↔ (N) `Tags`

## 🛠️ 기술 스택

- **Backend**: ASP.NET Core 9.0
- **Database**: MySQL with Entity Framework Core
- **Messaging**: RabbitMQ
- **API Documentation**: OpenAPI/Swagger
- **HTTP Client**: HttpClient
- **Logging**: Microsoft.Extensions.Logging
- **Compression**: Brotli + Gzip
- **External Libraries**:
    - `Newtonsoft.Json` for JSON serialization / deserialization
    - `HTML Agility Pack` for HTML parsing
    - `Kakao Map API` for address to coordinates conversion

## 📊 주요 기능

### 🔍 스크래핑 기능
- **다중 소스 지원**: Wanted, Jumpit 등 ✅
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

### Phase 1 - 기본 기능 구현 ✅

### Phase 2 - 기능 확장 🚧
- [ ] RabbitMQ를 활용해 백엔드 서버에 결과 전송
- [ ] 관리자 대시보드 웹 인터페이스
- [ ] AI 기반 채용공고 분석 시스템 (학습되지 않도록 주의)
- [ ] 데이터 분석 및 시각화

### Phase 3 - 최적화 ❌
- [ ] 캐싱 시스템 구현
- [ ] 성능 모니터링
- [ ] 자동 테스트 커버리지 확대
- [ ] CI/CD 파이프라인 구축