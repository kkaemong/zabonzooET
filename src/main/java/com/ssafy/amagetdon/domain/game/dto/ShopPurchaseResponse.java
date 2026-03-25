package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

@Getter
@Builder
@AllArgsConstructor
public class ShopPurchaseResponse {

    private Long itemId;
    private String itemName;
    private int purchasedQuantity;
    private int usedCoin;
    private int remainingCoin;
}