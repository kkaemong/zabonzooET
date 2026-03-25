package com.ssafy.amagetdon.domain.coin.entity;

import com.ssafy.amagetdon.domain.user.entity.User;
import com.ssafy.amagetdon.domain.game.entity.RunSession;
import jakarta.persistence.*;
import java.time.LocalDateTime;
import lombok.*;

@Entity
@Getter
@NoArgsConstructor(access = AccessLevel.PROTECTED)
@AllArgsConstructor
@Builder
public class CoinTransaction {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long coinTxId;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "user_id")
    private User user;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "run_id")
    private RunSession runSession;

    @Column(nullable = false)
    private String txType; // RUN_REWARD, SHOP_PURCHASE 등

    @Column(nullable = false)
    private Integer amount; // +100, -500

    @Column(nullable = false)
    private Integer balanceAfter; // 반영 후 잔액

    private String description;

    private LocalDateTime createdAt;

    @PrePersist
    protected void onCreate() {
        this.createdAt = LocalDateTime.now();
    }
}