package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Getter;

import java.time.LocalDateTime;

@Getter
@AllArgsConstructor
public class RunHistoryResponse {

    private Long runId;
    private Long stageId;
    private Integer distance;
    private Integer collectedCoin;
    private boolean cleared;
    private LocalDateTime startedAt;
}