#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
BE_DIR="$ROOT_DIR/BE"

cd "$BE_DIR"
docker compose down
