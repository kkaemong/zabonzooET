#!/usr/bin/env sh
set -eu

log() {
  printf '[jira-sync] %s\n' "$*"
}

require_var() {
  var_name="$1"
  eval "var_value=\${$var_name:-}"
  if [ -z "$var_value" ]; then
    log "missing required variable: $var_name"
    return 1
  fi
  return 0
}

if [ "${JIRA_SYNC_ENABLED:-true}" != "true" ]; then
  log "disabled (JIRA_SYNC_ENABLED=${JIRA_SYNC_ENABLED:-})"
  exit 0
fi

if ! command -v curl >/dev/null 2>&1; then
  log "curl is required"
  exit 1
fi

require_var JIRA_BASE_URL
require_var JIRA_USER_EMAIL
require_var JIRA_API_TOKEN
require_var JIRA_DONE_TRANSITION_ID

source_text=$(printf '%s\n%s\n%s\n' "${CI_MERGE_REQUEST_TITLE:-}" "${CI_COMMIT_TITLE:-}" "${CI_COMMIT_MESSAGE:-}")
issue_keys="$(printf '%s' "$source_text" | grep -Eo '[A-Z][A-Z0-9]+-[0-9]+' | sort -u || true)"

if [ -z "$issue_keys" ]; then
  log "no issue key found in commit/MR metadata"
  exit 0
fi

base_url="${JIRA_BASE_URL%/}"
failed=0

for issue_key in $issue_keys; do
  log "transitioning $issue_key -> ${JIRA_DONE_TRANSITION_ID}"

  http_code="$(curl -sS -o /tmp/jira-transition-response.txt -w '%{http_code}' \
    -u "${JIRA_USER_EMAIL}:${JIRA_API_TOKEN}" \
    -H 'Accept: application/json' \
    -H 'Content-Type: application/json' \
    -X POST "${base_url}/rest/api/3/issue/${issue_key}/transitions" \
    --data "{\"transition\":{\"id\":\"${JIRA_DONE_TRANSITION_ID}\"}}")"

  if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
    log "ok ($issue_key, HTTP $http_code)"
  else
    log "failed ($issue_key, HTTP $http_code)"
    sed -n '1,120p' /tmp/jira-transition-response.txt
    failed=1
  fi
done

rm -f /tmp/jira-transition-response.txt

if [ "$failed" -ne 0 ]; then
  exit 1
fi

log "done"
