package com.ssafy.amagetdon.domain.coin.repository;

import com.ssafy.amagetdon.domain.coin.entity.CoinTransaction;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface CoinTransactionRepository extends JpaRepository<CoinTransaction, Long> {

    List<CoinTransaction> findByUser_UserIdOrderByCreatedAtDesc(Long userId);
}