package com.ssafy.amagetdon.domain.quiz.entity;

import jakarta.persistence.*;
import lombok.AccessLevel;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;

@Getter
@Entity
@Table(name = "quiz_choice")
@NoArgsConstructor(access = AccessLevel.PROTECTED)
public class QuizChoice {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "quiz_choice_id")
    private Long id;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "quiz_question_id", nullable = false)
    private QuizQuestion quizQuestion;

    @Column(name = "choice_text", nullable = false, length = 255)
    private String choiceText;

    @Column(name = "is_correct", nullable = false)
    private Boolean isCorrect;

    @Builder
    public QuizChoice(QuizQuestion quizQuestion, String choiceText, Boolean isCorrect) {
        this.quizQuestion = quizQuestion;
        this.choiceText = choiceText;
        this.isCorrect = isCorrect;
    }
}