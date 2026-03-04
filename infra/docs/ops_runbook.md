# MonET Infra Ops Runbook

## 1) 배포 전 점검

1. EC2 docker / docker compose 동작 확인
2. `infra/.env.prod` 존재 및 DB 비밀번호 설정 확인
3. 사용할 이미지 태그 확인

## 2) 배포

```bash
./infra/scripts/deploy.sh <backend-image> <frontend-image>
```

## 3) 상태 확인

```bash
curl -f http://127.0.0.1/health
docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml ps
```

## 4) 로그 확인

```bash
docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml logs nginx --tail 100
docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml logs backend --tail 100
docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml logs postgres --tail 100
```

## 5) 롤백

```bash
./infra/scripts/deploy.sh <previous-backend-image> <previous-frontend-image>
```
