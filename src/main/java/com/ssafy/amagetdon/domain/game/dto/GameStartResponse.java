package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

@Getter
@Builder
@AllArgsConstructor
public class GameStartResponse {

    private Long runId;
    private Long stageId;
    private String stageCode;
    private String stageName;
    private int targetDistance;
    private int life;
    private int maxLife;
    private String status;
}