package com.ssafy.amagetdon.domain.coin.service;

import com.ssafy.amagetdon.domain.coin.entity.CoinTransaction;
import com.ssafy.amagetdon.domain.coin.repository.CoinTransactionRepository;
import com.ssafy.amagetdon.domain.game.entity.RunSession;
import com.ssafy.amagetdon.domain.user.entity.User;
import com.ssafy.amagetdon.domain.game.dto.CoinTransactionResponse;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import java.util.ArrayList;
import java.util.List;

@Service
@RequiredArgsConstructor
public class CoinTransactionService {

    private final CoinTransactionRepository coinTransactionRepository;

    public void saveTransaction(
            User user,
            RunSession runSession,
            String txType,
            int amount,
            int balanceAfter,
            String description
    ) {
        CoinTransaction tx = CoinTransaction.builder()
                .user(user)
                .runSession(runSession)
                .txType(txType)
                .amount(amount)
                .balanceAfter(balanceAfter)
                .description(description)
                .build();

        coinTransactionRepository.save(tx);
    }
    public List<CoinTransactionResponse> getTransactions(Long userId) {

        List<CoinTransaction> transactions =
                coinTransactionRepository.findByUser_UserIdOrderByCreatedAtDesc(userId);

        List<CoinTransactionResponse> result = new ArrayList<>();

        for (CoinTransaction tx : transactions) {
            CoinTransactionResponse response = new CoinTransactionResponse(
                    tx.getTxType(),
                    tx.getAmount(),
                    tx.getBalanceAfter(),
                    tx.getDescription(),
                    tx.getCreatedAt()
            );
            result.add(response);
        }

        return result;
    }
}