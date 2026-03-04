#!/usr/bin/env bash
set -euo pipefail

GITLAB_API_BASE="${GITLAB_API_BASE:-https://lab.ssafy.com/api/v4}"
GITLAB_TOKEN="${GITLAB_TOKEN:-}"
GITLAB_PROJECT_ID="${GITLAB_PROJECT_ID:-}"

if [ -z "$GITLAB_TOKEN" ] || [ -z "$GITLAB_PROJECT_ID" ]; then
  cat <<'USAGE'
Usage:
  export GITLAB_TOKEN=<gitlab_pat_with_api_scope>
  export GITLAB_PROJECT_ID=<numeric_id_or_urlencoded_path>
  export EC2_IP=<ec2_public_ip_or_dns>
  export EC2_USER=<ssh_user>
  export SSH_PRIVATE_KEY="$(cat ~/.ssh/your_key)"
  export DEPLOY_BACKEND_IMAGE=<registry/project/backend:tag>
  export DEPLOY_FRONTEND_IMAGE=<registry/project/frontend:tag>
  ./infra/scripts/day0-set-gitlab-vars.sh
USAGE
  exit 1
fi

api_url() {
  local key="$1"
  printf '%s/projects/%s/variables/%s' "$GITLAB_API_BASE" "$GITLAB_PROJECT_ID" "$key"
}

upsert_var() {
  local key="$1"
  local value="$2"
  local masked="${3:-false}"
  local protected="${4:-false}"

  if [ -z "$value" ]; then
    echo "[gitlab-vars] skip empty: $key"
    return 0
  fi

  local code
  code="$(curl -s -o /tmp/gitlab_var_get.out -w '%{http_code}' \
    --header "PRIVATE-TOKEN: $GITLAB_TOKEN" \
    "$(api_url "$key")")"

  if [ "$code" = "200" ]; then
    curl -sS --request PUT \
      --header "PRIVATE-TOKEN: $GITLAB_TOKEN" \
      --form "value=$value" \
      --form "masked=$masked" \
      --form "protected=$protected" \
      "$(api_url "$key")" >/tmp/gitlab_var_put.out
    echo "[gitlab-vars] updated: $key"
  else
    curl -sS --request POST \
      --header "PRIVATE-TOKEN: $GITLAB_TOKEN" \
      --form "key=$key" \
      --form "value=$value" \
      --form "masked=$masked" \
      --form "protected=$protected" \
      "$GITLAB_API_BASE/projects/$GITLAB_PROJECT_ID/variables" >/tmp/gitlab_var_post.out
    echo "[gitlab-vars] created: $key"
  fi
}

upsert_var "EC2_IP" "${EC2_IP:-}" "false" "true"
upsert_var "EC2_USER" "${EC2_USER:-}" "false" "true"
upsert_var "SSH_PRIVATE_KEY" "${SSH_PRIVATE_KEY:-}" "true" "true"
upsert_var "DEPLOY_BACKEND_IMAGE" "${DEPLOY_BACKEND_IMAGE:-}" "false" "true"
upsert_var "DEPLOY_FRONTEND_IMAGE" "${DEPLOY_FRONTEND_IMAGE:-}" "false" "true"

echo "[gitlab-vars] completed"
