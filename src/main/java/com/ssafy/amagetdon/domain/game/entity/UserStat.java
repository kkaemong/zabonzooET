package com.ssafy.amagetdon.domain.game.entity;

import com.ssafy.amagetdon.common.response.BaseTimeEntity;
import com.ssafy.amagetdon.domain.user.entity.User;
import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.FetchType;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.ManyToOne;
import jakarta.persistence.Table;
import lombok.AccessLevel;
import lombok.Getter;
import lombok.NoArgsConstructor;

@Getter
@Entity
@Table(name = "user_stat")
@NoArgsConstructor(access = AccessLevel.PROTECTED)
public class UserStat extends BaseTimeEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long userStatId;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "user_id", nullable = false)
    private User user;

    @Column(name = "coin_balance", nullable = false)
    private Integer coinBalance;

    @Column(name = "base_hp", nullable = false)
    private Integer baseHp;

    @Column(name = "base_speed", nullable = false)
    private Double baseSpeed;

    @Column(name = "booster_bonus_sec", nullable = false)
    private Integer boosterBonusSec;

    @Column(name = "current_stage_code")
    private String currentStageCode;

    @Column(name = "unlocked_stage_order")
    private Integer unlockedStageOrder;

    @Column(name = "final_cleared")
    private Boolean finalCleared;

    public void addCoin(int coin) {
        this.coinBalance += coin;
    }

    public void useCoin(int amount) {
        if (this.coinBalance < amount) {
            throw new IllegalArgumentException("코인이 부족합니다.");
        }
        this.coinBalance -= amount;
    }

    public void addBoosterBonusSec(int sec) {
        this.boosterBonusSec += sec;
    }

    public void addBaseSpeed(double speed) {
        this.baseSpeed += speed;
    }

    public void addBaseHp(int hp) {
        this.baseHp += hp;
    }

    public void updateCurrentStageCode(String currentStageCode) {
        this.currentStageCode = currentStageCode;
    }

    public void updateUnlockedStageOrder(Integer unlockedStageOrder) {
        this.unlockedStageOrder = unlockedStageOrder;
    }

    public void markFinalCleared() {
        this.finalCleared = true;
    }

    public UserStat(
            User user,
            Integer coinBalance,
            Integer baseHp,
            Double baseSpeed,
            Integer boosterBonusSec
    ) {
        this.user = user;
        this.coinBalance = coinBalance;
        this.baseHp = baseHp;
        this.baseSpeed = baseSpeed;
        this.boosterBonusSec = boosterBonusSec;
    }
}