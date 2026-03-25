package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

@Getter
@Builder
@AllArgsConstructor
public class GameProfileResponse {

    private String nickname;
    private int coin;
    private int totalCoin;
    private int hp;
    private String currentStage;
}