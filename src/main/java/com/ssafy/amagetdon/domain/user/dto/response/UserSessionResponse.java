package com.ssafy.amagetdon.domain.user.dto.response;

import lombok.Builder;
import lombok.Getter;

@Getter
public class UserSessionResponse {

    private final Long id;
    private final String loginId;
    private final String nickname;

    @Builder
    private UserSessionResponse(Long id, String loginId, String nickname) {
        this.id = id;
        this.loginId = loginId;
        this.nickname = nickname;
    }

    public static UserSessionResponse of(Long id, String loginId, String nickname) {
        return UserSessionResponse.builder()
                .id(id)
                .loginId(loginId)
                .nickname(nickname)
                .build();
    }
}
