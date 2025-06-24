using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class QuizSelectorManager : MonoBehaviour
{
    public List<QuizQuestionData> selectedQuestions = new();

    private FirebaseFirestore firestore;
    public Transform contentParent;
    public GameObject questionCardPrefab;

    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
    }

    public void FetchQuestionsFromFirestore()
    {
        firestore.Collection("questions").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    var data = doc.ConvertTo<QuizQuestionData>();
                    var card = Instantiate(questionCardPrefab, contentParent);

                    var ui = card.GetComponent<QuestionCardUI>();
                    ui.SetQuestion(data, () =>
                    {
                        if (!selectedQuestions.Contains(data))
                        {
                            selectedQuestions.Add(data);
                            Debug.Log("✅ Soru eklendi: " + data.questionID);
                        }
                    });
                }
            }
            else
            {
                Debug.LogError("❌ Soru yüklenemedi: " + task.Exception);
            }
        });
    }
}
