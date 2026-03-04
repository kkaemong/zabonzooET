# MonET Infra 협업 README

## 커밋/MR 규칙

- 커밋 형식: 기존 협업 규칙 유지
- MR 제목: Jira Key 필수 (`[MONET-123] 변경 요약`)

### Jira Key가 뭔가?

- Jira 이슈의 고유 키입니다.
- 예: `MONET-123`, `MONET-45`

### 커밋 메시지 형식

- 권장 형식: `{타입} : {작업 내용}({JiraKey})`
- 예시:
```text
infra : nginx reverse proxy 설정 정리(MONET-123)
chore : deploy 스크립트 변수 보강(MONET-128)
```

### MR 제목 형식

- 권장 형식: `[JiraKey] 변경 요약`
- 예시:
```text
[MONET-123] nginx 설정 및 health check 정리
[MONET-128] deploy.sh DEPLOY_REPO_DIR 반영
```

### 왜 이렇게 해야 하나?

- Jira 이슈와 GitLab 변경 이력을 1:1로 추적하기 쉽습니다.
- Jira 자동화 규칙(MR merge 시 완료 전환)과 연결됩니다.

## 필요한 명령어 (최초 1회)

```bash
./infra/scripts/install-jira-git-hooks.sh
```

- 목적: 커밋 시 Jira Key 누락을 로컬에서 바로 차단해서 실수를 줄입니다.
