package com.ssafy.amagetdon.domain.game.repository;

import com.ssafy.amagetdon.domain.game.entity.RunSession;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface RunSessionRepository extends JpaRepository<RunSession, Long> {

    List<RunSession> findByUserIdOrderByStartedAtDesc(Long userId);
}