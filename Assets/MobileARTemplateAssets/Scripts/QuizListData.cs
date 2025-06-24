using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
public class QuizListData
{
    [FirestoreProperty]
    public string quizTitle { get; set; }

    [FirestoreProperty]
    public List<QuizQuestionData> questions { get; set; }
}
