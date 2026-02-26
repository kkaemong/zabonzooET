#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
BE_DIR="$ROOT_DIR/BE"

cd "$BE_DIR"
if ! docker compose ps >/dev/null 2>&1; then
  echo "[infra] docker compose 상태를 조회할 수 없습니다."
  echo "[infra] Docker Desktop(WSL 통합) 또는 Docker daemon 상태를 확인하세요."
  exit 1
fi

echo "[infra] Docker Compose service status:"
docker compose ps

running_services="$(docker compose ps --services --status running)"
if ! printf '%s\n' "$running_services" | grep -qx "postgres"; then
  echo ""
  echo "[infra] postgres 서비스가 실행 중이 아닙니다."
  echo "[infra] 먼저 실행: infra/scripts/local-up.sh"
  exit 1
fi

if ! printf '%s\n' "$running_services" | grep -qx "redis"; then
  echo ""
  echo "[infra] redis 서비스가 실행 중이 아닙니다."
  echo "[infra] 먼저 실행: infra/scripts/local-up.sh"
  exit 1
fi

echo ""
echo "[infra] Postgres readiness check..."
if ! docker compose exec -T postgres sh -lc 'pg_isready -U "$POSTGRES_USER" -d "$POSTGRES_DB"'; then
  echo "[infra] Postgres readiness check 실패."
  echo "[infra] 로그 확인: cd BE && docker compose logs postgres --tail 200"
  exit 1
fi

echo "[infra] Redis ping check..."
redis_ping="$(docker compose exec -T redis redis-cli ping 2>/dev/null || true)"
if [ "$redis_ping" != "PONG" ]; then
  echo "[infra] Redis ping check 실패. 응답: ${redis_ping:-<empty>}"
  echo "[infra] 로그 확인: cd BE && docker compose logs redis --tail 200"
  exit 1
fi

echo "[infra] local-check completed."
