package com.ssafy.amagetdon.domain.quiz.dto;

import lombok.AllArgsConstructor;
import lombok.Getter;

import java.util.List;

@Getter
@AllArgsConstructor
public class QuizQuestionResponse {

    private Long quizQuestionId;
    private String questionText;
    private Integer timeLimitSec;
    private List<QuizChoiceResponse> choices;
}