# MonET Infra Ops Runbook

## 1) 배포 전 점검

1. EC2 docker / docker compose 동작 확인
2. `infra/.env.prod` 존재 및 DB 비밀번호 설정 확인
3. 사용할 이미지 태그 확인
4. 운영 도메인 예시: `j14a507.p.ssafy.io`

## 2) 배포

```bash
./infra/scripts/deploy.sh <backend-image> <frontend-image>
```

## 3) 상태 확인

```bash
curl -f http://127.0.0.1/health
curl -f http://127.0.0.1/api/health || curl -f http://127.0.0.1/api/actuator/health
curl -fk https://127.0.0.1/health
curl -f https://j14a507.p.ssafy.io/health
docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml ps
```

## 4) 로그 확인

```bash
docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml logs nginx --tail 100
docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml logs backend --tail 100
docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml logs postgres --tail 100
```

## 5) SSL 운영 확인

```bash
sudo openssl x509 -enddate -noout -in /etc/letsencrypt/live/j14a507.p.ssafy.io/fullchain.pem
sudo crontab -l
docker exec monet-nginx nginx -s reload
```

## 6) 롤백

이미지 태그 기준 재배포:

```bash
./infra/scripts/deploy.sh <previous-backend-image> <previous-frontend-image>
```

SSL 설정 문제 복구:

```bash
bash ./infra/scripts/setup-certbot.sh j14a507.p.ssafy.io
docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml up -d nginx --force-recreate
```
