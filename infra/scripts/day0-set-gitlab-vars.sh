#!/usr/bin/env bash
set -euo pipefail

GITLAB_API_BASE="${GITLAB_API_BASE:-https://gitlab.com/api/v4}"
GITLAB_TOKEN="${GITLAB_TOKEN:-}"
GITLAB_PROJECT_ID="${GITLAB_PROJECT_ID:-}"

if [ -z "$GITLAB_TOKEN" ] || [ -z "$GITLAB_PROJECT_ID" ]; then
  cat <<'EOF'
Usage:
  export GITLAB_TOKEN=<gitlab_pat_with_api_scope>
  export GITLAB_PROJECT_ID=<numeric_id_or_urlencoded_path>
  export EC2_IP=<ec2_public_ip_or_dns>
  export EC2_USER=<ssh_user>
  export SSH_PRIVATE_KEY="$(cat ~/.ssh/your_key)"
  export AWS_SECRET_ID=amagetdon/prod
  export AWS_REGION=ap-northeast-2
  ./infra/scripts/day0-set-gitlab-vars.sh
EOF
  exit 1
fi

api_url() {
  local path="$1"
  printf '%s/projects/%s/variables/%s' "$GITLAB_API_BASE" "$GITLAB_PROJECT_ID" "$path"
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
upsert_var "AWS_SECRET_ID" "${AWS_SECRET_ID:-}" "false" "true"
upsert_var "AWS_REGION" "${AWS_REGION:-ap-northeast-2}" "false" "true"

if [ -n "${CI_REGISTRY_USER:-}" ]; then
  upsert_var "CI_REGISTRY_USER" "${CI_REGISTRY_USER:-}" "false" "true"
fi
if [ -n "${CI_REGISTRY_PASSWORD:-}" ]; then
  upsert_var "CI_REGISTRY_PASSWORD" "${CI_REGISTRY_PASSWORD:-}" "true" "true"
fi

echo "[gitlab-vars] completed."
