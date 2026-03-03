# Infra Docs

아마겟돈 인프라 문서의 단일 진입점.

## 팀 공통 기준

1. 저장소 루트: `S14P21A507`
2. 백엔드 작업 경로: `BE/`
3. 로컬 실행 기준 파일:
- `BE/docker-compose.yml`
- `BE/.env` (개인 파일, 커밋 금지)
- `infra/docs/templates/be.env.example` (공유 템플릿)
4. 운영 실행 기준 파일:
- `infra/docker-compose.prod.yml`
- `infra/.env.prod` (운영 배포 시 동적 생성, 커밋 금지)
- `infra/docs/templates/prod.env.example`
5. 가장 빠른 로컬 명령:
- `infra/scripts/local-up.sh`
- `infra/scripts/local-check.sh`
- `infra/scripts/local-down.sh`
6. 운영 배포 관련 스크립트:
- `infra/scripts/build-push-image.sh`
- `infra/scripts/render-prod-env.sh`
- `infra/scripts/deploy.sh`
- `infra/scripts/init-ec2.sh`
- `infra/scripts/day0-set-gitlab-vars.sh`
- `infra/scripts/install-jira-git-hooks.sh`

## 문서 목록

1. `01_local_quickstart.md`: 로컬 실행/중지/확인
2. `02_env_contract.md`: 환경변수 계약
3. `03_ci_cd_skeleton.md`: CI/CD 골격과 운영 원칙
4. `04_ops_runbook.md`: 운영/장애 대응 절차
5. `05_secrets_cloudwatch_guide.md`: 운영 시크릿/로그 연동
6. `06_ec2_launch_day_checklist.md`: EC2 발급 직후 배포 체크리스트
7. `07_day0_command_set.md`: Day-0 실행 명령 세트
8. `templates/be.env.example`: 로컬 `.env` 템플릿
9. `templates/prod.env.example`: 운영 `.env.prod` 템플릿
