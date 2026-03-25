package com.ssafy.amagetdon.domain.game.dto;

import lombok.Getter;
import lombok.NoArgsConstructor;

@Getter
@NoArgsConstructor
public class FinanceEventRequest {

    private String stageId;
    private int baseCoin;
    private String choice;
    private String subOptionCode;
}