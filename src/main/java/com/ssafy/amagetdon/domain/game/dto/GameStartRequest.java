package com.ssafy.amagetdon.domain.game.dto;

public class GameStartRequest {

    private String stageCode;

    public GameStartRequest() {
    }

    public GameStartRequest(String stageCode) {
        this.stageCode = stageCode;
    }

    public String getStageCode() {
        return stageCode;
    }

    public void setStageCode(String stageCode) {
        this.stageCode = stageCode;
    }
}