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

- 방식: `MR merge` 커밋이 `infra/develop` 또는 `master`에 들어오면 GitLab CI가 Jira API로 이슈 상태 전환
- 커밋 컨벤션은 그대로 유지, 이슈 키(`S14P21A507-24`)가 MR 제목/브랜치명/커밋 메시지 중 하나에 있어야 함
- 필수 GitLab CI 변수:
  - `JIRA_BASE_URL` (예: `https://ssafy.atlassian.net`)
  - `JIRA_USER_EMAIL` (Jira 계정 이메일)
  - `JIRA_API_TOKEN` (Atlassian API token, masked/protected 권장)
  - `JIRA_DONE_TRANSITION_ID` (완료 상태 transition id)
- 선택 변수:
  - `JIRA_SYNC_ENABLED` (`true` 기본값, 필요 시 `false`로 비활성화)
