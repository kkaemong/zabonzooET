package com.ssafy.amagetdon.domain.game.controller;

import com.ssafy.amagetdon.domain.game.dto.RankingResponse;
import com.ssafy.amagetdon.domain.game.service.GameService;
import lombok.RequiredArgsConstructor;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequiredArgsConstructor
public class RankingController {

    private final GameService gameService;

    @GetMapping("/api/rankings")
    public RankingResponse getRankings() {
        return gameService.getRankings();
    }
}