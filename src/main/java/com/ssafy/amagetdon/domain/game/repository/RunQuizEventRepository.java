package com.ssafy.amagetdon.domain.game.repository;

import com.ssafy.amagetdon.domain.game.entity.RunQuizEvent;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface RunQuizEventRepository extends JpaRepository<RunQuizEvent, Long> {

    List<RunQuizEvent> findByRunId(Long runId);
}