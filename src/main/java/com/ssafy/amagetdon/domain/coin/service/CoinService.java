package com.ssafy.amagetdon.domain.coin.service;

import com.ssafy.amagetdon.common.exception.InsufficientCoinException;
import com.ssafy.amagetdon.common.exception.InvalidCoinAmountException;
import com.ssafy.amagetdon.domain.game.entity.UserStat;
import com.ssafy.amagetdon.domain.game.repository.UserStatRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
@RequiredArgsConstructor
@Transactional
public class CoinService {

    private final UserStatRepository userStatRepository;
    private final CoinTransactionService coinTransactionService;

    public int deductCoin(Long userId, int amount, String txType, String description) {
        validateAmount(amount);

        UserStat userStat = userStatRepository.findByUser_UserId(userId)
                .orElseThrow(() -> new IllegalArgumentException("사용자 게임 정보가 존재하지 않습니다."));

        if (userStat.getCoinBalance() < amount) {
            throw new InsufficientCoinException("코인이 부족합니다.");
        }

        userStat.useCoin(amount);
        int newBalance = userStat.getCoinBalance();

        coinTransactionService.saveTransaction(
                userStat.getUser(),
                null,
                txType,
                -amount,
                newBalance,
                description
        );

        return newBalance;
    }

    public int addCoin(Long userId, int amount, String txType, String description) {
        validateAmount(amount);

        UserStat userStat = userStatRepository.findByUser_UserId(userId)
                .orElseThrow(() -> new IllegalArgumentException("사용자 게임 정보가 존재하지 않습니다."));

        userStat.addCoin(amount);
        int newBalance = userStat.getCoinBalance();

        coinTransactionService.saveTransaction(
                userStat.getUser(),
                null,
                txType,
                amount,
                newBalance,
                description
        );

        return newBalance;
    }

    private void validateAmount(int amount) {
        if (amount <= 0) {
            throw new InvalidCoinAmountException("유효하지 않은 코인 금액입니다.");
        }
    }
}