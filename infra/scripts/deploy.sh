#!/bin/bash
set -euo pipefail

IMAGE="${1:-}"
if [ -z "$IMAGE" ]; then
  echo "Usage: ./deploy.sh <docker-image>"
  exit 1
fi
echo "Deploying $IMAGE..."

# Navigate to the infra directory relative to this script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )/.." && pwd )"
cd "$DIR"

# Pull the requested image
docker pull $IMAGE

# Update the running containers using docker-compose.prod.yml
IMAGE_TAG=$IMAGE docker compose -f docker-compose.prod.yml up -d --build

# Wait for backend container to be running, then fail fast with logs if not.
echo "Waiting for backend container to become stable..."
RUNNING=false
for i in {1..12}; do
  if docker compose -f docker-compose.prod.yml ps --services --status running | grep -qx "backend"; then
    RUNNING=true
    break
  fi
  echo "Still waiting... ($i/12)"
  sleep 5
done

if [ "$RUNNING" != true ]; then
  echo "==================================="
  echo "ERROR: backend container is not running after deploy."
  echo "==================================="
  docker compose -f docker-compose.prod.yml ps
  docker compose -f docker-compose.prod.yml logs backend --tail 200
  exit 1
fi

# Clean up unused dangling images to preserve disk space
docker image prune -f
echo "==================================="
echo "Deployment successful: $IMAGE"
echo "==================================="
