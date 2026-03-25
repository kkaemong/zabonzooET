package com.ssafy.amagetdon.domain.user.service;

import com.ssafy.amagetdon.domain.user.dto.request.LoginRequest;
import com.ssafy.amagetdon.domain.user.dto.request.SignUpRequest;
import com.ssafy.amagetdon.domain.user.dto.response.DuplicateCheckResponse;
import com.ssafy.amagetdon.domain.game.entity.UserStat;
import com.ssafy.amagetdon.domain.game.repository.UserStatRepository;
import com.ssafy.amagetdon.domain.user.entity.User;
import com.ssafy.amagetdon.common.exception.ErrorCode;
import com.ssafy.amagetdon.domain.user.repository.UserRepository;
import com.ssafy.amagetdon.common.exception.CustomException;
import lombok.RequiredArgsConstructor;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
@RequiredArgsConstructor
public class UserService {

    private static final String AVAILABLE_LOGIN_ID_MESSAGE = "사용 가능한 아이디입니다.";
    private static final String AVAILABLE_NICKNAME_MESSAGE = "사용 가능한 닉네임입니다.";

    private static final int INITIAL_COIN_BALANCE = 3000;
    private static final int INITIAL_BASE_HP = 100;
    private static final double INITIAL_BASE_SPEED = 2.0;
    private static final int INITIAL_BOOSTER_BONUS_SEC = 0;

    private final UserRepository userRepository;
    private final UserStatRepository userStatRepository;
    private final PasswordEncoder passwordEncoder;

    @Transactional
    public void signUp(SignUpRequest request) {
        if (userRepository.existsByLoginId(request.getLoginId())) {
            throw new CustomException(ErrorCode.DUPLICATE_LOGIN_ID);
        }
        if (userRepository.existsByNickname(request.getNickname())) {
            throw new CustomException(ErrorCode.DUPLICATE_NICKNAME);
        }

        String encodedPassword = passwordEncoder.encode(request.getPassword());
        User user = User.of(request.getLoginId(), encodedPassword, request.getNickname());
        User savedUser = userRepository.save(user);

        UserStat initialUserStat = new UserStat(
                savedUser,
                INITIAL_COIN_BALANCE,
                INITIAL_BASE_HP,
                INITIAL_BASE_SPEED,
                INITIAL_BOOSTER_BONUS_SEC
        );
        userStatRepository.save(initialUserStat);
    }

    @Transactional(readOnly = true)
    public DuplicateCheckResponse checkLoginIdDuplicate(String loginId) {
        boolean exists = userRepository.existsByLoginId(loginId);
        String message = exists ? ErrorCode.DUPLICATE_LOGIN_ID.getMessage() : AVAILABLE_LOGIN_ID_MESSAGE;
        return DuplicateCheckResponse.of(!exists, message);
    }

    @Transactional(readOnly = true)
    public DuplicateCheckResponse checkNicknameDuplicate(String nickname) {
        boolean exists = userRepository.existsByNickname(nickname);
        String message = exists ? ErrorCode.DUPLICATE_NICKNAME.getMessage() : AVAILABLE_NICKNAME_MESSAGE;
        return DuplicateCheckResponse.of(!exists, message);
    }

    @Transactional(readOnly = true)
    public User login(LoginRequest request) {
        User user = userRepository.findByLoginId(request.getLoginId())
                .orElseThrow(() -> new CustomException(ErrorCode.LOGIN_FAILED));

        if (!passwordEncoder.matches(request.getPassword(), user.getPassword())) {
            throw new CustomException(ErrorCode.LOGIN_FAILED);
        }
        return user;
    }
}
