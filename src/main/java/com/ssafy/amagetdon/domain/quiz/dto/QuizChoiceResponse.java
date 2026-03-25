package com.ssafy.amagetdon.domain.quiz.dto;

import lombok.AllArgsConstructor;
import lombok.Getter;

@Getter
@AllArgsConstructor
public class QuizChoiceResponse {

    private Long quizChoiceId;
    private String choiceText;
}