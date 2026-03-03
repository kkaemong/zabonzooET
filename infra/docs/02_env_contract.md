# Env Contract (BE 기준)

목적: 팀원이 같은 키 이름/의미를 쓰도록 고정.

## 필수 변수

1. `SPRING_PROFILES_ACTIVE`
- 기본: `local`

2. `POSTGRES_HOST`
- 로컬: `localhost`
- 컨테이너 내부 통신 시: `postgres`

3. `POSTGRES_PORT`
- 기본: `5432`

4. `POSTGRES_DB`
- 기본: `amagetdon`

5. `POSTGRES_USER`
- 기본: `A507`

6. `POSTGRES_PASSWORD`
- 개인 로컬 비밀번호
- 절대 커밋 금지

7. `REDIS_PORT`
- `BE/docker-compose.yml`의 Redis 포트 매핑 변수
- 기본: `6379`

8. `SPRING_REDIS_HOST`
- 로컬: `localhost`
- 컨테이너 내부 통신 시: `redis`

9. `SPRING_REDIS_PORT`
- 애플리케이션 Redis 접속 포트 변수
- 기본: `6379`
- 로컬에서는 `REDIS_PORT`와 동일 값 사용 권장

## 규칙

1. 실제 값은 `BE/.env`에만 저장
2. 공유 템플릿은 `infra/docs/templates/be.env.example`만 사용
3. 운영/배포 비밀값은 GitLab CI/CD Variables 또는 AWS Secrets Manager에서 관리
