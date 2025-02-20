🐜 개미 키우기 (Ant Farming) - 주식 모의투자 프로그램
프로젝트 소개
이 프로젝트는 C#으로 개발된 주식 모의투자 프로그램입니다. '거지 키우기'와 유사한 컨셉으로, 사용자가 가상의 자금으로 실제 암호화폐 시장의 데이터를 기반으로 투자 경험을 쌓을 수 있는 교육용 프로그램입니다.
그때 사용한 모의투자 프로그램 코드입니다 

개발 언어는 C#입니다. 

팀원들과 깃에 대한 대화가 없이 끝나서 제 코드만 업로드 했습니다.

주요 기능

실시간 암호화폐 시세 조회 (Upbit API 연동)
가상 계좌를 통한 매수/매도 기능
실시간 차트 시각화
포트폴리오 관리 및 수익률 확인
다양한 암호화폐 마켓 지원 (BTC, ETH, XRP, DOGE, SOL 등)

기술 스택

개발 언어: C# (.NET Framework 4.7.2)
UI Framework: Windows Forms
차트 라이브러리: Windows Forms Chart Control
통신: WCF (Windows Communication Foundation)
데이터베이스: MySQL
외부 API: Upbit API
JSON 처리: Newtonsoft.Json

프로젝트 구조
Copy└── 프로젝트 root/
    ├── Service/
    │   ├── ITradeService.cs         # 거래 서비스 인터페이스
    │   └── TradeWCFService.cs       # 거래 서비스 구현
    ├── Models/
    │   ├── TradeInfo.cs            # 거래 정보 모델
    │   ├── TradeRecord.cs          # 거래 기록 모델
    │   └── UpbitTicker.cs          # Upbit API 응답 모델
    ├── UI/
    │   ├── Form1.cs                # 메인 폼 구현
    │   └── ChartManager.cs         # 차트 관리 클래스
    └── Program.cs                  # 프로그램 진입점
주요 기능 상세
1. 실시간 시세 조회

Upbit API를 통한 실시간 가격 정보 수신
5분 간격의 캔들 데이터 차트 표시
가격 변동에 따른 실시간 UI 업데이트

2. 거래 기능

실시간 매수/매도 기능
보유 자산 및 수익률 계산
거래 이력 관리

3. 사용자 인터페이스

직관적인 거래 인터페이스
실시간 차트 시각화
포트폴리오 현황 표시
간편한 수량 조절 기능

설치 및 실행 방법

프로젝트 클론
Visual Studio에서 솔루션 파일 열기
NuGet 패키지 복원
MySQL 데이터베이스 설정
프로그램 실행

사용된 외부 라이브러리

Newtonsoft.Json v13.0.3
MySql.Data v9.1.0
System.ServiceModel (WCF)
Windows Forms Chart Control

향후 개선 사항

사용자 인증 시스템 추가
더 다양한 차트 지표 추가
거래 알림 기능 구현
백테스팅 기능 추가
실시간 호가 데이터 표시
