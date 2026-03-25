package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

@Getter
@Builder
@AllArgsConstructor
public class FinanceSubOptionResponse {

    private String code;
    private String name;
    private String description;
}