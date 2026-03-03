#!/usr/bin/env bash
set -euo pipefail

# ──────────────────────────────────────────────
#  setup-certbot.sh
#  EC2에서 Let's Encrypt SSL 인증서를 발급하고
#  nginx에 연결합니다.
# ──────────────────────────────────────────────

usage() {
  cat <<'EOF'
Usage: ./infra/scripts/setup-certbot.sh <domain> [email]

Arguments:
  domain    SSL 인증서를 발급할 도메인 (예: amagetdon.example.com)
  email     Let's Encrypt 알림용 이메일 (선택, 미입력 시 --register-unsafely-without-email)

Examples:
  ./infra/scripts/setup-certbot.sh amagetdon.ssafy.io
  ./infra/scripts/setup-certbot.sh amagetdon.ssafy.io admin@example.com
EOF
}

DOMAIN="${1:-}"
EMAIL="${2:-}"

if [ -z "$DOMAIN" ]; then
  usage
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
INFRA_DIR="$ROOT_DIR/infra"
SSL_TEMPLATE="$INFRA_DIR/ssl.conf.template"
SSL_CONF="$INFRA_DIR/ssl.conf"
COMPOSE_FILE="$INFRA_DIR/docker-compose.prod.yml"
ENV_FILE="$INFRA_DIR/.env.prod"

# ── 1. certbot 설치 확인 ──
if ! command -v certbot >/dev/null 2>&1; then
  echo "[certbot] certbot이 설치되어 있지 않습니다. 설치 중..."
  sudo apt-get update
  sudo apt-get install -y certbot
fi

# ── 2. nginx를 HTTP만으로 기동 (인증서 발급 전) ──
echo "[certbot] HTTPS 없이 nginx를 HTTP 모드로 기동합니다..."

# ssl.conf가 없으면 빈 파일 생성 (nginx include 에러 방지)
if [ ! -f "$SSL_CONF" ]; then
  echo "# SSL not yet configured" > "$SSL_CONF"
fi

# nginx 컨테이너 기동 (HTTP만)
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d nginx

# nginx가 기동될 때까지 대기
echo "[certbot] nginx 기동 대기 중..."
for i in $(seq 1 10); do
  if curl -fsS http://127.0.0.1/health >/dev/null 2>&1; then
    echo "[certbot] nginx HTTP 정상 기동 확인"
    break
  fi
  echo "[certbot] 대기 중... ($i/10)"
  sleep 2
done

# ── 3. certbot webroot 디렉토리 준비 ──
# Docker 볼륨에서 webroot 경로 확인
WEBROOT_VOLUME="$(docker volume inspect amagetdon_certbot_webroot --format '{{.Mountpoint}}' 2>/dev/null || echo '')"
if [ -z "$WEBROOT_VOLUME" ]; then
  # 볼륨 이름이 다를 수 있으므로 infra_ 프리픽스도 확인
  WEBROOT_VOLUME="$(docker volume inspect infra_certbot_webroot --format '{{.Mountpoint}}' 2>/dev/null || echo '')"
fi

# 볼륨을 못 찾으면 호스트 디렉토리로 대체
WEBROOT="${WEBROOT_VOLUME:-/var/www/certbot}"
sudo mkdir -p "$WEBROOT"

# ── 4. 인증서 발급 ──
echo "[certbot] 도메인: $DOMAIN"
echo "[certbot] Webroot: $WEBROOT"

CERTBOT_ARGS=(
  certonly
  --webroot
  -w "$WEBROOT"
  -d "$DOMAIN"
  --non-interactive
  --agree-tos
)

if [ -n "$EMAIL" ]; then
  CERTBOT_ARGS+=(--email "$EMAIL")
else
  CERTBOT_ARGS+=(--register-unsafely-without-email)
fi

echo "[certbot] 인증서 발급 시작..."
sudo certbot "${CERTBOT_ARGS[@]}"

# ── 5. ssl.conf 생성 (템플릿에서 도메인 치환) ──
echo "[certbot] SSL nginx 설정 생성 중..."
sed "s/DOMAIN_PLACEHOLDER/$DOMAIN/g" "$SSL_TEMPLATE" > "$SSL_CONF"

# ── 6. 인증서를 Docker 볼륨으로 복사 ──
CERT_VOLUME="$(docker volume inspect amagetdon_letsencrypt_certs --format '{{.Mountpoint}}' 2>/dev/null || echo '')"
if [ -z "$CERT_VOLUME" ]; then
  CERT_VOLUME="$(docker volume inspect infra_letsencrypt_certs --format '{{.Mountpoint}}' 2>/dev/null || echo '')"
fi

if [ -n "$CERT_VOLUME" ]; then
  echo "[certbot] 인증서를 Docker 볼륨으로 복사 중..."
  sudo cp -rL /etc/letsencrypt/* "$CERT_VOLUME/"
else
  echo "[certbot] WARNING: letsencrypt_certs 볼륨을 찾을 수 없습니다."
  echo "[certbot] docker-compose.prod.yml에서 볼륨 대신 바인드 마운트를 사용하세요."
fi

# ── 7. nginx 재기동 (HTTPS 활성화) ──
echo "[certbot] nginx를 HTTPS 모드로 재기동합니다..."
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d nginx --force-recreate

# ── 8. HTTPS 동작 확인 ──
echo "[certbot] HTTPS 동작 확인 중..."
HTTPS_OK=false
for i in $(seq 1 10); do
  if curl -fsSk https://127.0.0.1/health >/dev/null 2>&1; then
    HTTPS_OK=true
    break
  fi
  echo "[certbot] HTTPS 대기 중... ($i/10)"
  sleep 3
done

if [ "$HTTPS_OK" = true ]; then
  echo "==================================="
  echo "SSL 인증서 발급 및 HTTPS 설정 완료!"
  echo "도메인: https://$DOMAIN"
  echo "==================================="
else
  echo "==================================="
  echo "WARNING: HTTPS 헬스체크 실패"
  echo "nginx 로그를 확인하세요:"
  echo "  docker compose -f $COMPOSE_FILE logs nginx --tail 50"
  echo "==================================="
fi

# ── 9. 자동 갱신 cron 설정 ──
CRON_CMD="0 3 * * * certbot renew --quiet && cp -rL /etc/letsencrypt/* $CERT_VOLUME/ 2>/dev/null && docker exec amagetdon-nginx nginx -s reload"
if ! crontab -l 2>/dev/null | grep -q "certbot renew"; then
  echo "[certbot] 자동 갱신 cron 등록 중..."
  (crontab -l 2>/dev/null; echo "$CRON_CMD") | crontab -
  echo "[certbot] cron 등록 완료 (매일 03:00 자동 갱신)"
else
  echo "[certbot] 자동 갱신 cron이 이미 등록되어 있습니다."
fi
