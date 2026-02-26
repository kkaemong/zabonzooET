# CI/CD Skeleton (GitLab)

## 원칙

1. CI는 자동: `test -> build`
2. 배포는 수동: `manual`
3. 운영 배포 전까지 deploy job은 비활성 또는 수동 유지

## 현재 파이프라인 파일

- `.gitlab-ci.yml` (저장소 루트)

## 브랜치 정책(초안)

1. feature 브랜치: test/build 수행
2. main 브랜치: test/build 수행
3. deploy: 운영 준비 전까지 `when: manual`

## 추후 확장

1. SCA(의존성 스캔)
2. Container Scan
3. IaC Scan
