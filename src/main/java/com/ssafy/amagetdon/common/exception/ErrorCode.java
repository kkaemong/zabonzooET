package com.ssafy.amagetdon.common.exception;

import org.springframework.http.HttpStatus;

public enum ErrorCode {
    DUPLICATE_LOGIN_ID("409", HttpStatus.CONFLICT, "이미 사용 중인 아이디입니다."),
    DUPLICATE_NICKNAME("409", HttpStatus.CONFLICT, "이미 사용 중인 닉네임입니다."),
    LOGIN_FAILED("401", HttpStatus.UNAUTHORIZED, "아이디 또는 비밀번호가 올바르지 않습니다."),
    AUTH_REQUIRED("401", HttpStatus.UNAUTHORIZED, "로그인이 필요합니다."),
    INVALID_REQUEST("400", HttpStatus.BAD_REQUEST, "잘못된 요청입니다."),
    SERVER_ERROR("500", HttpStatus.INTERNAL_SERVER_ERROR, "서버 오류가 발생했습니다.");

    private final String code;
    private final HttpStatus status;
    private final String message;

    ErrorCode(String code, HttpStatus status, String message) {
        this.code = code;
        this.status = status;
        this.message = message;
    }

    public HttpStatus getStatus() {
        return status;
    }

    public String getMessage() {
        return message;
    }

    public String getCode() {
        return code;
    }
}
