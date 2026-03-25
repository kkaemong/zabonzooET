package com.ssafy.amagetdon.domain.game.controller;

import com.ssafy.amagetdon.common.response.ApiResponse;
import com.ssafy.amagetdon.domain.coin.service.CoinTransactionService;
import com.ssafy.amagetdon.domain.game.dto.CoinTransactionResponse;
import com.ssafy.amagetdon.domain.game.dto.FinanceEventRequest;
import com.ssafy.amagetdon.domain.game.dto.FinanceEventResponse;
import com.ssafy.amagetdon.domain.game.dto.FinanceOptionsResponse;
import com.ssafy.amagetdon.domain.game.dto.GameInventoryResponse;
import com.ssafy.amagetdon.domain.game.dto.GameProfileResponse;
import com.ssafy.amagetdon.domain.game.dto.GameShopResponse;
import com.ssafy.amagetdon.domain.game.dto.GameStageResponse;
import com.ssafy.amagetdon.domain.game.dto.GameStagesResponse;
import com.ssafy.amagetdon.domain.game.dto.GameStartRequest;
import com.ssafy.amagetdon.domain.game.dto.GameStartResponse;
import com.ssafy.amagetdon.domain.game.dto.QuizResultRequest;
import com.ssafy.amagetdon.domain.game.dto.QuizResultResponse;
import com.ssafy.amagetdon.domain.game.dto.RunHistoryResponse;
import com.ssafy.amagetdon.domain.game.dto.RunResultRequest;
import com.ssafy.amagetdon.domain.game.dto.RunResultResponse;
import com.ssafy.amagetdon.domain.game.dto.ShopPurchaseRequest;
import com.ssafy.amagetdon.domain.game.dto.ShopPurchaseResponse;
import com.ssafy.amagetdon.domain.game.dto.UfoRepairRequest;
import com.ssafy.amagetdon.domain.game.dto.UfoRepairResponse;
import com.ssafy.amagetdon.domain.game.dto.UserStatResponse;
import com.ssafy.amagetdon.domain.game.service.GameService;
import java.util.List;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/game")
@RequiredArgsConstructor
public class GameController {

    private final GameService gameService;
    private final CoinTransactionService coinTransactionService;

    @PostMapping("/start")
    public GameStartResponse startGame(@RequestBody GameStartRequest request) {
        return gameService.startGame(request.getStageCode());
    }

    @GetMapping("/profile")
    public GameProfileResponse getGameProfile() {
        return gameService.getGameProfile();
    }

    @GetMapping("/inventory")
    public GameInventoryResponse getInventory(@RequestParam Long userId) {
        return gameService.getInventory(userId);
    }

    @GetMapping("/shop")
    public GameShopResponse getShopItems() {
        return gameService.getShopItems();
    }

    @PostMapping("/shop/purchase")
    public ShopPurchaseResponse purchaseItem(@RequestBody ShopPurchaseRequest request) {
        return gameService.purchaseItem(request);
    }

    @GetMapping("/stages")
    public ResponseEntity<GameStagesResponse> getStages() {
        return ResponseEntity.ok(gameService.getStages());
    }

    @GetMapping("/stage")
    public ResponseEntity<GameStageResponse> getStage(@RequestParam String stageCode) {
        return ResponseEntity.ok(gameService.getStage(stageCode));
    }

    @PostMapping("/quiz-result")
    public QuizResultResponse submitQuizResult(@RequestBody QuizResultRequest request) {
        return gameService.submitQuizResult(request);
    }

    @PostMapping("/finance-event")
    public FinanceEventResponse processFinanceEvent(
            @RequestBody FinanceEventRequest request,
            @RequestParam Long userId
    ) {
        return gameService.processFinanceEvent(request, userId);
    }

    @PostMapping("/run-result")
    public RunResultResponse submitRunResult(
            @RequestBody RunResultRequest request,
            @RequestParam Long userId
    ) {
        return gameService.submitRunResult(request, userId);
    }

    @GetMapping("/run-history")
    public List<RunHistoryResponse> getRunHistory(@RequestParam Long userId) {
        return gameService.getRunHistory(userId);
    }

    @PostMapping("/ufo-repair")
    public UfoRepairResponse repairUfo(
            @RequestParam Long userId,
            @RequestBody UfoRepairRequest request
    ) {
        return gameService.repairUfo(request, userId);
    }

    @GetMapping("/finance-options")
    public FinanceOptionsResponse getFinanceOptions(@RequestParam String stageCode) {
        return gameService.getFinanceOptions(stageCode);
    }

    @GetMapping("/coin-transactions")
    public List<CoinTransactionResponse> getTransactions(@RequestParam Long userId) {
        return coinTransactionService.getTransactions(userId);
    }

    @GetMapping("/user-stat")
    public ResponseEntity<UserStatResponse> getUserStat(@RequestParam Long userId) {
        return ResponseEntity.ok(gameService.getUserStat(userId));
    }
}