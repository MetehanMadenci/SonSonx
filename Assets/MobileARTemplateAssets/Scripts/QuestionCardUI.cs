using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestionCardUI : MonoBehaviour
{
    public TMP_Text questionText;
    public TMP_Text[] optionTexts;
    public Button selectButton;

    private QuizQuestionData currentData;
    private System.Action onSelect;

public void SetQuestion(QuizQuestionData data, System.Action onSelectCallback)
{
    currentData = data;
    onSelect = onSelectCallback;

    questionText.text = data.questionText;

    Debug.Log($"[SetQuestion] Soru: {data.questionText}");
    Debug.Log($"[SetQuestion] Şık Sayısı: {data.questionAnswers?.Count}");

    for (int i = 0; i < optionTexts.Length && i < data.questionAnswers.Count; i++)
    {
        Debug.Log($"[SetQuestion] Şık {i}: {data.questionAnswers[i]}");
        optionTexts[i].text = data.questionAnswers[i];
    }

    selectButton.onClick.AddListener(() => onSelect?.Invoke());
}

    
}
