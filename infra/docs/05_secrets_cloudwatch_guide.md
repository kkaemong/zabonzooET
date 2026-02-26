# AWS Secrets Manager & CloudWatch Setup Guide

## 1. AWS Secrets Manager Integration (시크릿 매니저 연동)

Spring Boot에서 환경변수를 주입받는 방식은 크게 플러그인을 쓰거나 OS 환경변수로 덮는 방식이 있습니다. MVP의 안정적인 운영과 벤더 의존성을 낮추기 위해 **로컬 쉘 스크립트 기반 주입 방식**을 권장합니다.

### 스크립트 주입 예제 (deploy.sh 수정 안내)

배포 과정에서 `docker-compose`가 실행되기 전, AWS CLI명령어를 통해 보안 변수들을 호출하여 `.env` 파일을 동적으로 생성합니다.

```bash
# 1. AWS CLI를 통해 Secrets Manager 값 호출 (ap-northeast-2 기준)
SECRET_JSON=$(aws secretsmanager get-secret-value --secret-id amagetdon/prod --query SecretString --output text --region ap-northeast-2)

# 2. jq(JSON Parser)를 이용하여 .env 재생성
echo "SPRING_DATASOURCE_PASSWORD=$(jq -r '.db_password' <<< \$SECRET_JSON)" > .env.prod
echo "JWT_SECRET=$(jq -r '.jwt_secret' <<< \$SECRET_JSON)" >> .env.prod
```

> [!NOTE]
> 해당 방법을 사용하려면 반드시 **EC2 인스턴스에 IAM Role (SecretsManagerRead권한)**이 할당되어 있어야 합니다.

<br>

## 2. AWS CloudWatch Log Agent 설정

Spring Boot에서 직접 Logback appender를 통해 통신하지 않고, **Docker 데몬의 `awslogs` 로깅 드라이버**를 활용하여 인프라 레벨에서 CloudWatch로 로그를 전송합니다.

### `docker-compose.prod.yml` 적용 예제

```yaml
services:
  backend:
    image: ${IMAGE_TAG:-amagetdon-backend:latest}
    logging:
      driver: awslogs
      options:
        awslogs-region: "ap-northeast-2"
        awslogs-group: "/amagetdon/backend"
        awslogs-stream-prefix: "prod-server"
        awslogs-create-group: "true"
```

> [!IMPORTANT]
> `awslogs` 드라이버를 원활하게 쓰기 위해선 마찬가지로 **EC2 IAM Role에 `CloudWatchLogsFullAccess` 권한**이 연결되어 있어야 합니다.
