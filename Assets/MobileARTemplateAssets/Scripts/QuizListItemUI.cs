using UnityEngine;
public class QuizListItemUI : MonoBehaviour
{
    public TMPro.TMP_Text titleText;
    private QuizListData quizData;
    private System.Action<QuizListData> onSelect;

    public void Setup(QuizListData data, System.Action<QuizListData> onClick)
    {
        quizData = data;
        titleText.text = data.quizTitle;
        onSelect = onClick;
    }

public PanelManager panelManager; // Inspector’dan atanmalı

public void OnClick()
{
    Debug.Log("✅ Quiz item butonuna tıklandı: " + quizData.quizTitle);

    if (panelManager != null)
    {
        panelManager.ShowQuizUI(); // 🔹 Sadece quiz UI gösterilmeli
    }

    onSelect?.Invoke(quizData); // 🔹 Quiz soruları yüklenir
}


}
