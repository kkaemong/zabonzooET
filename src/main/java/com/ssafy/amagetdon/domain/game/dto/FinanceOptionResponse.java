package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

import java.util.List;

@Getter
@Builder
@AllArgsConstructor
public class FinanceOptionResponse {

    private String optionType;
    private String title;
    private String description;
    private List<FinanceSubOptionResponse> subOptions;
}