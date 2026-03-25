package com.ssafy.amagetdon.domain.game.repository;

import com.ssafy.amagetdon.domain.game.entity.Inventory;
import com.ssafy.amagetdon.domain.game.entity.Item;
import com.ssafy.amagetdon.domain.user.entity.User;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;

public interface InventoryRepository extends JpaRepository<Inventory, Long> {

    Optional<Inventory> findByUserAndItem(User user, Item item);

    List<Inventory> findByUser_UserId(Long userId);
}