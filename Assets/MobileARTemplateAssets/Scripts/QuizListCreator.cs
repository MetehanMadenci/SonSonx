using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using TMPro;

public class QuizListCreator : MonoBehaviour
{
    [Header("Gerekli Referanslar")]
    public TMP_InputField quizTitleInput;
    public QuizSelectorManager selectorManager;

    [Header("UI Mesaj Alanı (Opsiyonel)")]
    public TMP_Text feedbackText; // Inspector'dan atanmalı

    public void SaveToFirestore()
{
    Debug.Log("🟢 Butona tıklandı");

    if (selectorManager == null)
    {
        Debug.LogError("❌ selectorManager atanmadı (null)!");
        return;
    }

    var selectedQuestions = selectorManager.selectedQuestions;

    if (selectedQuestions == null || selectedQuestions.Count == 0)
    {
        Debug.LogWarning("⚠️ Hiç soru seçilmedi.");
        return;
    }

    for (int i = 0; i < selectedQuestions.Count; i++)
    {
        var q = selectedQuestions[i];

        if (string.IsNullOrEmpty(q.questionText) || string.IsNullOrEmpty(q.answer))
        {
            Debug.LogError($"❌ Soru {i} metni veya cevabı boş!");
            Debug.Log($"➡️ ID: {q.questionID}, Text: '{q.questionText}', Answer: '{q.answer}'");
            return;
        }
    }

    if (string.IsNullOrEmpty(quizTitleInput.text))
    {
        Debug.LogWarning("⚠️ Quiz başlığı boş.");
        return;
    }

    QuizListData newQuizList = new QuizListData
    {
        quizTitle = quizTitleInput.text,
        questions = new List<QuizQuestionData>(selectedQuestions)
    };

    FirebaseFirestore.DefaultInstance
        .Collection("quizzes")
        .AddAsync(newQuizList)
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("✅ Firestore'a quiz kaydedildi.");
            }
            else
            {
                Debug.LogError("❌ Kaydedilemedi: " + task.Exception);
            }
        });
}


    private void UpdateFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }
}
