package com.ssafy.amagetdon.domain.game.repository;

import com.ssafy.amagetdon.domain.game.entity.UserStat;
import java.util.Optional;
import org.springframework.data.jpa.repository.JpaRepository;

public interface UserStatRepository extends JpaRepository<UserStat, Long> {

    Optional<UserStat> findByUser_UserId(Long userId);
}