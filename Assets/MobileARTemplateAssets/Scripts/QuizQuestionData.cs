using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData] // 🔴 Bu attribute çok önemli
public class QuizQuestionData
{
    [FirestoreProperty]
    public string questionID { get; set; }

    [FirestoreProperty]
    public string questionText { get; set; }

    [FirestoreProperty]
    public List<string> questionAnswers { get; set; }

    [FirestoreProperty]
    public string answer { get; set; }
}
