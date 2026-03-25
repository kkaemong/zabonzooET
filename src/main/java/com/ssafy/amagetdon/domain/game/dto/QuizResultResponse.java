package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

@Getter
@Builder
@AllArgsConstructor
public class QuizResultResponse {

    private boolean correct;
    private String effectType;
    private double speedMultiplier;
    private int hpChange;
    private String monsterAction;
    private String message;
    private int currentLife;
    private int maxLife;
    private int quizCount;
}