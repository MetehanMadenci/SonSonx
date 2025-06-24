using UnityEngine;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;


public class QuestionManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField questionInput;
    public TMP_InputField optionAInput;
    public TMP_InputField optionBInput;
    public TMP_InputField optionCInput;
    public TMP_InputField optionDInput;
    public TMP_InputField optionEInput;
    public TMP_InputField correctAnswerInput;

    [Header("Question List")]
    public QuestionList questionList;

    public void OnContinueButtonPressed()
{
    if (questionList == null)
    {
        Debug.LogError("QuestionList atanmadı!");
        return;
    }

    // Yeni soru oluştur
    QuizQuestion newQuestion = ScriptableObject.CreateInstance<QuizQuestion>();

    newQuestion.QuestionID = System.Guid.NewGuid().ToString();
    newQuestion.QuestionText = questionInput.text;
    newQuestion.Options = new string[5]
    {
        optionAInput.text,
        optionBInput.text,
        optionCInput.text,
        optionDInput.text,
        optionEInput.text
    };

    // Doğru cevap kontrolü
   string rawInput = correctAnswerInput.text;
string correctInput = rawInput?.Trim().ToUpper();

Debug.Log($"[DEBUG] rawInput: '{rawInput}'");
Debug.Log($"[DEBUG] trimmed correctInput: '{correctInput}'");

if (!string.IsNullOrEmpty(correctInput) &&
    correctInput.Length == 1 &&
    "ABCDE".Contains(correctInput[0]))
{
    newQuestion.CorrectAnswer = correctInput[0];
    Debug.Log($"[DEBUG] Doğru cevap başarıyla atandı: {correctInput[0]}");
}
else
{
    Debug.LogError("❌ Hatalı giriş! Doğru cevap yalnızca A, B, C, D veya E olmalı!");
    return;
}


    // Listeye ekle
    questionList.questionList.Add(newQuestion);

    Debug.Log("✅ Soru eklendi! Şu anki toplam soru sayısı: " + questionList.questionList.Count);

    // Alanları temizle
    ClearInputs();
FirebaseFirestore firestore = FirebaseFirestore.DefaultInstance;

// Veri aktarımı için QuizQuestionData oluştur
QuizQuestionData data = new QuizQuestionData
{
    questionID = newQuestion.QuestionID,
    questionText = newQuestion.QuestionText,
    questionAnswers = new List<string>(newQuestion.Options),
    answer = newQuestion.CorrectAnswer.ToString()
};

// Firestore'a yaz (document ID olarak questionID kullanılıyor)
firestore.Collection("questions").Document(data.questionID).SetAsync(data).ContinueWithOnMainThread(task =>
{
    if (task.IsCompletedSuccessfully)
    {
        Debug.Log("✅ Firestore'a başarıyla gönderildi.");
    }
    else
    {
        Debug.LogError("❌ Firestore gönderim hatası: " + task.Exception);
    }
});}

    private void ClearInputs()
    {
        questionInput.text = "";
        optionAInput.text = "";
        optionBInput.text = "";
        optionCInput.text = "";
        optionDInput.text = "";
        optionEInput.text = "";
        correctAnswerInput.text = "";
    }
}
