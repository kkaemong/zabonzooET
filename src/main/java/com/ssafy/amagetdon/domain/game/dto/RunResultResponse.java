package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

@Getter
@Builder
@AllArgsConstructor
public class RunResultResponse {

    private Long runId;
    private boolean cleared;
    private int rewardCoin;
    private int remainingTotalCoin;
    private String currentEra;
    private String nextStep;
    private boolean financeEventAvailable;
    private String nextEra;
}