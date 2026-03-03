# Day-0 Command Set

Target: make infra easy for teammates and keep Jira + GitLab integration active from day zero.

## 1) Maintainer Laptop: One-Time Setup

```bash
cd /path/to/S14P21A507
chmod +x infra/scripts/*.sh

# 1) install Jira issue-key commit hooks for the repository
./infra/scripts/install-jira-git-hooks.sh

# 2) build and push backend image without touching BE/
export IMAGE_TAG=<registry>/<project>/backend:day0
./infra/scripts/build-push-image.sh "$IMAGE_TAG"
```

## 2) GitLab CI/CD Variables: API One-Liner

```bash
cd /path/to/S14P21A507

export GITLAB_TOKEN=<gitlab_pat_with_api_scope>
export GITLAB_PROJECT_ID=<numeric_id_or_urlencoded_path>
export EC2_IP=<ec2_public_ip_or_dns>
export EC2_USER=<ec2_ssh_user>
export SSH_PRIVATE_KEY="$(cat ~/.ssh/<deploy_key>)"
export AWS_SECRET_ID=amagetdon/prod
export AWS_REGION=ap-northeast-2

./infra/scripts/day0-set-gitlab-vars.sh
```

## 3) EC2: Bootstrap After Instance Issuance

```bash
ssh <ec2_user>@<ec2_ip>

git clone <repo_url>
cd S14P21A507
chmod +x infra/scripts/*.sh
./infra/scripts/init-ec2.sh
newgrp docker
```

## 4) EC2: First Deployment

```bash
cd /path/to/S14P21A507

export AWS_SECRET_ID=amagetdon/prod
export AWS_REGION=ap-northeast-2
export REGISTRY_HOST=<registry_host>      # optional
export REGISTRY_USERNAME=<registry_user>  # optional
export REGISTRY_PASSWORD=<registry_pass>  # optional

./infra/scripts/deploy.sh <registry>/<project>/backend:day0
docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml ps
curl -f http://127.0.0.1/health
```

## 5) Jira + GitLab Working Rules

```bash
# branch naming: include Jira key
git checkout -b feature/A507-123-login-api

# commit: hook auto-prefixes issue key when possible
git commit -m "implement login API"

# explicit smart commit format (optional)
git commit -m "A507-123 #comment deploy-ready #time 30m"
```

## 6) Quick Verification

```bash
# CI target branch pipeline
git push origin feature/A507-123-login-api

# merge to main and confirm deploy job
git checkout main
git merge --no-ff feature/A507-123-login-api
git push origin main
```

If your Jira workspace is not yet connected to GitLab, complete the one-time OAuth/app connection in Jira and GitLab UI first, then use the command set above.
