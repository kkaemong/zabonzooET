<div align="center">

<img src="./Assets/Art/UI/Logo/logo.png" width="520" alt="자본주E.T. 로고" />

<br/><br/>

<h2>🛸 &nbsp; 지구에 불시착한 외계인 E.T. &nbsp; 🛸</h2>
<h3>시대별 경제 격변을 헤쳐나가며 UFO 수리 자금을 모아라!</h3>

<br/>

<img src="https://img.shields.io/badge/기간-2026.02.16_~_2026.04.03-4f46e5?style=for-the-badge" />
&nbsp;
<img src="https://img.shields.io/badge/팀원-4명-7c3aed?style=for-the-badge" />
&nbsp;
<img src="https://img.shields.io/badge/플랫폼-Unity_WebGL-000000?style=for-the-badge&logo=unity&logoColor=white" />

<br/><br/>

Unity · Spring Boot · AWS 풀스택 **2D 러너 금융 학습 게임** &nbsp;|&nbsp; 시대별 경제 스테이지 주행 · 금융 퀴즈 · 자산 증식

<br/><br/>

---

🏆 &nbsp; **SSAFY 특화 프로젝트 핀테크 트랙** &nbsp; — &nbsp; 🥈 **우수상 2위**

---

</div>


## 🔄 &nbsp;핵심 플레이 루프

```mermaid
graph TD
    A(["🛸 Lobby<br/>E.T. 불시착 & 스테이지 선택"]) --> B(["🏃 Stage Run<br/>2D 달리기 & 코인 획득"])
    B --> C{"⚡ 돌발 경제 퀴즈"}
    C -->|"✅ 정답"| D(["🚀 가속 부스터<br/>& 방어막 획득"])
    C -->|"❌ 오답"| E(["💔 체력 유실<br/>& 일반 주행"])
    D --> F(["📊 700m 완주<br/>결과 서버 전송"])
    E --> F
    F --> G(["💰 Finance Select<br/>시대별 금융 상품 투자"])
    G --> H(["🔧 Lobby<br/>자산 업데이트 & UFO 수리"])
    H --> A
```

<br/>

---

## 🎮 &nbsp;실제 플레이 화면

> 게임플레이 흐름 순서대로 구성된 실제 구동 화면입니다. 클릭하면 펼쳐집니다!

<br/>

<details>
<summary><b>🎬 &nbsp;1. 로비 &amp; 스테이지 선택 (Lobby &amp; Stage Selection)</b></summary>
<br/>

> E.T.가 지구에 불시착하고, 시대별 금융 흐름의 스테이지로 진입하는 메인 화면입니다.

| 🛸 로비 (메인 화면) | 📋 스테이지 선택 |
| :---: | :---: |
| <img src="./gifs/메인페이지.gif" height="260" /> | <img src="./gifs/스테이지_선택.gif" height="260" /> |

</details>

<br/>

<details>
<summary><b>🎬 &nbsp;2. 시대별 게임 주행 (Era Stages)</b></summary>
<br/>

> 1980년대 경제 급성장기 → 2000년대 IT 닷컴버블 격변기 → 2020년대 고인플레이션기까지, 시대별 맥락이 설계된 2D 러너 맵을 주행합니다.

| 📼 1980년대 · 대한민국 급성장기 | 💻 2000년대 · IT 정보화 &amp; 닷컴버블기 |
| :---: | :---: |
| <img src="./gifs/1980게임.gif" height="260" /> | <img src="./gifs/2000게임.gif" height="260" /> |

| 📈 2020년대 · 팬데믹 &amp; 고인플레이션기 |
| :---: |
| <img src="./gifs/2020게임.gif" height="260" /> |

</details>

<br/>

<details>
<summary><b>🎬 &nbsp;3. 인게임 기믹 &amp; 이벤트 (In-Game Gimmicks &amp; Events)</b></summary>
<br/>

> 장애물 피격 시 카메라가 흔들리며 HP가 차감됩니다. 경제 군인과 충돌하면 **돌발 퀴즈**가 열리며, 정답 시 가속 부스터 & 방어막을 획득합니다.

| 💥 장애물 피격 (카메라 셰이크) | ❓ 돌발 경제 퀴즈 등장 |
| :---: | :---: |
| <img src="./gifs/부딪혔을때.gif" height="260" /> | <img src="./gifs/퀴즈.gif" height="260" /> |

| 🛡️ 퀴즈 정답 → 보호막 &amp; 속도 부스트 | 💀 게임 오버 (체력 유실) |
| :---: | :---: |
| <img src="./gifs/퀴즈_정답_후_방어막.gif" height="260" /> | <img src="./gifs/게임오버.gif" height="260" /> |

</details>

<br/>

<details>
<summary><b>🎬 &nbsp;4. 자산 증식 &amp; 정비 (Finance &amp; Goal Achievement)</b></summary>
<br/>

> 주행으로 번 자산을 금융 상품에 굴리고, 상점에서 아이템을 사거나 UFO를 정비하여 탈출을 가속화합니다.

| 💹 금융 상품 가입 &amp; 자산 증식 결과 | 🛒 아이템 상점 구매 |
| :---: | :---: |
| <img src="./gifs/금융_상품_선택_후_결과.gif" height="260" /> | <img src="./gifs/상점.gif" height="260" /> |

| 🔧 UFO 정비 (최종 목표) |
| :---: |
| <img src="./gifs/UFO_정비.gif" height="260" /> |

</details>

<br/>

---

## 🛠️ &nbsp;기술 스택

<table>
  <tr>
    <th width="140">분류</th>
    <th>기술</th>
  </tr>
  <tr>
    <td align="center"><b>🎮 Client</b></td>
    <td>
      <img src="https://img.shields.io/badge/Unity-WebGL-black?logo=unity" />
      &nbsp;
      <img src="https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white" />
      &nbsp; Object Pooling · Raycast Ground Detection · Dynamic Sound Pitch · Custom REST API (<code>APIManager.cs</code>)
    </td>
  </tr>
  <tr>
    <td align="center"><b>⚙️ Backend</b></td>
    <td>
      <img src="https://img.shields.io/badge/Spring_Boot-3.5.11-6DB33F?logo=springboot&logoColor=white" />
      &nbsp;
      <img src="https://img.shields.io/badge/Java_17-ED8B00?logo=openjdk&logoColor=white" />
      &nbsp;
      <img src="https://img.shields.io/badge/Gradle-02303A?logo=gradle&logoColor=white" />
      &nbsp; Spring Security · Session Auth · Swagger/OpenAPI 3
    </td>
  </tr>
  <tr>
    <td align="center"><b>🗄️ Database</b></td>
    <td>
      <img src="https://img.shields.io/badge/PostgreSQL-17-336791?logo=postgresql&logoColor=white" />
      &nbsp;
      <img src="https://img.shields.io/badge/Redis-7-DC382D?logo=redis&logoColor=white" />
      &nbsp; JPA · 세션 캐싱 · 실시간 리더보드
    </td>
  </tr>
  <tr>
    <td align="center"><b>☁️ Infra</b></td>
    <td>
      <img src="https://img.shields.io/badge/AWS_EC2-FF9900?logo=amazonaws&logoColor=white" />
      &nbsp;
      <img src="https://img.shields.io/badge/Docker_Compose-2496ED?logo=docker&logoColor=white" />
      &nbsp;
      <img src="https://img.shields.io/badge/Nginx-009639?logo=nginx&logoColor=white" />
      &nbsp; GitLab CI/CD · Let's Encrypt SSL
    </td>
  </tr>
  <tr>
    <td align="center"><b>🤝 협업</b></td>
    <td>
      <img src="https://img.shields.io/badge/Jira-0052CC?logo=jira&logoColor=white" />
      &nbsp;
      <img src="https://img.shields.io/badge/Notion-000000?logo=notion&logoColor=white" />
      &nbsp;
      <img src="https://img.shields.io/badge/GitLab-FC6D26?logo=gitlab&logoColor=white" />
      &nbsp; 주간 스프린트 · 백로그 관리 · 회의록 아카이빙
    </td>
  </tr>
</table>

<br/>

---

## 📂 &nbsp;디렉토리 구조

```text
zabonzooET/
├── Assets/                       # 🎮 Unity 게임 클라이언트 에셋 & C# 스크립트
│   ├── Scripts/
│   │   ├── APIManager.cs         #   커스텀 HTTP 통신 모듈
│   │   ├── player.cs             #   플레이어 물리 & 점프 시스템
│   │   ├── GameManager.cs        #   인게임 전체 흐름 관리
│   │   ├── QuizManager.cs        #   퀴즈 UI 모달 & 판정 로직
│   │   ├── FinanceSelect/        #   금융 상품 선택 씬 스크립트
│   │   └── StageSelect/          #   스테이지 선택 씬 스크립트
│   ├── Scenes/                   #   Lobby · StageSelect · 1980s/2000s/2020s · FinanceSelect
│   ├── Sprites/                  #   시대별 캐릭터 & UI 스프라이트
│   └── WebGLTemplates/           #   브라우저 배포용 커스텀 WebGL 템플릿
│
├── src/main/java/                # ⚙️ Spring Boot 백엔드 소스
│   └── com/ssafy/amagetdon/
│       ├── common/               #   글로벌 예외처리 · 세션 인터셉터 · 웹 설정
│       └── domain/               #   User · Game · Quiz · Coin 도메인 비즈니스 로직
│
├── infra/                        # ☁️ 배포 인프라 설정
│   ├── docker-compose.prod.yml   #   프로덕션 컨테이너 구성
│   ├── nginx.conf                #   리버스 프록시 & 라우팅
│   └── scripts/                  #   EC2 초기화 · 배포 자동화 스크립트
│
├── portfolio/                    # 🌐 React 포트폴리오 소개 페이지
└── build.gradle                  # Gradle 빌드 스크립트
```

<br/>

---

## 🚀 &nbsp;주요 기술적 도전 및 해결

<br/>

### ⚡ 1. KDB 공공데이터 기반 동적 퀴즈 자동 생성 — `QuizDataLoader.java`

> 🔴 **문제점** <br/>
> 금융 학습 효과를 위해 수백 개의 퀴즈가 필요했으나, DB 수동 등록은 비효율적이었습니다.
> 
> 🟢 **해결책** <br/>
> KDB 금융 용어 공공데이터 CSV를 파싱하는 **자동 배치 로더**를 구현했습니다.
> 
> 🔵 **구현 방식** <br/>
> 서버 기동 시 용어 설명문에서 정답을 추출하고, 전체 용어 풀에서 무작위 오답 3개를 Shuffle하여 **4지선다 객관식 퀴즈를 동적으로 자동 생성**하도록 구축했습니다.

<br/>

### 🎯 2. 게임 물리 & 연출 디테일링 — `player.cs` · `GameManager.cs`

> 🔴 **1단/2단 점프 구현** <br/>
> `Rigidbody2D` 물리 연산과 `Raycast` 지면 감지를 혼합하여 매끄럽고 정교한 점프 피드백을 구현했습니다.
> 
> 🟢 **가변 사운드 피치** <br/>
> 주행 속도 변화에 따라 달리기 오디오의 **Pitch를 실시간으로 비례 연동**시켜 청각적 속도감을 극대화했습니다.
> 
> 🔵 **카메라 셰이크 연출** <br/>
> 피격 시 화면 흔들림 효과와 무적 프레임 기믹을 추가하여 타격감을 살리고 게임 밸런싱을 동시에 해결했습니다.

<br/>

### 🔒 3. 보안성 중심의 인프라 격리 설계

> 🔐 **환경 변수화** <br/>
> `application.yml` 내 PostgreSQL, Redis 계정 정보 등 민감한 데이터를 모두 `${ENV_VAR}` 방식으로 외부화했습니다.
> 
> 🛡️ **.gitignore 최적화** <br/>
> Unity 에디터 캐시(`Library/`, `Temp/`) 및 인프라 시크릿(`.env`)을 완벽하게 격리하여, Public 저장소에서의 보안 사고를 원천 차단했습니다.

<br/>

---

## 👥 &nbsp;팀 구성 및 역할 분담

<table>
  <tr>
    <td align="center" valign="top" width="50%">
      <br/><br/>
      <a href="https://github.com/kkaemong"><img src="./profiles/jinjuneyoung.png" width="130" style="border-radius:50%" alt="프로필 이미지 삽입"/></a><br/><br/>
      <h3>진준영</h3>
      <span style="font-size: 1.1em;"><b>Unity 클라이언트</b></span><br/><br/>
      <div align="left" style="font-size: 1.05em;">
        <ul>
          <li>Unity를 활용하여 핵심 인게임 씬(Scene)을 구축하고, 중력 적용 및 캐릭터 애니메이션을 구현하여 역동적인 환경 조성</li>
          <li>Rigidbody2D 기반의 물리 점프 시스템 및 게임 내 재화(코인) 동적 생성 로직을 설계하여 핵심 게임 루프 완성</li>
          <li>커스텀 HTTP 통신 모듈을 단독 설계하여 Spring Boot 백엔드와의 금융 퀴즈 데이터를 연동하고 퀴즈 모달 UI 개발</li>
            <li>가변 사운드 피치, 카메라 셰이크 등 디테일한 연출 요소들을 추가하여 사용자의 게임 몰입도 및 타격감 향상</li>
        </ul>
      </div>
    </td>
    <td align="center" valign="top" width="50%">
      <br/><br/>
      <a href="#"><img src="./profiles/gyeongminji.png" width="130" style="border-radius:50%" alt="프로필 이미지 삽입"/></a><br/><br/>
      <h3>경민지</h3>
      <span style="font-size: 1.1em;"><b>Unity 클라이언트</b></span><br/><br/>
      <div align="left" style="font-size: 1.05em;">
        <ul>
          <li>스테이지 선택 & 금융 상품 씬 구현</li>
          <li>E.T. & 배경 2D 에셋 직접 드로잉</li>
          <li>스프라이트 패킹 & UI 최적화</li>
        </ul>
      </div>
    </td>
  </tr>
  <tr>
    <td align="center" valign="top" width="50%">
      <br/><br/>
      <a href="#"><img src="./profiles/hongjeonghee.png" width="130" style="border-radius:50%" alt="프로필 이미지 삽입"/></a><br/><br/>
      <h3>홍정희</h3>
      <span style="font-size: 1.1em;"><b>Spring Boot 백엔드</b></span><br/><br/>
      <div align="left" style="font-size: 1.05em;">
        <ul>
          <li>전 도메인 REST API 단독 구현</li>
          <li>KDB 기반 4지선다 퀴즈 자동 생성</li>
          <li>Redis 캐싱 & 실시간 리더보드</li>
        </ul>
      </div>
    </td>
    <td align="center" valign="top" width="50%">
      <br/><br/>
      <a href="#"><img src="./profiles/kimdaeyeon.png" width="130" style="border-radius:50%" alt="프로필 이미지 삽입"/></a><br/><br/>
      <h3>김대연</h3>
      <span style="font-size: 1.1em;"><b>인프라 & Unity 클라이언트</b></span><br/><br/>
      <div align="left" style="font-size: 1.05em;">
        <ul>
          <li>로비 · 상점 · UFO 정비소 UI 구현</li>
          <li>AWS EC2 + Docker Compose 배포</li>
          <li>Nginx 리버스 프록시 & WebGL 최적화</li>
        </ul>
      </div>
    </td>
  </tr>
</table>

<br/>

---

<div align="center">

**자본주E.T.** · SSAFY 공통 프로젝트 2기 핀테크 트랙

</div>
