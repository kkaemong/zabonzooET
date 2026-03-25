package com.ssafy.amagetdon.domain.game.entity;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.AccessLevel;
import lombok.Getter;
import lombok.NoArgsConstructor;

@Entity
@Table(name = "stage")
@Getter
@NoArgsConstructor(access = AccessLevel.PROTECTED)
public class Stage {

    @Id
    @Column(name = "stage_id")
    private Long stageId;

    @Column(name = "world_no", nullable = false)
    private Integer worldNo;

    @Column(name = "stage_no", nullable = false)
    private Integer stageNo;

    @Column(name = "stage_code", nullable = false)
    private String stageCode;

    @Column(name = "stage_name", nullable = false)
    private String stageName;

    @Column(name = "stage_order", nullable = false)
    private Integer stageOrder;

    @Column(name = "stage_length", nullable = false)
    private Integer stageLength;

    @Column(name = "base_reward_coin", nullable = false)
    private Integer baseRewardCoin;

    @Column(name = "is_active", nullable = false)
    private Boolean isActive;
}