package com.ssafy.amagetdon.domain.game.dto;

import lombok.Getter;
import lombok.NoArgsConstructor;

import java.util.List;

@Getter
@NoArgsConstructor
public class RunResultRequest {

    private String stageId;
    private Long runId;
    private int playTime;
    private int distance;
    private int collectedCoin;
    private int remainingHp;
    private boolean quizCorrect;
    private String financeChoice;
    private boolean cleared;
    private List<String> usedItems;
}