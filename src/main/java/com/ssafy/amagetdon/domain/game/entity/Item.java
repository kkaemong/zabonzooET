package com.ssafy.amagetdon.domain.game.entity;

import jakarta.persistence.*;
import lombok.AccessLevel;
import lombok.Getter;
import lombok.NoArgsConstructor;

@Entity
@Getter
@NoArgsConstructor(access = AccessLevel.PROTECTED)
public class Item {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long itemId;

    @Column(nullable = false, unique = true)
    private String itemName;

    @Column(nullable = false)
    private Integer price;

    @Column(nullable = false)
    private String description;

    @Column(nullable = false)
    private Boolean isActive;

    public Item(String itemName, Integer price, String description, Boolean isActive) {
        this.itemName = itemName;
        this.price = price;
        this.description = description;
        this.isActive = isActive;
    }
}