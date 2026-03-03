#!/usr/bin/env bash
set -euo pipefail

IMAGE="${1:-}"
if [ -z "$IMAGE" ]; then
  echo "Usage: ./infra/scripts/build-push-image.sh <docker-image>"
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
DOCKERFILE="$ROOT_DIR/infra/Dockerfile"
DOCKER_PLATFORM="${DOCKER_PLATFORM:-linux/amd64}"

echo "[build-push] image: $IMAGE"
echo "[build-push] dockerfile: $DOCKERFILE"
echo "[build-push] platform: $DOCKER_PLATFORM"

docker build --platform "$DOCKER_PLATFORM" -f "$DOCKERFILE" -t "$IMAGE" "$ROOT_DIR"
docker push "$IMAGE"

echo "[build-push] completed: $IMAGE"
