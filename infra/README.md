# MonET Infra 협업 README

## 커밋/MR 규칙

- 커밋 형식: 기존 협업 규칙 유지
- MR 제목: Jira Key 필수 (`[S14P21A507-24] 변경 요약`)

### Jira Key가 뭔가?

- Jira 이슈의 고유 키입니다.
- 예: `S14P21A507-24`, `S14P21A507-108`

### 커밋 메시지 형식

- 권장 형식: `{타입} : {작업 내용}({JiraKey})`
- 예시:
```text
infra : nginx reverse proxy 설정 정리(S14P21A507-24)
chore : deploy 스크립트 변수 보강(S14P21A507-24)
```

### MR 제목 형식

- 권장 형식: `[JiraKey] 변경 요약`
- 예시:
```text
[S14P21A507-24] nginx 설정 및 health check 정리
[S14P21A507-108] deploy.sh DEPLOY_REPO_DIR 반영
```

### 왜 이렇게 해야 하나?

- Jira 이슈와 GitLab 변경 이력을 1:1로 추적하기 쉽습니다.
- Jira 자동화 규칙(MR merge 시 완료 전환)과 연결됩니다.

## 필요한 명령어 (최초 1회)

```bash
./infra/scripts/install-jira-git-hooks.sh
```

- 목적: 커밋 시 Jira Key 누락을 로컬에서 바로 차단해서 실수를 줄입니다.

## Jira 자동 완료(CI)

- 방식: `MR merge` 커밋이 `infra/develop`에 들어오면 GitLab CI가 Jira API로 이슈 상태 전환
- 커밋 컨벤션은 그대로 유지, 이슈 키(`S14P21A507-24`)가 MR 제목/브랜치명/커밋 메시지 중 하나에 있어야 함
- 필수 GitLab CI 변수:
  - `JIRA_BASE_URL` (예: `https://ssafy.atlassian.net`)
  - `JIRA_USER_EMAIL` (Jira 계정 이메일)
  - `JIRA_API_TOKEN` (Atlassian API token, masked/protected 권장)
  - `JIRA_DONE_TRANSITION_ID` (완료 상태 transition id)
- 선택 변수:
  - `JIRA_SYNC_ENABLED` (`true` 기본값, 필요 시 `false`로 비활성화)

## 자동배포 최소 계약

- 현재 배포 기준: `infra/develop` push 시 자동배포
- `master`는 발표자료/README 중심 브랜치로 남겨도 되며, 자동배포는 `infra/develop`만 기준으로 동작
- 배포 예시 도메인: `j14a507.p.ssafy.io`
- 필수 GitLab CI 변수:
  - `EC2_IP`
  - `EC2_USER`
  - `DEPLOY_REPO_DIR`
  - `SSH_PRIVATE_KEY`
  - `DEPLOY_BACKEND_IMAGE`
  - `DEPLOY_FRONTEND_IMAGE`
  - `DEPLOY_ENABLED=true`

### Day0 변수 일괄 등록

```bash
export GITLAB_TOKEN=<gitlab_pat_with_api_scope>
export GITLAB_PROJECT_ID=<numeric_id_or_urlencoded_path>
export EC2_IP=j14a507.p.ssafy.io
export EC2_USER=ubuntu
export DEPLOY_REPO_DIR=/home/ubuntu/S14P21A507
export SSH_PRIVATE_KEY="$(cat ~/.ssh/your_key)"
export DEPLOY_BACKEND_IMAGE=<registry/project/backend:tag>
export DEPLOY_FRONTEND_IMAGE=<registry/project/frontend:tag>
export DEPLOY_ENABLED=true
./infra/scripts/day0-set-gitlab-vars.sh
```

- 목적: 자동배포에 필요한 최소 계약을 GitLab Variables에 한 번에 반영
- 주의: `SSH_PRIVATE_KEY` 는 멀티라인 PEM 그대로 입력해야 하며, masked 대신 protected 기준으로 관리
- 주의: 배포 시 CI가 현재 커밋의 `infra/` 디렉터리를 EC2에 동기화하며, `infra/.env.prod` 와 `infra/ssl.conf` 는 서버 로컬 파일로 유지
- 권장 운영: 작업은 기능 브랜치에서 하고, 배포가 필요한 변경만 `infra/develop`에 반영
