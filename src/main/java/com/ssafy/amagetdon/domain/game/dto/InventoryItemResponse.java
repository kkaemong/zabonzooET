package com.ssafy.amagetdon.domain.game.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;

@Getter
@Builder
@AllArgsConstructor
public class InventoryItemResponse {

    private Long itemId;
    private String itemName;
    private int quantity;
    private String description;
}