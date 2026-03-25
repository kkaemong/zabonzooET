package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

@Getter
@Builder
@AllArgsConstructor
public class ShopItemResponse {

    private Long itemId;
    private String itemName;
    private int price;
    private String description;
    private boolean purchasable;
}