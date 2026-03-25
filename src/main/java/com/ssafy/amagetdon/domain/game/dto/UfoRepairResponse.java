package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

@Getter
@Builder
@AllArgsConstructor
public class UfoRepairResponse {

    private String partName;
    private int repairCost;
    private int remainingCoin;
    private String effect;
    private String message;
}