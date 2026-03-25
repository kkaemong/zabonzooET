package com.ssafy.amagetdon.domain.game.entity;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.AccessLevel;
import lombok.Getter;
import lombok.NoArgsConstructor;

import java.time.LocalDateTime;

@Entity
@Table(name = "run_session")
@Getter
@NoArgsConstructor(access = AccessLevel.PROTECTED)
public class RunSession {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "run_id")
    private Long runId;

    @Column(name = "user_id", nullable = false)
    private Long userId;

    @Column(name = "stage_id", nullable = false)
    private Long stageId;

    @Column(name = "start_hp", nullable = false)
    private Integer startHp;

    @Column(name = "end_hp")
    private Integer endHp;

    @Column(name = "distance_reached")
    private Integer distanceReached;

    @Column(name = "collected_coin")
    private Integer collectedCoin;

    @Column(name = "life", nullable = false)
    private Integer life;

    @Column(name = "quiz_count", nullable = false)
    private Integer quizCount;

    @Column(name = "status", nullable = false)
    private String status;

    @Column(name = "started_at", nullable = false)
    private LocalDateTime startedAt;

    @Column(name = "ended_at")
    private LocalDateTime endedAt;

    public RunSession(Long userId, Long stageId, Integer startHp, String status, LocalDateTime startedAt) {
        this.userId = userId;
        this.stageId = stageId;
        this.startHp = startHp;
        this.status = status;
        this.startedAt = startedAt;
        this.life = 3;
        this.quizCount = 0;
    }

    public void finishRun(Integer endHp, Integer distanceReached, Integer collectedCoin) {
        this.endHp = endHp;
        this.distanceReached = distanceReached;
        this.collectedCoin = collectedCoin;
        this.status = "FINISHED";
        this.endedAt = LocalDateTime.now();
    }

    public void decreaseLife() {
        if (this.life > 0) {
            this.life -= 1;
        }
    }

    public void increaseLife() {
        if (this.life < 3) {
            this.life += 1;
        }
    }

    public void increaseQuizCount() {
        this.quizCount += 1;
    }

    public boolean isGameOver() {
        return this.life <= 0;
    }

    public void gameOver() {
        this.status = "GAME_OVER";
        this.endedAt = LocalDateTime.now();
    }
}