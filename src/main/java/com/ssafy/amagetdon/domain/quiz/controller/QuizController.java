package com.ssafy.amagetdon.domain.quiz.controller;

import com.ssafy.amagetdon.domain.quiz.dto.QuizQuestionResponse;
import com.ssafy.amagetdon.domain.quiz.service.QuizService;
import lombok.RequiredArgsConstructor;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.bind.annotation.RequestParam;

@RestController
@RequiredArgsConstructor
public class QuizController {

    private final QuizService quizService;

    @GetMapping("/api/game/quiz")
    public QuizQuestionResponse getQuiz(@RequestParam Long runId) {
        return quizService.getRandomQuiz(runId);
    }
}
