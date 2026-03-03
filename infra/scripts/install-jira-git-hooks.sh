#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
HOOK_DIR="$ROOT_DIR/.git/hooks"
SOURCE_DIR="$ROOT_DIR/infra/hooks"

if [ ! -d "$ROOT_DIR/.git" ]; then
  echo "[install-hooks] .git directory was not found. Run from a cloned git repository."
  exit 1
fi

install_hook() {
  local name="$1"
  local source="$SOURCE_DIR/$name"
  local target="$HOOK_DIR/$name"

  if [ ! -f "$source" ]; then
    echo "[install-hooks] missing hook source: $source"
    exit 1
  fi

  cp "$source" "$target"
  chmod +x "$target"
  echo "[install-hooks] installed: $target"
}

install_hook "prepare-commit-msg"
install_hook "commit-msg"

cat <<'EOF'
[install-hooks] done
- 브랜치 예시: be-feat/login-api_S14P21A507-1
- 커밋 예시: feat : 로그인 API 추가(S14P21A507-1)
EOF
