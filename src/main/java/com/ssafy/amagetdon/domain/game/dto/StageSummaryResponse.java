package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

@Getter
@Builder
@AllArgsConstructor
public class StageSummaryResponse {

    private String stageId;
    private String stageName;
    private boolean locked;
    private boolean cleared;
    private int bestCoin;
    private String recommendedLevel;
    private int targetDistance;
    private int starCount;
}