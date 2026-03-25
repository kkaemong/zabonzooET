package com.ssafy.amagetdon.domain.game.service;

import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpMethod;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

@Service
public class GmsService {

    private static final String GMS_URL = "https://gms.ssafy.io/gmsapi/api.openai.com/v1/chat/completions";
    private static final String MODEL = "gpt-5-mini";

    private final RestTemplate restTemplate = new RestTemplate();

    public String generateFeedback(
            String stageId,
            String choice,
            String subOptionCode,
            int baseCoin,
            int changeCoin,
            String resultType
    ) {
        String gmsKey = System.getenv("GMS_KEY");
        if (gmsKey == null || gmsKey.isBlank()) {
            throw new IllegalStateException("GMS_KEY 환경변수가 설정되지 않았습니다.");
        }

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        headers.setBearerAuth(gmsKey);

        Map<String, Object> developerMessage = new HashMap<>();
        developerMessage.put("role", "developer");
        developerMessage.put("content", "항상 한국어로 답하고, 초등학생도 이해할 수 있게 2~3문장으로 쉽게 설명해.");

        Map<String, Object> userMessage = new HashMap<>();
        userMessage.put("role", "user");
        userMessage.put("content", buildPrompt(stageId, choice, subOptionCode, baseCoin, changeCoin, resultType));

        Map<String, Object> body = new HashMap<>();
        body.put("model", MODEL);
        body.put("messages", List.of(developerMessage, userMessage));

        HttpEntity<Map<String, Object>> requestEntity = new HttpEntity<>(body, headers);

        ResponseEntity<Map> response = restTemplate.exchange(
                GMS_URL,
                HttpMethod.POST,
                requestEntity,
                Map.class
        );

        String content = extractContent(response.getBody());
        if (content == null || content.isBlank()) {
            throw new IllegalStateException("GMS 응답에서 content를 추출하지 못했습니다.");
        }

        return content;
    }

    private String buildPrompt(
            String stageId,
            String choice,
            String subOptionCode,
            int baseCoin,
            int changeCoin,
            String resultType
    ) {
        return """
                너는 금융 학습 게임의 경제 해설자다.
                사용자의 선택 결과를 보고, 왜 이런 결과가 나왔는지 시대 배경과 함께 2~3문장으로 쉽게 설명해라.
                너무 딱딱하지 않게, 게임 결과 설명처럼 작성해라.

                [입력 정보]
                시대: %s
                상위 선택: %s
                세부 선택: %s
                기준 코인: %d
                변화 코인: %d
                결과 타입: %s

                [출력 조건]
                - 한국어
                - 2~3문장
                - 초등학생도 이해할 수 있게 쉽게
                - 시대 배경을 1문장 이상 포함
                """.formatted(stageId, choice, subOptionCode, baseCoin, changeCoin, resultType);
    }

    @SuppressWarnings("unchecked")
    private String extractContent(Map<String, Object> body) {
        if (body == null) {
            return null;
        }

        Object choicesObj = body.get("choices");
        if (!(choicesObj instanceof List<?> choices) || choices.isEmpty()) {
            return null;
        }

        Object firstChoiceObj = choices.get(0);
        if (!(firstChoiceObj instanceof Map<?, ?> firstChoice)) {
            return null;
        }

        Object messageObj = firstChoice.get("message");
        if (!(messageObj instanceof Map<?, ?> message)) {
            return null;
        }

        Object contentObj = message.get("content");
        if (!(contentObj instanceof String content)) {
            return null;
        }

        return content.trim();
    }
}