package com.ssafy.amagetdon.domain.user.dto.response;

import lombok.Builder;
import lombok.Getter;

@Getter
public class SimpleResponse {

    private final String message;

    @Builder
    private SimpleResponse(String message) {
        this.message = message;
    }

    public static SimpleResponse of(String message) {
        return SimpleResponse.builder()
                .message(message)
                .build();
    }
}
