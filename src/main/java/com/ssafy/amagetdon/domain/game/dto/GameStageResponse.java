package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

@Getter
@Builder
@AllArgsConstructor
public class GameStageResponse {

    private String stageId;
    private String stageCode;
    private String stageName;
    private int targetDistance;
    private int stageOrder;
    private boolean unlocked;
}