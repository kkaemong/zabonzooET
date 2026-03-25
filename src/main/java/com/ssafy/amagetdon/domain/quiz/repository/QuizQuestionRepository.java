package com.ssafy.amagetdon.domain.quiz.repository;

import com.ssafy.amagetdon.domain.quiz.entity.QuizQuestion;
import org.springframework.data.jpa.repository.JpaRepository;

public interface QuizQuestionRepository extends JpaRepository<QuizQuestion, Long> {
}