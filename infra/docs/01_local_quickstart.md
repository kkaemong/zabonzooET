# Local Quickstart (BE 기준)

## 0. 가장 빠른 방법

저장소 루트(`S14P21A507`)에서 실행:

```bash
chmod +x infra/scripts/*.sh
infra/scripts/local-up.sh
infra/scripts/local-check.sh
```

## 1. 최초 1회

1. `infra/docs/templates/be.env.example`를 복사해서 `BE/.env` 생성
2. `POSTGRES_PASSWORD`만 개인 로컬 값으로 변경

```bash
cd BE
cp ../infra/docs/templates/be.env.example .env
```

## 2. 로컬 인프라 실행

```bash
cd BE
docker compose up -d
docker compose ps
```

정상 기준:
- `local-postgres`: `healthy`
- `local-redis`: `Up`

## 3. 연결 확인

```bash
docker exec -it local-postgres psql -U A507 -d amagetdon -c "select now();"
docker exec -it local-redis redis-cli ping
```

정상 기준:
- Postgres: `now` 1 row
- Redis: `PONG`

## 4. 중지/정리

```bash
cd BE
docker compose down
```

데이터 볼륨까지 초기화:

```bash
cd BE
docker compose down -v
```

빠른 중지:

```bash
infra/scripts/local-down.sh
```
