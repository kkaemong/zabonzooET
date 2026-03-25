package com.ssafy.amagetdon.domain.user.dto.request;

import jakarta.validation.constraints.NotBlank;
import lombok.Getter;
import lombok.NoArgsConstructor;

@Getter
@NoArgsConstructor
public class LoginRequest {

    @NotBlank(message = "loginId는 필수입니다.")
    private String loginId;

    @NotBlank(message = "password는 필수입니다.")
    private String password;
}
