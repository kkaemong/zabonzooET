package com.ssafy.amagetdon.domain.user.dto.response;

import lombok.AllArgsConstructor;
import lombok.Getter;

@Getter
@AllArgsConstructor
public class SessionUserResponse {

    private Long userId;
    private String loginId;
    private String nickname;
}
