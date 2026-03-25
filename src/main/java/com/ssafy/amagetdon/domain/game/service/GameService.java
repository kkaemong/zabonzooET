package com.ssafy.amagetdon.domain.game.service;

import com.ssafy.amagetdon.domain.coin.service.CoinService;
import com.ssafy.amagetdon.domain.coin.service.CoinTransactionService;
import com.ssafy.amagetdon.domain.game.dto.FinanceEventRequest;
import com.ssafy.amagetdon.domain.game.dto.FinanceEventResponse;
import com.ssafy.amagetdon.domain.game.dto.FinanceOptionResponse;
import com.ssafy.amagetdon.domain.game.dto.FinanceOptionsResponse;
import com.ssafy.amagetdon.domain.game.dto.FinanceSubOptionResponse;
import com.ssafy.amagetdon.domain.game.dto.GameInventoryResponse;
import com.ssafy.amagetdon.domain.game.dto.GameProfileResponse;
import com.ssafy.amagetdon.domain.game.dto.GameShopResponse;
import com.ssafy.amagetdon.domain.game.dto.GameStageResponse;
import com.ssafy.amagetdon.domain.game.dto.GameStagesResponse;
import com.ssafy.amagetdon.domain.game.dto.GameStartResponse;
import com.ssafy.amagetdon.domain.game.dto.InventoryItemResponse;
import com.ssafy.amagetdon.domain.game.dto.QuizResultRequest;
import com.ssafy.amagetdon.domain.game.dto.QuizResultResponse;
import com.ssafy.amagetdon.domain.game.dto.RankingItemResponse;
import com.ssafy.amagetdon.domain.game.dto.RankingResponse;
import com.ssafy.amagetdon.domain.game.dto.RunHistoryResponse;
import com.ssafy.amagetdon.domain.game.dto.RunResultRequest;
import com.ssafy.amagetdon.domain.game.dto.RunResultResponse;
import com.ssafy.amagetdon.domain.game.dto.ShopItemResponse;
import com.ssafy.amagetdon.domain.game.dto.ShopPurchaseRequest;
import com.ssafy.amagetdon.domain.game.dto.ShopPurchaseResponse;
import com.ssafy.amagetdon.domain.game.dto.StageSummaryResponse;
import com.ssafy.amagetdon.domain.game.dto.UfoRepairRequest;
import com.ssafy.amagetdon.domain.game.dto.UfoRepairResponse;
import com.ssafy.amagetdon.domain.game.dto.UserStatResponse;
import com.ssafy.amagetdon.domain.game.entity.Inventory;
import com.ssafy.amagetdon.domain.game.entity.Item;
import com.ssafy.amagetdon.domain.game.entity.RunSession;
import com.ssafy.amagetdon.domain.game.entity.Stage;
import com.ssafy.amagetdon.domain.game.entity.UserStat;
import com.ssafy.amagetdon.domain.game.repository.InventoryRepository;
import com.ssafy.amagetdon.domain.game.repository.ItemRepository;
import com.ssafy.amagetdon.domain.game.repository.RunSessionRepository;
import com.ssafy.amagetdon.domain.game.repository.StageRepository;
import com.ssafy.amagetdon.domain.game.repository.UserStatRepository;
import com.ssafy.amagetdon.domain.user.entity.User;
import com.ssafy.amagetdon.domain.quiz.service.QuizService;
import com.ssafy.amagetdon.domain.game.repository.RunQuizEventRepository;
import com.ssafy.amagetdon.domain.game.entity.RunQuizEvent;
import java.time.LocalDateTime;
import java.util.concurrent.ThreadLocalRandom;
import java.util.ArrayList;
import java.util.List;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
@RequiredArgsConstructor
public class GameService {

    private final UserStatRepository userStatRepository;
    private final StageRepository stageRepository;
    private final RunSessionRepository runSessionRepository;
    private final CoinTransactionService coinTransactionService;
    private final CoinService coinService;
    private final ItemRepository itemRepository;
    private final InventoryRepository inventoryRepository;
    private final GmsService gmsService;
    private final QuizService quizService;
    private final RunQuizEventRepository runQuizEventRepository;

    public GameStartResponse startGame(String stageCode) {
        validateStage(stageCode);

        Stage selectedStage = stageRepository.findByStageCode(stageCode)
                .orElseThrow(() -> new IllegalArgumentException("존재하지 않는 스테이지입니다."));

        RunSession runSession = new RunSession(
                1L,
                selectedStage.getStageId(),
                100,
                "STARTED",
                LocalDateTime.now()
        );

        runSessionRepository.save(runSession);

        return GameStartResponse.builder()
                .runId(runSession.getRunId())
                .stageId(selectedStage.getStageId())
                .stageCode(selectedStage.getStageCode())
                .stageName(selectedStage.getStageName())
                .targetDistance(selectedStage.getStageLength())
                .life(3)
                .maxLife(3)
                .status("STARTED")
                .build();
    }

    public GameProfileResponse getGameProfile() {
        return GameProfileResponse.builder()
                .nickname("player1")
                .coin(1200)
                .totalCoin(12000)
                .hp(100)
                .currentStage("2-1")
                .build();
    }

    public UserStatResponse getUserStat(Long userId) {
        UserStat userStat = userStatRepository.findByUser_UserId(userId)
                .orElseThrow(() -> new IllegalArgumentException("유저 게임 정보가 없습니다."));

        return new UserStatResponse(
                userStat.getCoinBalance(),
                userStat.getBaseHp(),
                userStat.getBaseSpeed(),
                userStat.getBoosterBonusSec()
        );
    }

    public GameInventoryResponse getInventory(Long userId) {
        List<Inventory> inventoryList = inventoryRepository.findByUser_UserId(userId);
        List<InventoryItemResponse> items = new ArrayList<>();

        for (Inventory inventory : inventoryList) {
            items.add(
                    InventoryItemResponse.builder()
                            .itemId(inventory.getItem().getItemId())
                            .itemName(inventory.getItem().getItemName())
                            .quantity(inventory.getQuantity())
                            .description(inventory.getItem().getDescription())
                            .build()
            );
        }

        return GameInventoryResponse.builder()
                .items(items)
                .build();
    }

    public GameShopResponse getShopItems() {
        List<Item> itemList = itemRepository.findByIsActiveTrue();
        List<ShopItemResponse> items = new ArrayList<>();

        for (Item item : itemList) {
            items.add(
                    ShopItemResponse.builder()
                            .itemId(item.getItemId())
                            .itemName(item.getItemName())
                            .price(item.getPrice())
                            .description(item.getDescription())
                            .purchasable(true)
                            .build()
            );
        }

        return GameShopResponse.builder()
                .items(items)
                .build();
    }

    public ShopPurchaseResponse purchaseItem(ShopPurchaseRequest request) {
        if (request.getUserId() == null) {
            throw new IllegalArgumentException("userId는 필수입니다.");
        }

        if (request.getItemId() == null) {
            throw new IllegalArgumentException("itemId는 필수입니다.");
        }

        if (request.getQuantity() <= 0) {
            throw new IllegalArgumentException("quantity는 1 이상이어야 합니다.");
        }

        UserStat userStat = userStatRepository.findByUser_UserId(request.getUserId())
                .orElseThrow(() -> new IllegalArgumentException("유저 게임 정보가 없습니다."));

        User user = userStat.getUser();

        Item item = itemRepository.findById(request.getItemId())
                .orElseThrow(() -> new IllegalArgumentException("존재하지 않는 아이템입니다."));

        if (!item.getIsActive()) {
            throw new IllegalArgumentException("비활성화된 아이템입니다.");
        }

        int usedCoin = item.getPrice() * request.getQuantity();

        coinService.deductCoin(
                request.getUserId(),
                usedCoin,
                "SHOP_PURCHASE",
                "상점 아이템 구매"
        );

        Inventory inventory = inventoryRepository.findByUserAndItem(user, item)
                .orElse(null);

        if (inventory == null) {
            inventory = new Inventory(user, item, request.getQuantity());
        } else {
            inventory.addQuantity(request.getQuantity());
        }

        inventoryRepository.save(inventory);

        return ShopPurchaseResponse.builder()
                .itemId(item.getItemId())
                .itemName(item.getItemName())
                .purchasedQuantity(request.getQuantity())
                .usedCoin(usedCoin)
                .remainingCoin(userStat.getCoinBalance())
                .build();
    }

    public GameStagesResponse getStages() {
        List<Stage> stageList = stageRepository.findByIsActiveTrueOrderByStageOrderAsc();
        List<StageSummaryResponse> stages = new ArrayList<>();

        for (Stage stage : stageList) {
            stages.add(
                    StageSummaryResponse.builder()
                            .stageId(stage.getStageCode())
                            .stageName(stage.getStageName())
                            .locked(false)
                            .cleared(false)
                            .bestCoin(0)
                            .recommendedLevel("NORMAL")
                            .targetDistance(stage.getStageLength())
                            .starCount(0)
                            .build()
            );
        }

        Integer worldId = null;
        String worldName = null;
        String currentStageId = null;

        if (!stageList.isEmpty()) {
            worldId = stageList.get(0).getWorldNo();
            worldName = "Era Mode";
            currentStageId = stageList.get(0).getStageCode();
        }

        return GameStagesResponse.builder()
                .worldId(worldId)
                .worldName(worldName)
                .currentStageId(currentStageId)
                .stages(stages)
                .build();
    }

    public GameStageResponse getStage(String stageCode) {
        if (stageCode == null || stageCode.isBlank()) {
            throw new IllegalArgumentException("stageCode는 필수입니다.");
        }

        Stage stage = stageRepository.findByStageCode(stageCode)
                .orElseThrow(() -> new IllegalArgumentException("존재하지 않는 스테이지입니다."));

        return GameStageResponse.builder()
                .stageId(String.valueOf(stage.getStageId()))
                .stageCode(stage.getStageCode())
                .stageName(stage.getStageName())
                .targetDistance(stage.getStageLength())
                .stageOrder(stage.getStageOrder())
                .unlocked(true)
                .build();
    }

    @Transactional
    public QuizResultResponse submitQuizResult(QuizResultRequest request) {
        if (request == null) {
            throw new IllegalArgumentException("요청값이 없습니다.");
        }

        if (request.getRunId() == null) {
            throw new IllegalArgumentException("runId는 필수입니다.");
        }

        if (request.getQuizId() == null) {
            throw new IllegalArgumentException("quizId는 필수입니다.");
        }

        if (request.getSelectedAnswer() == null) {
            throw new IllegalArgumentException("selectedAnswer는 필수입니다.");
        }

        RunSession runSession = runSessionRepository.findById(request.getRunId())
                .orElseThrow(() -> new IllegalArgumentException("플레이 기록이 존재하지 않습니다."));

        boolean correct;
        int hpChange;
        String effectType;
        String message;

        if (request.isTimeOver()) {
            correct = false;
            runSession.decreaseLife();
            hpChange = -1;
            effectType = "DEBUFF";
            message = "시간 초과로 오답 처리되었습니다.";
        } else {
            correct = quizService.checkAnswer(
                    request.getQuizId(),
                    request.getSelectedAnswer()
            );

            if (correct) {
                runSession.increaseLife();
                hpChange = 1;
                effectType = "BUFF";
                message = "정답입니다! 체력이 증가했습니다.";
            } else {
                runSession.decreaseLife();
                hpChange = -1;
                effectType = "DEBUFF";
                message = "오답입니다... 체력이 감소했습니다.";
            }
        }

        runSession.increaseQuizCount();

        if (runSession.isGameOver()) {
            runSession.gameOver();
        }
        RunQuizEvent runQuizEvent = new RunQuizEvent(
                request.getRunId(),
                request.getQuizId(),
                request.getSelectedAnswer(),
                correct,
                request.getResponseTime(),
                request.isTimeOver(),
                hpChange
        );

        runQuizEventRepository.save(runQuizEvent);
        runSessionRepository.save(runSession);

        return QuizResultResponse.builder()
                .correct(correct)
                .effectType(effectType)
                .speedMultiplier(1.0)
                .hpChange(hpChange)
                .monsterAction("NONE")
                .message(message)
                .currentLife(runSession.getLife())
                .maxLife(3)
                .quizCount(runSession.getQuizCount())
                .build();
    }

    public FinanceEventResponse processFinanceEvent(FinanceEventRequest request, Long userId) {
        if (request == null) {
            throw new IllegalArgumentException("요청값이 없습니다.");
        }

        if (request.getStageId() == null || request.getStageId().isBlank()) {
            throw new IllegalArgumentException("stageId는 필수입니다.");
        }

        validateStage(request.getStageId());

        if (request.getBaseCoin() < 0) {
            throw new IllegalArgumentException("baseCoin은 0 이상이어야 합니다.");
        }

        if (request.getChoice() == null || request.getChoice().isBlank()) {
            throw new IllegalArgumentException("choice는 필수입니다.");
        }

        if (request.getSubOptionCode() == null || request.getSubOptionCode().isBlank()) {
            throw new IllegalArgumentException("subOptionCode는 필수입니다.");
        }

        FinanceResult financeResult = calculateFinanceResult(
                request.getStageId(),
                request.getChoice(),
                request.getSubOptionCode(),
                request.getBaseCoin()
        );

        int finalCoin;
        if (financeResult.getChangeCoin() >= 0) {
            finalCoin = coinService.addCoin(
                    userId,
                    financeResult.getChangeCoin(),
                    "FINANCE_EVENT",
                    "금융 이벤트 - " + financeResult.getDetailResult()
            );
        } else {
            finalCoin = coinService.deductCoin(
                    userId,
                    Math.abs(financeResult.getChangeCoin()),
                    "FINANCE_EVENT",
                    "금융 이벤트 - " + financeResult.getDetailResult()
            );
        }

        String nextEra = null;
        boolean finalClear = false;

        if (request.getStageId().equals("ERA_1980")) {
            nextEra = "ERA_2000";
        } else if (request.getStageId().equals("ERA_2000")) {
            nextEra = "ERA_2020";
        } else if (request.getStageId().equals("ERA_2020")) {
            finalClear = true;
        }
        UserStat userStat = userStatRepository.findByUser_UserId(userId)
                .orElseThrow(() -> new IllegalArgumentException("유저 게임 정보가 없습니다."));

        if (nextEra != null) {
            userStat.updateCurrentStageCode(nextEra);

            Integer currentUnlockedStageOrder = userStat.getUnlockedStageOrder();
            if (currentUnlockedStageOrder == null) {
                currentUnlockedStageOrder = 1;
            }

            userStat.updateUnlockedStageOrder(currentUnlockedStageOrder + 1);
        }

        if (finalClear) {
            userStat.markFinalCleared();
        }

        userStatRepository.save(userStat);

        String aiFeedback;

        try {
            aiFeedback = gmsService.generateFeedback(
                    request.getStageId(),
                    request.getChoice(),
                    request.getSubOptionCode(),
                    request.getBaseCoin(),
                    financeResult.getChangeCoin(),
                    financeResult.getResultType()
            );
        } catch (Exception e) {
            aiFeedback = financeResult.getAiFeedback();
        }

        if (aiFeedback == null || aiFeedback.isBlank()) {
            aiFeedback = financeResult.getAiFeedback();
        }

        return FinanceEventResponse.builder()
                .stageId(request.getStageId())
                .choice(request.getChoice())
                .baseCoin(request.getBaseCoin())
                .changeCoin(financeResult.getChangeCoin())
                .finalCoin(finalCoin)
                .resultType(financeResult.getResultType())
                .detailResult(financeResult.getDetailResult())
                .aiFeedback(aiFeedback)
                .nextEra(nextEra)
                .finalClear(finalClear)
                .build();

    }

    private FinanceResult calculateFinanceResult(String stageId, String choice, String subOptionCode, int baseCoin) {
        if (stageId.equals("ERA_1980")) {
            return calculate1980Finance(choice, subOptionCode, baseCoin);
        }

        if (stageId.equals("ERA_2000")) {
            return calculate2000Finance(choice, subOptionCode, baseCoin);
        }

        if (stageId.equals("ERA_2020")) {
            return calculate2020Finance(choice, subOptionCode, baseCoin);
        }

        throw new IllegalArgumentException("존재하지 않는 시대입니다.");
    }

    private FinanceResult calculate1980Finance(String choice, String subOptionCode, int baseCoin) {
        if (choice.equals("SAVING")) {
            if (subOptionCode.equals("BANK_DEPOSIT")) {
                int changeCoin = calculatePercent(baseCoin, 12);
                return new FinanceResult(
                        changeCoin,
                        "PROFIT",
                        "BANK_DEPOSIT",
                        "1980년대는 고금리 시대라 은행 예금만으로도 높은 이자를 기대할 수 있었습니다."
                );
            }

            if (subOptionCode.equals("PRIVATE_FINANCE")) {
                boolean success = chance(70);
                int changeCoin;
                String resultType;
                String feedback;

                if (success) {
                    changeCoin = calculatePercent(baseCoin, 15);
                    resultType = "PROFIT";
                    feedback = "사금융이나 계는 높은 수익을 기대할 수 있었지만 그만큼 위험도 큰 선택이었습니다.";
                } else {
                    changeCoin = -calculatePercent(baseCoin, 20);
                    resultType = "LOSS";
                    feedback = "고수익을 약속하는 사금융은 실패 시 손실 위험도 컸습니다.";
                }

                return new FinanceResult(changeCoin, resultType, "PRIVATE_FINANCE", feedback);
            }
        }

        if (choice.equals("INVESTMENT")) {
            if (subOptionCode.equals("GOLD")) {
                boolean highReturn = chance(30);
                int changeCoin;

                if (highReturn) {
                    changeCoin = calculatePercent(baseCoin, 15);
                } else {
                    changeCoin = calculatePercent(baseCoin, 5);
                }

                return new FinanceResult(
                        changeCoin,
                        "PROFIT",
                        "GOLD",
                        "금은 경제 불안 시 가치가 주목받는 안전자산으로 여겨졌습니다."
                );
            }

            if (subOptionCode.equals("LAND")) {
                boolean success = chance(60);
                int changeCoin;

                if (success) {
                    changeCoin = calculatePercent(baseCoin, 15);
                    return new FinanceResult(
                            changeCoin,
                            "PROFIT",
                            "LAND",
                            "개발 기대감이 있는 토지와 부동산은 장기적으로 자산 상승을 기대할 수 있었습니다."
                    );
                } else {
                    changeCoin = 0;
                    return new FinanceResult(
                            changeCoin,
                            "NEUTRAL",
                            "LAND",
                            "토지와 부동산은 항상 바로 수익이 나는 자산은 아니어서 보합으로 끝날 수도 있었습니다."
                    );
                }
            }

            if (subOptionCode.equals("HEAVY_INDUSTRY")) {
                boolean success = chance(50);
                int changeCoin;

                if (success) {
                    changeCoin = calculatePercent(baseCoin, 20);
                    return new FinanceResult(
                            changeCoin,
                            "PROFIT",
                            "HEAVY_INDUSTRY",
                            "1980년대 산업화 과정에서 중공업은 성장 기대가 큰 분야였습니다."
                    );
                } else {
                    changeCoin = -calculatePercent(baseCoin, 10);
                    return new FinanceResult(
                            changeCoin,
                            "LOSS",
                            "HEAVY_INDUSTRY",
                            "산업 투자는 성장 가능성이 크지만 경기와 정책에 따라 손실도 발생할 수 있었습니다."
                    );
                }
            }
        }

        if (choice.equals("LOTTO")) {
            return calculate1980Lotto(subOptionCode);
        }

        throw new IllegalArgumentException("존재하지 않는 금융 선택지입니다.");
    }

    private FinanceResult calculate2000Finance(String choice, String subOptionCode, int baseCoin) {
        if (choice.equals("SAVING")) {
            if (subOptionCode.equals("BANK_SAVINGS")) {
                int changeCoin = calculatePercent(baseCoin, 4);
                return new FinanceResult(
                        changeCoin,
                        "PROFIT",
                        "BANK_SAVINGS",
                        "2000년대에는 저축이 안정적이지만 예전보다 금리 메리트는 줄어들었습니다."
                );
            }

            if (subOptionCode.equals("BANK_INSTALLMENT")) {
                int changeCoin = calculatePercent(baseCoin, 6);
                return new FinanceResult(
                        changeCoin,
                        "PROFIT",
                        "BANK_INSTALLMENT",
                        "적금은 예금보다 조금 더 높은 수익을 기대할 수 있는 꾸준한 저축 방식이었습니다."
                );
            }
        }

        if (choice.equals("INVESTMENT")) {
            if (subOptionCode.equals("SAMSUNG")) {
                boolean success = chance(65);

                if (success) {
                    return new FinanceResult(
                            calculatePercent(baseCoin, 18),
                            "PROFIT",
                            "SAMSUNG",
                            "2000년대에는 대표 IT 기업 투자가 성장 수혜를 받을 가능성이 있었습니다."
                    );
                } else {
                    return new FinanceResult(
                            -calculatePercent(baseCoin, 8),
                            "LOSS",
                            "SAMSUNG",
                            "대표 기업 투자도 시장 상황에 따라 조정과 손실이 발생할 수 있었습니다."
                    );
                }
            }

            if (subOptionCode.equals("VENTURE_IT")) {
                boolean success = chance(40);

                if (success) {
                    return new FinanceResult(
                            calculatePercent(baseCoin, 35),
                            "PROFIT",
                            "VENTURE_IT",
                            "벤처와 IT주는 큰 수익을 줄 수 있었지만 변동성도 매우 컸습니다."
                    );
                } else {
                    return new FinanceResult(
                            -calculatePercent(baseCoin, 20),
                            "LOSS",
                            "VENTURE_IT",
                            "IT 버블 성격의 자산은 급등 뒤 급락 위험도 함께 있었습니다."
                    );
                }
            }

            if (subOptionCode.equals("APARTMENT")) {
                boolean success = chance(70);

                if (success) {
                    return new FinanceResult(
                            calculatePercent(baseCoin, 15),
                            "PROFIT",
                            "APARTMENT",
                            "2000년대 부동산은 비교적 강한 자산 상승 흐름을 보이던 시기였습니다."
                    );
                } else {
                    return new FinanceResult(
                            -calculatePercent(baseCoin, 5),
                            "LOSS",
                            "APARTMENT",
                            "부동산도 항상 오르기만 하는 자산은 아니어서 조정이 올 수 있었습니다."
                    );
                }
            }
        }

        if (choice.equals("LOTTO")) {
            return calculate2000Lotto(subOptionCode);
        }

        throw new IllegalArgumentException("존재하지 않는 금융 선택지입니다.");
    }

    private FinanceResult calculate2020Finance(String choice, String subOptionCode, int baseCoin) {
        if (choice.equals("SAVING")) {
            if (subOptionCode.equals("BANK_DEPOSIT")) {
                int changeCoin = calculatePercent(baseCoin, 2);
                return new FinanceResult(
                        changeCoin,
                        "PROFIT",
                        "BANK_DEPOSIT",
                        "2020년대 예금은 가장 안전하지만 수익률은 낮은 선택이었습니다."
                );
            }

            if (subOptionCode.equals("CMA")) {
                int changeCoin = calculatePercent(baseCoin, 3);
                return new FinanceResult(
                        changeCoin,
                        "PROFIT",
                        "CMA",
                        "CMA와 파킹형 상품은 유동성이 좋지만 큰 수익을 기대하기는 어려웠습니다."
                );
            }
        }

        if (choice.equals("INVESTMENT")) {
            if (subOptionCode.equals("ETF")) {
                boolean success = chance(70);

                if (success) {
                    return new FinanceResult(
                            calculatePercent(baseCoin, 10),
                            "PROFIT",
                            "ETF",
                            "ETF는 분산 투자 효과로 비교적 안정적인 투자 수단으로 인식되었습니다."
                    );
                } else {
                    return new FinanceResult(
                            -calculatePercent(baseCoin, 5),
                            "LOSS",
                            "ETF",
                            "ETF도 시장 전체가 흔들리면 손실이 발생할 수 있습니다."
                    );
                }
            }

            if (subOptionCode.equals("US_GROWTH")) {
                boolean success = chance(55);

                if (success) {
                    return new FinanceResult(
                            calculatePercent(baseCoin, 20),
                            "PROFIT",
                            "US_GROWTH",
                            "성장주는 높은 기대수익이 있지만 변동성도 함께 존재합니다."
                    );
                } else {
                    return new FinanceResult(
                            -calculatePercent(baseCoin, 12),
                            "LOSS",
                            "US_GROWTH",
                            "성장주는 기대가 꺾일 경우 큰 조정을 받을 수 있습니다."
                    );
                }
            }

            if (subOptionCode.equals("CRYPTO")) {
                boolean success = chance(30);

                if (success) {
                    return new FinanceResult(
                            calculatePercent(baseCoin, 50),
                            "PROFIT",
                            "CRYPTO",
                            "코인은 초고위험 자산이지만 단기간 큰 수익이 날 수도 있습니다."
                    );
                } else {
                    return new FinanceResult(
                            -calculatePercent(baseCoin, 30),
                            "LOSS",
                            "CRYPTO",
                            "코인은 변동성이 매우 커 큰 손실이 날 가능성도 높습니다."
                    );
                }
            }
        }

        if (choice.equals("LOTTO")) {
            return calculate2020Lotto(subOptionCode);
        }

        throw new IllegalArgumentException("존재하지 않는 금융 선택지입니다.");
    }

    private FinanceResult calculate1980Lotto(String subOptionCode) {
        if (subOptionCode.equals("JACKPOT")) {
            return new FinanceResult(10000, "PROFIT", "JACKPOT", "극히 드물지만 큰 행운이 찾아왔습니다.");
        }

        if (subOptionCode.equals("BUSINESS_CHANCE")) {
            return new FinanceResult(2000, "PROFIT", "BUSINESS_CHANCE", "뜻밖의 사업 기회를 잡아 자산이 늘었습니다.");
        }

        if (subOptionCode.equals("HOUSE_CHANCE")) {
            return new FinanceResult(1200, "PROFIT", "HOUSE_CHANCE", "좋은 기회를 잡아 자산 형성에 도움이 되었습니다.");
        }

        if (subOptionCode.equals("HOSPITAL_COST")) {
            return new FinanceResult(-1000, "LOSS", "HOSPITAL_COST", "예상치 못한 병원비 지출이 발생했습니다.");
        }

        if (subOptionCode.equals("EDUCATION_COST")) {
            return new FinanceResult(-800, "LOSS", "EDUCATION_COST", "교육비 지출로 자산이 줄었습니다.");
        }

        if (subOptionCode.equals("FAMILY_EVENT_COST")) {
            return new FinanceResult(-500, "LOSS", "FAMILY_EVENT_COST", "집안 행사비로 지출이 생겼습니다.");
        }

        throw new IllegalArgumentException("존재하지 않는 로또 세부 선택지입니다.");
    }

    private FinanceResult calculate2000Lotto(String subOptionCode) {
        if (subOptionCode.equals("LOTTO_FIRST")) {
            return new FinanceResult(8000, "PROFIT", "LOTTO_FIRST", "로또 1등 당첨으로 큰 보상을 얻었습니다.");
        }

        if (subOptionCode.equals("LOTTO_SMALL_WIN")) {
            return new FinanceResult(1000, "PROFIT", "LOTTO_SMALL_WIN", "소소한 당첨으로 기분 좋은 수익이 생겼습니다.");
        }

        if (subOptionCode.equals("JOB_SUCCESS")) {
            return new FinanceResult(1500, "PROFIT", "JOB_SUCCESS", "취업에 성공해 자산에 여유가 생겼습니다.");
        }

        if (subOptionCode.equals("CARD_DEBT")) {
            return new FinanceResult(-1200, "LOSS", "CARD_DEBT", "카드값 부담으로 지출이 커졌습니다.");
        }

        if (subOptionCode.equals("TUITION_COST")) {
            return new FinanceResult(-900, "LOSS", "TUITION_COST", "등록금과 교육비 부담이 생겼습니다.");
        }

        if (subOptionCode.equals("INVEST_LOSS")) {
            return new FinanceResult(-1500, "LOSS", "INVEST_LOSS", "투자 실패로 손실이 발생했습니다.");
        }

        throw new IllegalArgumentException("존재하지 않는 로또 세부 선택지입니다.");
    }

    private FinanceResult calculate2020Lotto(String subOptionCode) {
        if (subOptionCode.equals("CRYPTO_SURGE")) {
            return new FinanceResult(12000, "PROFIT", "CRYPTO_SURGE", "가상자산 급등으로 큰 수익이 발생했습니다.");
        }

        if (subOptionCode.equals("STOCK_SURGE")) {
            return new FinanceResult(2500, "PROFIT", "STOCK_SURGE", "시장 호재로 주가가 크게 올랐습니다.");
        }

        if (subOptionCode.equals("GOV_SUPPORT")) {
            return new FinanceResult(1000, "PROFIT", "GOV_SUPPORT", "외부 지원과 호재로 자산이 늘었습니다.");
        }

        if (subOptionCode.equals("RATE_HIKE")) {
            return new FinanceResult(-1500, "LOSS", "RATE_HIKE", "금리 인상 충격으로 자산 가치가 하락했습니다.");
        }

        if (subOptionCode.equals("MARKET_CRASH")) {
            return new FinanceResult(-2000, "LOSS", "MARKET_CRASH", "시장 폭락으로 손실이 커졌습니다.");
        }

        if (subOptionCode.equals("LIVING_COST")) {
            return new FinanceResult(-800, "LOSS", "LIVING_COST", "생활비 부담이 증가해 지출이 커졌습니다.");
        }

        throw new IllegalArgumentException("존재하지 않는 로또 세부 선택지입니다.");
    }

    private int calculatePercent(int baseCoin, int percent) {
        return (baseCoin * percent) / 100;
    }

    private boolean chance(int successPercent) {
        int randomValue = ThreadLocalRandom.current().nextInt(100) + 1;
        return randomValue <= successPercent;
    }

    private static class FinanceResult {
        private final int changeCoin;
        private final String resultType;
        private final String detailResult;
        private final String aiFeedback;

        public FinanceResult(int changeCoin, String resultType, String detailResult, String aiFeedback) {
            this.changeCoin = changeCoin;
            this.resultType = resultType;
            this.detailResult = detailResult;
            this.aiFeedback = aiFeedback;
        }

        public int getChangeCoin() {
            return changeCoin;
        }

        public String getResultType() {
            return resultType;
        }

        public String getDetailResult() {
            return detailResult;
        }

        public String getAiFeedback() {
            return aiFeedback;
        }
    }

    public RunResultResponse submitRunResult(RunResultRequest request, Long userId) {
        if (request == null) {
            throw new IllegalArgumentException("요청값이 없습니다.");
        }

        if (request.getRunId() == null) {
            throw new IllegalArgumentException("runId는 필수입니다.");
        }

        if (request.getStageId() == null || request.getStageId().isBlank()) {
            throw new IllegalArgumentException("stageId는 필수입니다.");
        }

        validateStage(request.getStageId());

        if (request.getPlayTime() < 0) {
            throw new IllegalArgumentException("playTime은 0 이상이어야 합니다.");
        }

        if (request.getDistance() < 0) {
            throw new IllegalArgumentException("distance는 0 이상이어야 합니다.");
        }

        if (request.getCollectedCoin() < 0) {
            throw new IllegalArgumentException("collectedCoin은 0 이상이어야 합니다.");
        }

        if (request.getRemainingHp() < 0) {
            throw new IllegalArgumentException("remainingHp는 0 이상이어야 합니다.");
        }

        if (request.getFinanceChoice() == null || request.getFinanceChoice().isBlank()) {
            throw new IllegalArgumentException("financeChoice는 필수입니다.");
        }

        RunSession runSession = runSessionRepository.findById(request.getRunId())
                .orElseThrow(() -> new IllegalArgumentException("플레이 기록이 존재하지 않습니다."));

        int rewardCoin = request.getCollectedCoin();

        if (request.isQuizCorrect()) {
            rewardCoin += 100;
        }

        if (request.isCleared()) {
            rewardCoin += 200;
        }

        runSession.finishRun(
                request.getRemainingHp(),
                request.getDistance(),
                rewardCoin
        );

        runSessionRepository.save(runSession);

        UserStat userStat = userStatRepository.findByUser_UserId(userId)
                .orElseThrow(() -> new IllegalArgumentException("유저 게임 정보가 없습니다."));

        int remainingTotalCoin = coinService.addCoin(
                userId,
                rewardCoin,
                "RUN_REWARD",
                "게임 보상 지급"
        );
        Stage currentStage = stageRepository.findByStageCode(request.getStageId())
                .orElseThrow(() -> new IllegalArgumentException("존재하지 않는 스테이지입니다."));

        String currentEra = currentStage.getStageCode();
        String nextStep = "FINANCE_EVENT";
        boolean financeEventAvailable = request.isCleared();

        String nextEra = null;
        if (currentEra.equals("ERA_1980")) {
            nextEra = "ERA_2000";
        } else if (currentEra.equals("ERA_2000")) {
            nextEra = "ERA_2020";
        }

        return RunResultResponse.builder()
                .runId(runSession.getRunId())
                .cleared(request.isCleared())
                .rewardCoin(rewardCoin)
                .remainingTotalCoin(remainingTotalCoin)
                .currentEra(currentEra)
                .nextStep(nextStep)
                .financeEventAvailable(financeEventAvailable)
                .nextEra(nextEra)
                .build();
    }

    public UfoRepairResponse repairUfo(UfoRepairRequest request, Long userId) {
        if (request == null) {
            throw new IllegalArgumentException("요청값이 없습니다.");
        }

        if (request.getPartName() == null || request.getPartName().isBlank()) {
            throw new IllegalArgumentException("partName은 필수입니다.");
        }

        String partName = request.getPartName();
        int repairCost;
        String effect;

        if (partName.equals("ENGINE")) {
            repairCost = 1000;
            effect = "부스터 지속시간 증가";
        } else if (partName.equals("WHEEL")) {
            repairCost = 1500;
            effect = "기본 속도 증가";
        } else if (partName.equals("ARMOR")) {
            repairCost = 2000;
            effect = "최대 HP 증가";
        } else {
            throw new IllegalArgumentException("존재하지 않는 파츠입니다.");
        }

        int remainingCoin = coinService.deductCoin(
                userId,
                repairCost,
                "UFO_REPAIR",
                "UFO 수리"
        );

        UserStat userStat = userStatRepository.findByUser_UserId(userId)
                .orElseThrow(() -> new IllegalArgumentException("유저 게임 정보가 없습니다."));

        if (partName.equals("ENGINE")) {
            userStat.addBoosterBonusSec(2);
        } else if (partName.equals("WHEEL")) {
            userStat.addBaseSpeed(1.0);
        } else if (partName.equals("ARMOR")) {
            userStat.addBaseHp(20);
        }

        userStatRepository.save(userStat);

        return UfoRepairResponse.builder()
                .partName(partName)
                .repairCost(repairCost)
                .remainingCoin(remainingCoin)
                .effect(effect)
                .message("UFO 수리가 완료되었습니다.")
                .build();
    }

    public RankingResponse getRankings() {
        List<RankingItemResponse> rankings = new ArrayList<>();

        rankings.add(
                RankingItemResponse.builder()
                        .rank(1)
                        .nickname("playerA")
                        .totalCoin(15000)
                        .build()
        );

        rankings.add(
                RankingItemResponse.builder()
                        .rank(2)
                        .nickname("playerB")
                        .totalCoin(13200)
                        .build()
        );

        rankings.add(
                RankingItemResponse.builder()
                        .rank(3)
                        .nickname("player1")
                        .totalCoin(12000)
                        .build()
        );

        rankings.add(
                RankingItemResponse.builder()
                        .rank(4)
                        .nickname("playerC")
                        .totalCoin(11000)
                        .build()
        );

        rankings.add(
                RankingItemResponse.builder()
                        .rank(5)
                        .nickname("playerD")
                        .totalCoin(9800)
                        .build()
        );

        return RankingResponse.builder()
                .myRank(3)
                .rankings(rankings)
                .build();
    }

    public FinanceOptionsResponse getFinanceOptions(String stageCode) {
        validateStage(stageCode);

        List<FinanceOptionResponse> options = new ArrayList<>();

        List<FinanceSubOptionResponse> savingSubOptions = new ArrayList<>();
        List<FinanceSubOptionResponse> investmentSubOptions = new ArrayList<>();
        List<FinanceSubOptionResponse> lottoSubOptions = new ArrayList<>();

        if (stageCode.equals("ERA_1980")) {

            savingSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("BANK_DEPOSIT")
                            .name("은행 정기예금")
                            .description("은행에 돈만 넣어도 두 자릿수 이자를 기대할 수 있는 안정형 상품")
                            .build()
            );

            savingSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("PRIVATE_FINANCE")
                            .name("계 / 사금융")
                            .description("높은 수익을 기대할 수 있지만 위험도 큰 선택")
                            .build()
            );

            investmentSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("GOLD")
                            .name("금")
                            .description("경제 불안 시 가치가 주목받는 안전자산")
                            .build()
            );

            investmentSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("LAND")
                            .name("토지 / 부동산")
                            .description("개발 기대감이 반영되는 실물자산")
                            .build()
            );

            investmentSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("HEAVY_INDUSTRY")
                            .name("중공업 투자")
                            .description("조선·철강 등 산업 성장 기대 투자")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("JACKPOT")
                            .name("대박 당첨")
                            .description("극저확률로 큰 보상을 얻는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("BUSINESS_CHANCE")
                            .name("사업 기회")
                            .description("뜻밖의 돈 벌 기회가 생기는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("HOUSE_CHANCE")
                            .name("집 마련 기회")
                            .description("자산 형성에 유리한 기회가 생기는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("HOSPITAL_COST")
                            .name("병원비 지출")
                            .description("갑작스러운 의료비가 발생하는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("EDUCATION_COST")
                            .name("학원비 / 교육비")
                            .description("교육 관련 지출이 발생하는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("FAMILY_EVENT_COST")
                            .name("집안 행사비")
                            .description("경조사나 집안 행사로 지출이 생기는 이벤트")
                            .build()
            );
        } else if (stageCode.equals("ERA_2000")) {

            savingSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("BANK_SAVINGS")
                            .name("A은행 예금")
                            .description("안정적으로 돈을 모으는 기본 선택")
                            .build()
            );

            savingSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("BANK_INSTALLMENT")
                            .name("B은행 적금")
                            .description("예금보다 조금 높은 수익을 기대하는 선택")
                            .build()
            );

            investmentSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("SAMSUNG")
                            .name("삼성전자")
                            .description("대표 대기업/IT 성장 수혜 기대 투자")
                            .build()
            );

            investmentSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("VENTURE_IT")
                            .name("벤처 / IT주")
                            .description("고수익 가능성이 크지만 변동성도 큰 투자")
                            .build()
            );

            investmentSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("APARTMENT")
                            .name("아파트 / 부동산")
                            .description("중장기 자산 상승을 기대하는 투자")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("LOTTO_FIRST")
                            .name("1등 당첨")
                            .description("실제 로또 감성의 최고 보상 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("LOTTO_SMALL_WIN")
                            .name("소소한 당첨")
                            .description("작지만 기분 좋은 보상을 얻는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("JOB_SUCCESS")
                            .name("취업 성공")
                            .description("인생 이벤트로 코인을 얻는 기회")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("CARD_DEBT")
                            .name("카드값 폭탄")
                            .description("과소비로 인해 큰 지출이 발생하는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("TUITION_COST")
                            .name("등록금 / 학원비")
                            .description("교육비 부담이 생기는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("INVEST_LOSS")
                            .name("투자 손실")
                            .description("잘못된 판단으로 손실이 나는 이벤트")
                            .build()
            );
        } else if (stageCode.equals("ERA_2020")) {

            savingSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("BANK_DEPOSIT")
                            .name("A은행 예금")
                            .description("가장 안전하지만 수익은 낮은 선택")
                            .build()
            );

            savingSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("CMA")
                            .name("B은행 CMA / 파킹형")
                            .description("유동성은 좋지만 수익은 낮은 선택")
                            .build()
            );

            investmentSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("ETF")
                            .name("ETF")
                            .description("분산 투자로 비교적 안정적인 선택")
                            .build()
            );

            investmentSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("US_GROWTH")
                            .name("미국주식 / 성장주")
                            .description("성장 가능성이 높지만 변동성도 존재")
                            .build()
            );

            investmentSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("CRYPTO")
                            .name("코인")
                            .description("초고위험 초고수익 자산")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("CRYPTO_SURGE")
                            .name("코인 폭등")
                            .description("극단적 상승으로 큰 수익을 얻는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("STOCK_SURGE")
                            .name("주식 급등")
                            .description("시장 호재로 수익이 나는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("GOV_SUPPORT")
                            .name("정부 지원금 / 호재")
                            .description("외부 호재로 자산이 늘어나는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("RATE_HIKE")
                            .name("금리 인상 충격")
                            .description("긴축으로 자산시장이 흔들리는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("MARKET_CRASH")
                            .name("시장 폭락")
                            .description("전체 시장 하락으로 손실이 발생하는 이벤트")
                            .build()
            );

            lottoSubOptions.add(
                    FinanceSubOptionResponse.builder()
                            .code("LIVING_COST")
                            .name("생활비 급증")
                            .description("물가 상승으로 고정 지출이 커지는 이벤트")
                            .build()
            );
        }

        options.add(
                FinanceOptionResponse.builder()
                        .optionType("SAVING")
                        .title("저축")
                        .description("안정적으로 자산을 관리하는 선택")
                        .subOptions(savingSubOptions)
                        .build()
        );

        options.add(
                FinanceOptionResponse.builder()
                        .optionType("INVESTMENT")
                        .title("투자")
                        .description("수익 가능성과 손실 위험이 함께 있는 선택")
                        .subOptions(investmentSubOptions)
                        .build()
        );

        options.add(
                FinanceOptionResponse.builder()
                        .optionType("LOTTO")
                        .title("로또")
                        .description("랜덤 이벤트로 큰 보상 또는 지출이 발생하는 선택")
                        .subOptions(lottoSubOptions)
                        .build()
        );

        return FinanceOptionsResponse.builder()
                .options(options)
                .build();
    }

    private List<FinanceOptionResponse> get1980Options() {
        List<FinanceOptionResponse> options = new ArrayList<>();

        options.add(
                FinanceOptionResponse.builder()
                        .optionType("SAVING")
                        .title("저축")
                        .description("금리가 높아 안정적인 수익을 기대할 수 있습니다.")
                        .subOptions(new ArrayList<>())
                        .build()
        );

        options.add(
                FinanceOptionResponse.builder()
                        .optionType("LOTTO")
                        .title("로또")
                        .description("높은 리스크, 높은 보상")
                        .subOptions(new ArrayList<>())
                        .build()
        );

        return options;
    }

    private List<FinanceOptionResponse> get2000Options() {
        List<FinanceOptionResponse> options = new ArrayList<>();

        List<FinanceSubOptionResponse> investment = new ArrayList<>();

        investment.add(
                FinanceSubOptionResponse.builder()
                        .code("IT")
                        .name("IT 산업")
                        .description("닷컴버블과 함께 성장하는 산업")
                        .build()
        );

        options.add(
                FinanceOptionResponse.builder()
                        .optionType("INVESTMENT")
                        .title("투자")
                        .description("산업 선택에 따라 수익이 달라집니다.")
                        .subOptions(investment)
                        .build()
        );

        return options;
    }

    private List<FinanceOptionResponse> get2020Options() {
        List<FinanceOptionResponse> options = new ArrayList<>();

        List<FinanceSubOptionResponse> investment = new ArrayList<>();

        investment.add(
                FinanceSubOptionResponse.builder()
                        .code("CRYPTO")
                        .name("암호화폐")
                        .description("변동성이 매우 높은 투자")
                        .build()
        );

        investment.add(
                FinanceSubOptionResponse.builder()
                        .code("STOCK")
                        .name("주식")
                        .description("시장 상황에 따라 변동")
                        .build()
        );

        options.add(
                FinanceOptionResponse.builder()
                        .optionType("INVESTMENT")
                        .title("투자")
                        .description("고위험 고수익 선택지")
                        .subOptions(investment)
                        .build()
        );

        return options;
    }

    private void validateStage(String stage) {
        if (stage == null || stage.isBlank()) {
            throw new IllegalArgumentException("스테이지 값은 필수입니다.");
        }

        if (!stage.matches("^ERA_(1980|2000|2020)$")) {
            throw new IllegalArgumentException("존재하지 않는 스테이지입니다.");
        }
    }



    public List<RunHistoryResponse> getRunHistory(Long userId) {
        List<RunSession> sessions = runSessionRepository.findByUserIdOrderByStartedAtDesc(userId);

        List<RunHistoryResponse> result = new ArrayList<>();

        for (RunSession session : sessions) {
            boolean cleared = "FINISHED".equals(session.getStatus());

            RunHistoryResponse dto = new RunHistoryResponse(
                    session.getRunId(),
                    session.getStageId(),
                    session.getDistanceReached(),
                    session.getCollectedCoin(),
                    cleared,
                    session.getStartedAt()
            );

            result.add(dto);
        }

        return result;
    }
}