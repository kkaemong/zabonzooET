# MonET Ops Runbook

## 1. 배포 전 확인

1. `master` 기준 CI 성공 확인
2. GitLab Variables (`EC2_IP`, `EC2_USER`, `SSH_PRIVATE_KEY`) 확인
3. EC2 디스크 여유/도커 상태 확인

## 2. 배포

```bash
./infra/scripts/deploy.sh <registry>/<project>/backend:<tag>
```

## 3. 점검

```bash
curl -f http://127.0.0.1/health
docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml ps
```

## 4. 장애 대응

1. nginx 로그 확인
2. backend 로그 확인
3. postgres 상태 확인
4. 이전 이미지 태그로 롤백 재배포
