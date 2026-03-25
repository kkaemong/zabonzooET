package com.ssafy.amagetdon.domain.game.dto;

import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Getter
@Setter
@NoArgsConstructor
public class ShopPurchaseRequest {

    private Long userId;
    private Long itemId;
    private int quantity;
}