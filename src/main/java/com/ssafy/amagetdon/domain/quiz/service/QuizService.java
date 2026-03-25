package com.ssafy.amagetdon.domain.quiz.service;

import com.ssafy.amagetdon.domain.quiz.dto.QuizChoiceResponse;
import com.ssafy.amagetdon.domain.quiz.dto.QuizQuestionResponse;
import com.ssafy.amagetdon.domain.quiz.entity.QuizChoice;
import com.ssafy.amagetdon.domain.quiz.entity.QuizQuestion;
import com.ssafy.amagetdon.domain.quiz.repository.QuizChoiceRepository;
import com.ssafy.amagetdon.domain.quiz.repository.QuizQuestionRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import com.ssafy.amagetdon.domain.game.entity.RunQuizEvent;
import com.ssafy.amagetdon.domain.game.repository.RunQuizEventRepository;

import java.util.ArrayList;
import java.util.List;

@Service
@RequiredArgsConstructor
public class QuizService {

    private final QuizQuestionRepository quizQuestionRepository;
    private final QuizChoiceRepository quizChoiceRepository;
    private final RunQuizEventRepository runQuizEventRepository;

    @Transactional(readOnly = true)
    public QuizQuestionResponse getRandomQuiz(Long runId) {

        List<RunQuizEvent> solvedEvents = runQuizEventRepository.findByRunId(runId);
        List<Long> solvedQuizIds = new ArrayList<>();

        for (RunQuizEvent event : solvedEvents) {
            solvedQuizIds.add(event.getQuizId());
        }

        List<QuizQuestion> allQuestions = quizQuestionRepository.findAll();
        List<QuizQuestion> availableQuestions = new ArrayList<>();

        for (QuizQuestion question : allQuestions) {
            if (!solvedQuizIds.contains(question.getId())) {
                availableQuestions.add(question);
            }
        }

        if (availableQuestions.isEmpty()) {
            throw new RuntimeException("출제 가능한 퀴즈가 없습니다.");
        }

        int randomIndex = (int) (Math.random() * availableQuestions.size());
        QuizQuestion question = availableQuestions.get(randomIndex);

        List<QuizChoice> choices = quizChoiceRepository.findAll();
        List<QuizChoiceResponse> choiceResponses = new ArrayList<>();

        for (QuizChoice choice : choices) {
            if (choice.getQuizQuestion().getId().equals(question.getId())) {
                choiceResponses.add(
                        new QuizChoiceResponse(
                                choice.getId(),
                                choice.getChoiceText()
                        )
                );
            }
        }

        return new QuizQuestionResponse(
                question.getId(),
                question.getQuestionText(),
                question.getTimeLimitSec(),
                choiceResponses
        );
    }
    @Transactional(readOnly = true)
    public boolean checkAnswer(Long quizId, Integer selectedAnswer) {
        QuizQuestion question = quizQuestionRepository.findById(quizId)
                .orElseThrow(() -> new RuntimeException("해당 퀴즈가 없습니다."));

        List<QuizChoice> allChoices = quizChoiceRepository.findAll();
        List<QuizChoice> questionChoices = new ArrayList<>();

        for (QuizChoice choice : allChoices) {
            if (choice.getQuizQuestion().getId().equals(question.getId())) {
                questionChoices.add(choice);
            }
        }

        if (selectedAnswer == null || selectedAnswer < 1 || selectedAnswer > questionChoices.size()) {
            throw new RuntimeException("선택한 답 번호가 올바르지 않습니다.");
        }

        QuizChoice selectedChoice = questionChoices.get(selectedAnswer - 1);
        return selectedChoice.getIsCorrect();
    }
}