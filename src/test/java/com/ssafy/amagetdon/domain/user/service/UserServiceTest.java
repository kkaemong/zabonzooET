package com.ssafy.amagetdon.domain.user.service;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import com.ssafy.amagetdon.common.exception.CustomException;
import com.ssafy.amagetdon.common.exception.ErrorCode;
import com.ssafy.amagetdon.domain.game.entity.UserStat;
import com.ssafy.amagetdon.domain.game.repository.UserStatRepository;
import com.ssafy.amagetdon.domain.user.dto.request.LoginRequest;
import com.ssafy.amagetdon.domain.user.dto.request.SignUpRequest;
import com.ssafy.amagetdon.domain.user.dto.response.DuplicateCheckResponse;
import com.ssafy.amagetdon.domain.user.entity.User;
import com.ssafy.amagetdon.domain.user.repository.UserRepository;
import java.util.Optional;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.ArgumentCaptor;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.security.crypto.password.PasswordEncoder;

@ExtendWith(MockitoExtension.class)
class UserServiceTest {

    @Mock
    private UserRepository userRepository;

    @Mock
    private PasswordEncoder passwordEncoder;

    @Mock
    private UserStatRepository userStatRepository;

    @InjectMocks
    private UserService userService;

    @Test
    void signUp_success() {
        SignUpRequest request = new SignUpRequestTestBuilder()
                .loginId("test123")
                .password("1234")
                .nickname("harin")
                .build();

        when(userRepository.existsByLoginId("test123")).thenReturn(false);
        when(userRepository.existsByNickname("harin")).thenReturn(false);
        when(passwordEncoder.encode("1234")).thenReturn("encoded");
        when(userRepository.save(any(User.class))).thenAnswer(invocation -> invocation.getArgument(0));

        userService.signUp(request);

        ArgumentCaptor<User> captor = ArgumentCaptor.forClass(User.class);
        verify(userRepository).save(captor.capture());
        User saved = captor.getValue();
        assertThat(saved.getLoginId()).isEqualTo("test123");
        assertThat(saved.getPassword()).isEqualTo("encoded");
        assertThat(saved.getNickname()).isEqualTo("harin");
        verify(userStatRepository).save(any(UserStat.class));
    }

    @Test
    void signUp_duplicateLoginId() {
        SignUpRequest request = new SignUpRequestTestBuilder()
                .loginId("dup")
                .password("1234")
                .nickname("harin")
                .build();

        when(userRepository.existsByLoginId("dup")).thenReturn(true);

        assertThatThrownBy(() -> userService.signUp(request))
                .isInstanceOf(CustomException.class)
                .satisfies(ex -> assertThat(((CustomException) ex).getErrorCode())
                        .isEqualTo(ErrorCode.DUPLICATE_LOGIN_ID));
    }

    @Test
    void signUp_duplicateNickname() {
        SignUpRequest request = new SignUpRequestTestBuilder()
                .loginId("test123")
                .password("1234")
                .nickname("dupNick")
                .build();

        when(userRepository.existsByLoginId("test123")).thenReturn(false);
        when(userRepository.existsByNickname("dupNick")).thenReturn(true);

        assertThatThrownBy(() -> userService.signUp(request))
                .isInstanceOf(CustomException.class)
                .satisfies(ex -> assertThat(((CustomException) ex).getErrorCode())
                        .isEqualTo(ErrorCode.DUPLICATE_NICKNAME));
    }

    @Test
    void checkLoginIdDuplicate_available() {
        when(userRepository.existsByLoginId("newId")).thenReturn(false);

        DuplicateCheckResponse response = userService.checkLoginIdDuplicate("newId");

        assertThat(response.isAvailable()).isTrue();
        assertThat(response.getMessage()).isEqualTo("사용 가능한 아이디입니다.");
    }

    @Test
    void checkLoginIdDuplicate_unavailable() {
        when(userRepository.existsByLoginId("dupId")).thenReturn(true);

        DuplicateCheckResponse response = userService.checkLoginIdDuplicate("dupId");

        assertThat(response.isAvailable()).isFalse();
        assertThat(response.getMessage()).isEqualTo("이미 사용 중인 아이디입니다.");
    }

    @Test
    void checkNicknameDuplicate_available() {
        when(userRepository.existsByNickname("newNick")).thenReturn(false);

        DuplicateCheckResponse response = userService.checkNicknameDuplicate("newNick");

        assertThat(response.isAvailable()).isTrue();
        assertThat(response.getMessage()).isEqualTo("사용 가능한 닉네임입니다.");
    }

    @Test
    void checkNicknameDuplicate_unavailable() {
        when(userRepository.existsByNickname("dupNick")).thenReturn(true);

        DuplicateCheckResponse response = userService.checkNicknameDuplicate("dupNick");

        assertThat(response.isAvailable()).isFalse();
        assertThat(response.getMessage()).isEqualTo("이미 사용 중인 닉네임입니다.");
    }

    @Test
    void login_success() {
        LoginRequest request = new LoginRequestTestBuilder()
                .loginId("test123")
                .password("1234")
                .build();
        User user = User.of("test123", "encoded", "nick");

        when(userRepository.findByLoginId("test123")).thenReturn(Optional.of(user));
        when(passwordEncoder.matches(eq("1234"), eq("encoded"))).thenReturn(true);

        User result = userService.login(request);

        verify(passwordEncoder).matches("1234", "encoded");
        assertThat(result.getLoginId()).isEqualTo("test123");
    }

    @Test
    void login_fail_userNotFound() {
        LoginRequest request = new LoginRequestTestBuilder()
                .loginId("missing")
                .password("1234")
                .build();

        when(userRepository.findByLoginId("missing")).thenReturn(Optional.empty());

        assertThatThrownBy(() -> userService.login(request))
                .isInstanceOf(CustomException.class)
                .satisfies(ex -> assertThat(((CustomException) ex).getErrorCode())
                        .isEqualTo(ErrorCode.LOGIN_FAILED));
    }

    @Test
    void login_fail_passwordMismatch() {
        LoginRequest request = new LoginRequestTestBuilder()
                .loginId("test123")
                .password("wrong")
                .build();
        User user = User.of("test123", "encoded", "nick");

        when(userRepository.findByLoginId("test123")).thenReturn(Optional.of(user));
        when(passwordEncoder.matches(eq("wrong"), eq("encoded"))).thenReturn(false);

        assertThatThrownBy(() -> userService.login(request))
                .isInstanceOf(CustomException.class)
                .satisfies(ex -> assertThat(((CustomException) ex).getErrorCode())
                        .isEqualTo(ErrorCode.LOGIN_FAILED));
    }

    private static class SignUpRequestTestBuilder {
        private String loginId;
        private String password;
        private String nickname;

        SignUpRequestTestBuilder loginId(String loginId) {
            this.loginId = loginId;
            return this;
        }

        SignUpRequestTestBuilder password(String password) {
            this.password = password;
            return this;
        }

        SignUpRequestTestBuilder nickname(String nickname) {
            this.nickname = nickname;
            return this;
        }

        SignUpRequest build() {
            SignUpRequest request = new SignUpRequest();
            TestReflectionUtils.setField(request, "loginId", loginId);
            TestReflectionUtils.setField(request, "password", password);
            TestReflectionUtils.setField(request, "nickname", nickname);
            return request;
        }
    }

    private static class LoginRequestTestBuilder {
        private String loginId;
        private String password;

        LoginRequestTestBuilder loginId(String loginId) {
            this.loginId = loginId;
            return this;
        }

        LoginRequestTestBuilder password(String password) {
            this.password = password;
            return this;
        }

        LoginRequest build() {
            LoginRequest request = new LoginRequest();
            TestReflectionUtils.setField(request, "loginId", loginId);
            TestReflectionUtils.setField(request, "password", password);
            return request;
        }
    }

    private static class TestReflectionUtils {
        private static void setField(Object target, String fieldName, Object value) {
            try {
                java.lang.reflect.Field field = target.getClass().getDeclaredField(fieldName);
                field.setAccessible(true);
                field.set(target, value);
            } catch (ReflectiveOperationException ex) {
                throw new IllegalStateException(ex);
            }
        }
    }
}
