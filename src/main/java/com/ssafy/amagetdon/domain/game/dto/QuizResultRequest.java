package com.ssafy.amagetdon.domain.game.dto;

import lombok.Getter;
import lombok.NoArgsConstructor;

@Getter
@NoArgsConstructor
public class QuizResultRequest {

    private String stageId;
    private Long quizId;
    private Integer selectedAnswer;
    private Double responseTime;
    private boolean timeOver;
    private Long runId;
}