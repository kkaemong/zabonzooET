package com.ssafy.amagetdon.domain.game.repository;

import com.ssafy.amagetdon.domain.game.entity.Item;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface ItemRepository extends JpaRepository<Item, Long> {

    List<Item> findByIsActiveTrue();
}