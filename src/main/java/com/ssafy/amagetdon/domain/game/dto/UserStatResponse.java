package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Getter;

@Getter
@AllArgsConstructor
public class UserStatResponse {

    private Integer coinBalance;
    private Integer baseHp;
    private Double baseSpeed;
    private Integer boosterBonusSec;
}