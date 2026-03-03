# EC2 Launch-Day Checklist

Goal: make production deployment possible immediately after EC2 instance issuance, without changing `BE/`.

## 1. Before EC2 Is Issued

1. Decide the image registry and repository path (`<registry>/<project>/backend`).
2. Prepare application secret source:
   - Option A: AWS Secrets Manager (recommended): keep `db_host`, `db_port`, `db_name`, `db_username`, `db_password`, `redis_host`, `redis_port`.
   - Option B: direct env export on the EC2 host.
3. Build/push backend image with infra Dockerfile:
   - `./infra/scripts/build-push-image.sh <registry>/<project>/backend:<tag>`
4. Confirm the image is pullable from the future EC2 host account/network.

## 2. Right After EC2 Is Issued

1. SSH into EC2.
2. Bootstrap dependencies:
   - `./infra/scripts/init-ec2.sh`
3. Re-login or run `newgrp docker`.
4. Clone the repository and move to project root (`S14P21A507`).
5. Verify prerequisites:
   - `docker --version`
   - `docker compose version`
   - `aws --version` (if using Secrets Manager)
   - `jq --version` (if using Secrets Manager)

## 3. First Deployment

1. Export required runtime variables:
   - required always: `IMAGE_TAG`
   - required for secrets mode: `AWS_SECRET_ID` (optional `AWS_REGION`)
   - optional for private registry auth: `REGISTRY_HOST`, `REGISTRY_USERNAME`, `REGISTRY_PASSWORD`
2. Deploy:
   - `./infra/scripts/deploy.sh <registry>/<project>/backend:<tag>`
3. Verify:
   - `docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml ps`
   - `curl -f http://127.0.0.1/health`

## 4. Failure Handling

1. Check container status:
   - `docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml ps`
2. Inspect logs:
   - `docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml logs backend --tail 200`
   - `docker compose --env-file infra/.env.prod -f infra/docker-compose.prod.yml logs nginx --tail 200`
3. Re-deploy known-good image tag:
   - `./infra/scripts/deploy.sh <registry>/<project>/backend:<known-good-tag>`
