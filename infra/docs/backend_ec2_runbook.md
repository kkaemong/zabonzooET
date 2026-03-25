# Backend-First EC2 Runbook

This runbook deploys backend, postgres, redis, and nginx first.
It does not assume the frontend is ready.

## 1. Prepare EC2

Run on the EC2 host:

```bash
cd /home/ubuntu/S14P21A507
./infra/scripts/init-ec2.sh
newgrp docker
```

Recommended security group rules:

- 80/tcp open
- 443/tcp open
- 22/tcp limited to your office or team IPs

## 2. Prepare backend env

Create the backend env file from the template:

```bash
cd /home/ubuntu/S14P21A507
cp infra/docs/templates/backend.env.example infra/.env.backend.prod
chmod 600 infra/.env.backend.prod
```

Update the values in `infra/.env.backend.prod`:

- `BACKEND_IMAGE`
- `POSTGRES_PASSWORD`
- any host or port values you need to override

If the database password contains special characters such as `#`, `$`, or `!`,
keep it quoted in the env file:

```text
POSTGRES_PASSWORD="Qx7!mT9@pL2#vK8$"
```

The current backend team settings map naturally to:

- `SPRING_PROFILES_ACTIVE=prod`
- `POSTGRES_HOST=local-postgres`
- `POSTGRES_PORT=5432`
- `POSTGRES_DB=amagetdon`
- `POSTGRES_USER=A507`
- `REDIS_HOST=local-redis`
- `REDIS_PORT=6379`

## 3. Build and push the backend image

From a machine that can access the repository and registry:

```bash
docker build -t <your-registry>/amagetdon-backend:<tag> .
docker push <your-registry>/amagetdon-backend:<tag>
```

Write the same image tag into `infra/.env.backend.prod` as `BACKEND_IMAGE=...`.

## 4. Export the teammate local Docker DB

On the teammate machine:

Full database backup:

```bash
docker exec -t local-postgres pg_dump -U A507 -d amagetdon -Fc > amagetdon.dump
```

If you only want critical gameplay data first:

```bash
docker exec -t local-postgres pg_dump -U A507 -d amagetdon \
  --data-only \
  --table=item \
  --table=user_stat \
  --table=inventory \
  > amagetdon-seed.sql
```

Copy the dump to EC2:

```bash
scp amagetdon.dump ubuntu@<EC2_IP>:/home/ubuntu/
```

## 5. Restore the DB on EC2

Start postgres first, then restore:

```bash
cd /home/ubuntu/S14P21A507
./infra/scripts/restore-postgres-dump.sh /home/ubuntu/amagetdon.dump
```

Or with a plain SQL file:

```bash
./infra/scripts/restore-postgres-dump.sh /home/ubuntu/amagetdon-seed.sql
```

## 6. Deploy backend + infra

```bash
cd /home/ubuntu/S14P21A507
./infra/scripts/deploy-backend.sh <your-registry>/amagetdon-backend:<tag>
```

This starts:

- `nginx`
- `backend`
- `local-postgres`
- `local-redis`

## 7. Verify

On the EC2 host:

```bash
curl http://127.0.0.1/actuator/health
curl http://127.0.0.1/swagger-ui/index.html
docker compose --env-file infra/.env.backend.prod -f infra/docker-compose.backend.yml ps
```

From your local machine:

```bash
curl http://<EC2_IP>/actuator/health
```

## 8. Unity local client test

The current lobby default backend URL is:

- [LobbyAuthApi.cs](/C:/m/Assets/Scripts/LobbyAuthApi.cs#L9)

If you want the local Unity client to hit EC2, set the lobby backend URL in the inspector:

- [LobbyDirector.cs](/C:/m/Assets/Scripts/LobbyDirector.cs#L30)

Change it from `http://localhost:8080` to:

```text
http://<EC2_IP>
```

Or, if you terminate TLS at nginx later:

```text
https://<your-domain>
```

## 9. Current limitation

This backend-first deployment is the recommended path while:

- stage select integration is still in progress
- the Unity frontend is not ready for production hosting
- the old full-stack infra still assumes a separate frontend container
