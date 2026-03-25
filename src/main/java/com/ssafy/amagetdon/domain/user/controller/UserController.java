package com.ssafy.amagetdon.domain.user.controller;

import com.ssafy.amagetdon.common.exception.CustomException;
import com.ssafy.amagetdon.common.exception.ErrorCode;
import com.ssafy.amagetdon.common.interceptor.SessionKeys;
import com.ssafy.amagetdon.common.response.ApiResponse;
import com.ssafy.amagetdon.domain.user.docs.UserDocs;
import com.ssafy.amagetdon.domain.user.dto.request.LoginRequest;
import com.ssafy.amagetdon.domain.user.dto.request.SignUpRequest;
import com.ssafy.amagetdon.domain.user.dto.response.DuplicateCheckResponse;
import com.ssafy.amagetdon.domain.user.dto.response.SessionUserResponse;
import com.ssafy.amagetdon.domain.user.entity.User;
import com.ssafy.amagetdon.domain.user.service.UserService;
import io.swagger.v3.oas.annotations.Parameter;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.servlet.http.HttpSession;
import jakarta.validation.Valid;
import jakarta.validation.constraints.NotBlank;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.validation.annotation.Validated;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/users")
@RequiredArgsConstructor
@Validated
@Tag(name = "User", description = "User auth APIs")
public class UserController {

    private final UserService userService;

    @PostMapping("/signup")
    @UserDocs.Signup
    public ResponseEntity<ApiResponse<Void>> signUp(@Valid @RequestBody SignUpRequest request) {
        userService.signUp(request);
        return ResponseEntity.ok(ApiResponse.success("회원가입이 완료되었습니다."));
    }

    @GetMapping("/check-login-id")
    @UserDocs.CheckLoginId
    public ResponseEntity<ApiResponse<Void>> checkLoginId(
            @Parameter(description = "login id")
            @RequestParam
            @NotBlank(message = "loginId는 필수입니다.")
            String loginId
    ) {
        DuplicateCheckResponse response = userService.checkLoginIdDuplicate(loginId);
        if (response.isAvailable()) {
            return ResponseEntity.ok(ApiResponse.success(response.getMessage()));
        }

        return ResponseEntity.status(ErrorCode.DUPLICATE_LOGIN_ID.getStatus())
                .body(ApiResponse.failure(ErrorCode.DUPLICATE_LOGIN_ID.getCode(), response.getMessage()));
    }

    @GetMapping("/check-nickname")
    @UserDocs.CheckNickname
    public ResponseEntity<ApiResponse<Void>> checkNickname(
            @Parameter(description = "nickname")
            @RequestParam
            @NotBlank(message = "nickname은 필수입니다.")
            String nickname
    ) {
        DuplicateCheckResponse response = userService.checkNicknameDuplicate(nickname);
        if (response.isAvailable()) {
            return ResponseEntity.ok(ApiResponse.success(response.getMessage()));
        }

        return ResponseEntity.status(ErrorCode.DUPLICATE_NICKNAME.getStatus())
                .body(ApiResponse.failure(ErrorCode.DUPLICATE_NICKNAME.getCode(), response.getMessage()));
    }

    @PostMapping("/login")
    @UserDocs.Login
    public ResponseEntity<ApiResponse<Void>> login(@Valid @RequestBody LoginRequest request, HttpSession session) {
        User user = userService.login(request);
        session.setAttribute(SessionKeys.USER_ID, user.getUserId());
        session.setAttribute(SessionKeys.LOGIN_ID, user.getLoginId());
        session.setAttribute(SessionKeys.NICKNAME, user.getNickname());
        return ResponseEntity.ok(ApiResponse.success("로그인 성공"));
    }

    @PostMapping("/logout")
    @UserDocs.Logout
    public ResponseEntity<ApiResponse<Void>> logout(HttpSession session) {
        session.invalidate();
        return ResponseEntity.ok(ApiResponse.success("로그아웃 성공"));
    }

    @GetMapping("/me")
    @UserDocs.Me
    public ResponseEntity<SessionUserResponse> me(HttpSession session) {
        Long userId = (Long) session.getAttribute(SessionKeys.USER_ID);
        String loginId = (String) session.getAttribute(SessionKeys.LOGIN_ID);
        String nickname = (String) session.getAttribute(SessionKeys.NICKNAME);

        if (userId == null || loginId == null || nickname == null) {
            throw new CustomException(ErrorCode.AUTH_REQUIRED);
        }

        return ResponseEntity.ok(new SessionUserResponse(userId, loginId, nickname));
    }
}
