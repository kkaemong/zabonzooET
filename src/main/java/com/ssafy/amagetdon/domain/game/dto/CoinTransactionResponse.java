package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Getter;

import java.time.LocalDateTime;

@Getter
@AllArgsConstructor
public class CoinTransactionResponse {

    private String txType;
    private int amount;
    private int balanceAfter;
    private String description;
    private LocalDateTime createdAt;
}