package com.ssafy.amagetdon.domain.game.repository;

import com.ssafy.amagetdon.domain.game.entity.Stage;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;

public interface StageRepository extends JpaRepository<Stage, Long> {

    List<Stage> findByIsActiveTrueOrderByStageOrderAsc();

    Optional<Stage> findByStageCode(String stageCode);
}