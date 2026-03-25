package com.ssafy.amagetdon.domain.quiz.repository;

import com.ssafy.amagetdon.domain.quiz.entity.QuizChoice;
import org.springframework.data.jpa.repository.JpaRepository;

public interface QuizChoiceRepository extends JpaRepository<QuizChoice, Long> {
}