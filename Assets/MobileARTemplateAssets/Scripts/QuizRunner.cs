using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuizRunner : MonoBehaviour
{
    public List<QuizQuestionData> selectedQuestions = new();
    private int currentQuestionIndex = 0;
    private List<string> answerKey = new();

    [Header("UI Elements")]
    public TMP_Text questionText;
    public TMP_Text[] optionTexts;

    public void SetSelectedQuestions(List<QuizQuestionData> questions)
    {
        selectedQuestions = questions;
        currentQuestionIndex = 0;
        answerKey.Clear();

        foreach (var q in selectedQuestions)
        {
            answerKey.Add(q.answer);
        }

        ShowQuestion(currentQuestionIndex);
    }

    public void ShowQuestion(int index)
    {
        if (index < 0 || index >= selectedQuestions.Count) return;

        var q = selectedQuestions[index];
        questionText.text = $"Soru {index + 1}: {q.questionText}";

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (i < q.questionAnswers.Count)
            {
                optionTexts[i].gameObject.SetActive(true);
                optionTexts[i].text = $"{(char)('A' + i)}: {q.questionAnswers[i]}";
            }
            else
            {
                optionTexts[i].gameObject.SetActive(false);
            }
        }
    }

    public void NextQuestion()
    {
        if (currentQuestionIndex < selectedQuestions.Count - 1)
        {
            currentQuestionIndex++;
            ShowQuestion(currentQuestionIndex);
        }
    }

    public void PreviousQuestion()
    {
        if (currentQuestionIndex > 0)
        {
            currentQuestionIndex--;
            ShowQuestion(currentQuestionIndex);
        }
    }

    public void PrintAnswerKey()
    {
        for (int i = 0; i < answerKey.Count; i++)
        {
            Debug.Log($"{i + 1}. Soru: {answerKey[i]}");
        }
    }

    // Hazır quiz listesinden başlatmak için
    public void StartQuizFromList(QuizListData quiz)
    {
        SetSelectedQuestions(quiz.questions);
    }
}
