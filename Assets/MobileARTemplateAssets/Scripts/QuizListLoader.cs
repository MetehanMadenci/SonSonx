using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class QuizListLoader : MonoBehaviour
{
    public List<QuizListData> allQuizLists = new();

    public void LoadAllQuizLists(System.Action<List<QuizListData>> onComplete)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        db.Collection("quizzes").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                allQuizLists.Clear();
                foreach (var doc in task.Result.Documents)
                {
                    QuizListData quiz = doc.ConvertTo<QuizListData>();
                    allQuizLists.Add(quiz);
                }

                onComplete?.Invoke(allQuizLists);
            }
            else
            {
                Debug.LogError("Quiz listeleri alınamadı: " + task.Exception);
            }
        });
    }
}
