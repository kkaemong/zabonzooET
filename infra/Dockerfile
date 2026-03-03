# syntax=docker/dockerfile:1.7

FROM gradle:8.10.2-jdk17 AS build
WORKDIR /workspace
COPY BE/ ./
RUN gradle clean bootJar -x test --no-daemon

FROM eclipse-temurin:17-jre
WORKDIR /app
COPY --from=build /workspace/build/libs/*.jar /app/app.jar
EXPOSE 8080
ENTRYPOINT ["java", "-jar", "/app/app.jar"]
