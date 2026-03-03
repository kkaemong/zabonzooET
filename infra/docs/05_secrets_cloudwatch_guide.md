# AWS Secrets Manager & CloudWatch Setup Guide

## 1. AWS Secrets Manager Integration (시크릿 매니저 연동)

Spring Boot에서 환경변수를 주입받는 방식은 크게 플러그인을 쓰거나 OS 환경변수로 덮는 방식이 있습니다. MVP의 안정적인 운영과 벤더 의존성을 낮추기 위해 **로컬 쉘 스크립트 기반 주입 방식**을 권장합니다.

### 스크립트 주입 방식 (현재 적용된 경로)

배포 과정에서 `docker compose`가 실행되기 전에 `infra/scripts/render-prod-env.sh`가 보안 변수들을 호출하여 `infra/.env.prod`를 동적으로 생성합니다.

```bash
# EC2 배포 시 예시
export AWS_SECRET_ID=amagetdon/prod
export AWS_REGION=ap-northeast-2
./infra/scripts/deploy.sh <registry>/<project>/backend:<tag>
```

> [!NOTE]
> 해당 방법을 사용하려면 반드시 **EC2 인스턴스에 IAM Role (SecretsManagerRead권한)**이 할당되어 있어야 합니다.

<br>

## 2. AWS CloudWatch Log Agent 설정

Spring Boot에서 직접 Logback appender를 통해 통신하지 않고, **Docker 데몬의 `awslogs` 로깅 드라이버**를 활용하여 인프라 레벨에서 CloudWatch로 로그를 전송합니다.

### `docker-compose.prod.yml` 적용 상태

```yaml
services:
  backend:
    image: ${IMAGE_TAG:?IMAGE_TAG is required}
    env_file:
      - .env.prod
```

> [!IMPORTANT]
> CloudWatch 로그 전송(`awslogs`)은 현재 기본 compose에는 아직 미적용입니다. 적용하려면 `docker-compose.prod.yml`에 `logging.driver=awslogs`를 추가하고 EC2 IAM Role 권한을 연결하세요.
