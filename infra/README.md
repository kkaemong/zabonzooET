# MonET (자본주E.T) Infra

이 저장소는 infra-only 운영 저장소입니다.

## 핵심 원칙

1. FE/BE 소스 코드는 이 저장소에서 관리하지 않습니다.
2. FE/BE는 별도 파이프라인에서 이미지 빌드 후 레지스트리에 push합니다.
3. 본 저장소는 EC2에서 compose 배포만 담당합니다.
4. Backend 연동 기준은 `origin/back/develop` 브랜치의 환경변수 규격을 따른다.

## 배포 구성

- Reverse Proxy: Nginx
- Frontend: 컨테이너 이미지 (`FRONTEND_IMAGE`)
- Backend: 컨테이너 이미지 (`BACKEND_IMAGE`)
- Database: PostgreSQL (EC2 내부 컨테이너)
- Optional: Redis 7 (`--profile redis`)

## 배포 방법

```bash
cp infra/docs/templates/prod.env.example infra/.env.prod
./infra/scripts/deploy.sh <backend-image> <frontend-image>
```

## 필수 파일

- `infra/docker-compose.prod.yml`
- `infra/nginx.conf`
- `infra/scripts/deploy.sh`
- `infra/docs/templates/prod.env.example`
