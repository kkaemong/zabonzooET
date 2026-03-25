package com.ssafy.amagetdon.domain.user.dto.response;

import lombok.Builder;
import lombok.Getter;

@Getter
public class DuplicateCheckResponse {

    private final boolean available;
    private final String message;

    @Builder
    private DuplicateCheckResponse(boolean available, String message) {
        this.available = available;
        this.message = message;
    }

    public static DuplicateCheckResponse of(boolean available, String message) {
        return DuplicateCheckResponse.builder()
                .available(available)
                .message(message)
                .build();
    }
}
