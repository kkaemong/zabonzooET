package com.ssafy.amagetdon.domain.quiz.entity;

import jakarta.persistence.*;
import lombok.AccessLevel;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;

@Getter
@Entity
@Table(name = "quiz_question")
@NoArgsConstructor(access = AccessLevel.PROTECTED)
public class QuizQuestion {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "quiz_question_id")
    private Long id;

    @Column(name = "question_text", nullable = false, columnDefinition = "TEXT")
    private String questionText;

    @Column(name = "difficulty", nullable = false, length = 20)
    private String difficulty;

    @Column(name = "time_limit_sec", nullable = false)
    private Integer timeLimitSec;

    @Column(name = "explanation", nullable = false, columnDefinition = "TEXT")
    private String explanation;

    @Column(name = "is_active", nullable = false)
    private Boolean isActive;

    @Builder
    public QuizQuestion(String questionText, String difficulty, Integer timeLimitSec,
                        String explanation, Boolean isActive) {
        this.questionText = questionText;
        this.difficulty = difficulty;
        this.timeLimitSec = timeLimitSec;
        this.explanation = explanation;
        this.isActive = isActive;
    }
}