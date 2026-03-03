# 아마겟돈 인프라 (Infra)

아마겟돈 프로젝트 인프라 구성 및 팀원용 가이드 문서입니다.

---

## 🚀 팀원 온보딩 — 처음 1회만 하면 됩니다

레포 클론 직후 아래 명령어를 실행하세요.

```bash
cd S14P21A507
chmod +x infra/scripts/*.sh
./infra/scripts/install-jira-git-hooks.sh
```

이 명령은 Git commit hook 두 개를 설치합니다:

| 훅 | 역할 |
|----|------|
| `prepare-commit-msg` | 브랜치 이름에서 Jira 이슈 키를 자동 추출하여 커밋 메시지 앞에 삽입 |
| `commit-msg` | 커밋 메시지에 Jira 이슈 키가 없으면 커밋을 거부 |

---

## 📌 일상 작업 플로우

### 1단계: Jira 이슈 생성

작업 시작 전 Jira에서 에픽/이슈를 생성합니다.

### 2단계: 브랜치 생성 (이슈 키 포함)

```bash
# 형식: {구분}-{작업타입}/{영어작업내용}_{지라이슈번호}
git checkout -b be-feat/login-api_S14P21A507-17
git checkout -b fe-refactor/user-service_S14P21A507-31
git checkout -b be-infra/ci-workflow_S14P21A507-42
```

| 구분 | 작업타입 |
|------|----------|
| `be`, `fe`, `infra` | `feat`, `refactor`, `fix`, `docs`, `test`, `chore`, `infra` |

### 3단계: 커밋 (Jira 키 자동 삽입)

평소처럼 커밋하면 **훅이 자동으로** 브랜치에서 Jira 키를 읽어 메시지 끝에 괄호로 붙여줍니다.

```bash
# 이렇게 커밋하면:
git commit -m "feat : 로그인 API 추가"

# 자동으로 이렇게 됩니다:
# → feat : 로그인 API 추가(S14P21A507-17)
```

직접 명시해도 됩니다:

```bash
git commit -m "feat : 로그인 API 추가(S14P21A507-17)"
```

> ⛔ **Jira 이슈 키가 없는 커밋은 거부됩니다.** 이슈 키가 브랜치에도 없고 메시지에도 없으면 커밋이 실패합니다.

### 4단계: Push & MR/PR 생성

```bash
git push origin be-feat/login-api_S14P21A507-17
```

MR/PR 작성 시 아래 템플릿을 사용합니다:

```md
## 목적
-

## 변경 사항
-

## 테스트
-

## 관련 이슈
-
```

### 5단계: 리뷰 → 병합 → Jira 완료

1. 최소 **1명 승인** 후 병합
2. 병합 완료 후 Jira 이슈 상태를 **완료**로 변경

---

## 📋 한눈에 보는 플로우

| 단계 | 누가 | 뭘 하나 |
|------|------|---------|
| **0. 훅 설치** | 팀원 각자 | `./infra/scripts/install-jira-git-hooks.sh` (최초 1회) |
| **1. 이슈 생성** | 작업자 | Jira에서 에픽/이슈 생성 |
| **2. 브랜치 생성** | 작업자 | 이슈 키 포함해서 `git checkout -b` |
| **3. 커밋** | 작업자 | 평소처럼 커밋 (키 자동 삽입) |
| **4. Push & MR** | 작업자 | Push 후 MR 템플릿 작성 |
| **5. 리뷰 → 병합** | 리뷰어 + 작업자 | 1명 이상 승인 후 병합 |
| **6. 완료** | 작업자 | Jira 상태 `완료`로 변경 |

---

## 🗂️ 인프라 디렉토리 구조

```
infra/
├── docs/                        # 인프라 문서
│   ├── README.md                # 문서 목록 진입점
│   ├── 01_local_quickstart.md   # 로컬 실행/중지/확인
│   ├── 02_env_contract.md       # 환경변수 계약
│   ├── 03_ci_cd_skeleton.md     # CI/CD 골격
│   ├── 04_ops_runbook.md        # 운영/장애 대응
│   ├── 05_secrets_cloudwatch_guide.md
│   ├── 06_ec2_launch_day_checklist.md
│   ├── 07_day0_command_set.md   # Day-0 명령 세트
│   └── templates/               # 환경변수 템플릿
├── hooks/                       # Git hooks (Jira 연동)
│   ├── prepare-commit-msg
│   └── commit-msg
├── scripts/                     # 자동화 스크립트
│   ├── install-jira-git-hooks.sh
│   ├── local-up.sh / local-down.sh / local-check.sh
│   ├── build-push-image.sh
│   ├── render-prod-env.sh
│   ├── deploy.sh
│   ├── init-ec2.sh
│   └── day0-set-gitlab-vars.sh
├── Dockerfile
├── docker-compose.prod.yml
├── nginx.conf
└── ssl.conf.template
```

---

## 🔗 관련 문서

- [협업규칙 통합본](../Project_base/아마겟돈_협업규칙_통합본.md)
- [기술스택 확정안](../Project_base/아마겟돈_기술스택_확정안.md)
- [인프라 문서 목록](./docs/README.md)

---

최종 갱신: 2026-03-03
