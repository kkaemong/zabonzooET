package com.ssafy.amagetdon.domain.quiz.service;

import com.ssafy.amagetdon.domain.quiz.entity.QuizChoice;
import com.ssafy.amagetdon.domain.quiz.entity.QuizQuestion;
import com.ssafy.amagetdon.domain.quiz.repository.QuizChoiceRepository;
import com.ssafy.amagetdon.domain.quiz.repository.QuizQuestionRepository;
import jakarta.annotation.PostConstruct;
import lombok.RequiredArgsConstructor;
import org.springframework.core.io.ClassPathResource;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

@Component
@RequiredArgsConstructor
public class QuizDataLoader {

    private final QuizQuestionRepository quizQuestionRepository;
    private final QuizChoiceRepository quizChoiceRepository;

    @PostConstruct
    @Transactional
    public void loadQuizData() {
        if (quizQuestionRepository.count() > 0) {
            return;
        }

        List<QuizCsvRow> rows = readCsv();
        List<String> allTerms = new ArrayList<>();

        for (QuizCsvRow row : rows) {
            allTerms.add(row.term());
        }

        for (QuizCsvRow row : rows) {
            QuizQuestion question = QuizQuestion.builder()
                    .questionText("다음 설명에 해당하는 금융 용어는 무엇일까요?\n" + row.description())
                    .difficulty("EASY")
                    .timeLimitSec(10)
                    .explanation(row.description())
                    .isActive(true)
                    .build();

            QuizQuestion savedQuestion = quizQuestionRepository.save(question);

            List<String> choices = createChoices(row.term(), allTerms);

            for (String choice : choices) {
                QuizChoice quizChoice = QuizChoice.builder()
                        .quizQuestion(savedQuestion)
                        .choiceText(choice)
                        .isCorrect(choice.equals(row.term()))
                        .build();

                quizChoiceRepository.save(quizChoice);
            }
        }
    }

    private List<QuizCsvRow> readCsv() {
        List<QuizCsvRow> rows = new ArrayList<>();

        try (
                BufferedReader reader = new BufferedReader(
                        new InputStreamReader(
                                new ClassPathResource("quiz/한국산업은행_금융 관련 용어_20151231.csv").getInputStream(),
                                "EUC-KR"
                        )
                )
        ) {
            String line = reader.readLine();

            while ((line = reader.readLine()) != null) {
                String[] tokens = line.split(",", 4);

                if (tokens.length < 4) {
                    continue;
                }

                String category = tokens[0].trim();
                String subcategory = tokens[1].trim();
                String term = tokens[2].trim();
                String description = tokens[3].trim();

                rows.add(new QuizCsvRow(category, subcategory, term, description));
            }
        } catch (Exception e) {
            throw new RuntimeException("퀴즈 CSV 로딩 실패", e);
        }

        return rows;
    }

    private List<String> createChoices(String answer, List<String> allTerms) {
        List<String> shuffled = new ArrayList<>();
        List<String> result = new ArrayList<>();

        for (String term : allTerms) {
            if (!term.equals(answer)) {
                shuffled.add(term);
            }
        }

        Collections.shuffle(shuffled);

        result.add(answer);

        int count = 0;
        for (String wrong : shuffled) {
            result.add(wrong);
            count++;

            if (count == 3) {
                break;
            }
        }

        Collections.shuffle(result);
        return result;
    }

    private record QuizCsvRow(
            String category,
            String subcategory,
            String term,
            String description
    ) {
    }
}