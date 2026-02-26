# Ops Runbook (초안)

## 1. 배포 전 점검

1. 최근 main 기준 빌드 성공 확인
2. 필수 환경변수 등록 확인
3. 변경 범위와 롤백 방법 확인

## 2. 장애 1차 점검 순서

1. 컨테이너 상태 확인
2. 애플리케이션 로그 확인
3. DB/Redis 연결 확인
4. 헬스 체크 확인

## 3. 기본 확인 명령

```bash
docker ps
docker logs <app-container> --tail 200
docker exec -it local-postgres psql -U A507 -d amagetdon -c "select 1;"
docker exec -it local-redis redis-cli ping
```

## 4. 롤백 원칙

1. 장애 발생 시 즉시 이전 정상 버전으로 복귀
2. 롤백 후 원인 분석은 별도 이슈로 분리
3. 재배포는 원인/재현/대응안 확인 후 진행
