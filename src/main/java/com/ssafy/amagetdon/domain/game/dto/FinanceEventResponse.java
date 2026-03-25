package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

@Getter
@Builder
@AllArgsConstructor
public class FinanceEventResponse {

    private String stageId;
    private String choice;
    private int baseCoin;
    private int changeCoin;
    private int finalCoin;
    private String resultType;
    private String detailResult;
    private String aiFeedback;
    private String nextEra;
    private boolean finalClear;
}