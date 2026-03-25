package com.ssafy.amagetdon.domain.user.docs;

import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.media.Content;
import io.swagger.v3.oas.annotations.media.ExampleObject;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import io.swagger.v3.oas.annotations.responses.ApiResponses;
import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

public final class UserDocs {

    private UserDocs() {
    }

    @Target(ElementType.METHOD)
    @Retention(RetentionPolicy.RUNTIME)
    @Operation(summary = "회원가입")
    @ApiResponses({
            @ApiResponse(responseCode = "200", description = "회원가입 성공",
                    content = @Content(mediaType = "application/json",
                            examples = @ExampleObject(value = """
                                    {"success":true,"message":"회원가입이 완료되었습니다.","errorCode":null}
                                    """))),
            @ApiResponse(responseCode = "409", description = "중복",
                    content = @Content(mediaType = "application/json",
                            examples = @ExampleObject(value = """
                                    {"success":false,"message":"이미 사용 중인 아이디입니다.","errorCode":"409"}
                                    """)))
    })
    public @interface Signup {
    }

    @Target(ElementType.METHOD)
    @Retention(RetentionPolicy.RUNTIME)
    @Operation(summary = "아이디 중복 확인")
    @ApiResponses({
            @ApiResponse(responseCode = "200", description = "확인 성공",
                    content = @Content(mediaType = "application/json",
                            examples = {
                                    @ExampleObject(name = "사용 가능", value = """
                                            {"success":true,"message":"사용 가능한 아이디입니다.","errorCode":null}
                                            """),
                                    @ExampleObject(name = "이미 사용 중", value = """
                                            {"success":false,"message":"이미 사용 중인 아이디입니다.","errorCode":"409"}
                                            """)
                            }))
    })
    public @interface CheckLoginId {
    }

    @Target(ElementType.METHOD)
    @Retention(RetentionPolicy.RUNTIME)
    @Operation(summary = "닉네임 중복 확인")
    @ApiResponses({
            @ApiResponse(responseCode = "200", description = "확인 성공",
                    content = @Content(mediaType = "application/json",
                            examples = {
                                    @ExampleObject(name = "사용 가능", value = """
                                            {"success":true,"message":"사용 가능한 닉네임입니다.","errorCode":null}
                                            """),
                                    @ExampleObject(name = "이미 사용 중", value = """
                                            {"success":false,"message":"이미 사용 중인 닉네임입니다.","errorCode":"409"}
                                            """)
                            }))
    })
    public @interface CheckNickname {
    }

    @Target(ElementType.METHOD)
    @Retention(RetentionPolicy.RUNTIME)
    @Operation(summary = "로그인 (세션 생성)")
    @ApiResponses({
            @ApiResponse(responseCode = "200", description = "로그인 성공",
                    content = @Content(mediaType = "application/json",
                            examples = @ExampleObject(value = """
                                    {"success":true,"message":"로그인 성공","errorCode":null}
                                    """))),
            @ApiResponse(responseCode = "401", description = "로그인 실패",
                    content = @Content(mediaType = "application/json",
                            examples = @ExampleObject(value = """
                                    {"success":false,"message":"아이디 또는 비밀번호가 올바르지 않습니다.","errorCode":"401"}
                                    """)))
    })
    public @interface Login {
    }

    @Target(ElementType.METHOD)
    @Retention(RetentionPolicy.RUNTIME)
    @Operation(summary = "로그아웃 (세션 만료)")
    @ApiResponses({
            @ApiResponse(responseCode = "200", description = "로그아웃 성공",
                    content = @Content(mediaType = "application/json",
                            examples = @ExampleObject(value = """
                                    {"success":true,"message":"로그아웃 성공","errorCode":null}
                                    """)))
    })
    public @interface Logout {
    }

    @Target(ElementType.METHOD)
    @Retention(RetentionPolicy.RUNTIME)
    @Operation(summary = "세션 사용자 조회")
    @ApiResponses({
            @ApiResponse(responseCode = "200", description = "조회 성공",
                    content = @Content(mediaType = "application/json",
                            examples = @ExampleObject(value = """
                                    {"success":true,"message":"세션 사용자 조회 성공","errorCode":null}
                                    """))),
            @ApiResponse(responseCode = "401", description = "인증 필요",
                    content = @Content(mediaType = "application/json",
                            examples = @ExampleObject(value = """
                                    {"success":false,"message":"로그인이 필요합니다.","errorCode":"401"}
                                    """)))
    })
    public @interface Me {
    }
}
