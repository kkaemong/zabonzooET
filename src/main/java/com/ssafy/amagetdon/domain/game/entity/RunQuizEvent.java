package com.ssafy.amagetdon.domain.game.entity;

import jakarta.persistence.*;
import lombok.AccessLevel;
import lombok.Getter;
import lombok.NoArgsConstructor;

import java.time.LocalDateTime;

@Entity
@Table(name = "run_quiz_event")
@Getter
@NoArgsConstructor(access = AccessLevel.PROTECTED)
public class RunQuizEvent {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "run_quiz_event_id")
    private Long runQuizEventId;

    @Column(name = "run_id", nullable = false)
    private Long runId;

    @Column(name = "quiz_id", nullable = false)
    private Long quizId;

    @Column(name = "selected_answer", nullable = false)
    private Integer selectedAnswer;

    @Column(name = "correct", nullable = false)
    private Boolean correct;

    @Column(name = "response_time")
    private Double responseTime;

    @Column(name = "time_over", nullable = false)
    private Boolean timeOver;

    @Column(name = "hp_change", nullable = false)
    private Integer hpChange;

    @Column(name = "created_at", nullable = false)
    private LocalDateTime createdAt;

    public RunQuizEvent(
            Long runId,
            Long quizId,
            Integer selectedAnswer,
            Boolean correct,
            Double responseTime,
            Boolean timeOver,
            Integer hpChange
    ) {
        this.runId = runId;
        this.quizId = quizId;
        this.selectedAnswer = selectedAnswer;
        this.correct = correct;
        this.responseTime = responseTime;
        this.timeOver = timeOver;
        this.hpChange = hpChange;
        this.createdAt = LocalDateTime.now();
    }
}