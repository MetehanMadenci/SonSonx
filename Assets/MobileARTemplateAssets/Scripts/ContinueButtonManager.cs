using UnityEngine;

public class ContinueButtonManager : MonoBehaviour
{
    public PreviewEditorManager previewEditorManager;

    public void OnContinueButtonClick()
    {
        QuestionDataPre currentData = previewEditorManager.GetEditedQuestionData();

        // Burada console'a yazalım (test için)
        Debug.Log("Soru: " + currentData.question);
        for (int i = 0; i < currentData.choices.Count; i++)
        {
            Debug.Log("Seçenek " + (char)('A' + i) + ": " + currentData.choices[i]);
        }

        // Firestore'a gönderme adımı burada olacak
    }
}
