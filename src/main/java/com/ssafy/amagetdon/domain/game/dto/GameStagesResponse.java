package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

import java.util.List;

@Getter
@Builder
@AllArgsConstructor
public class GameStagesResponse {

    private int worldId;
    private String worldName;
    private String currentStageId;
    private List<StageSummaryResponse> stages;
}