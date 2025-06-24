using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;

public class QuestionUploader : MonoBehaviour
{
    public TMP_InputField questionInput;
    public TMP_InputField optionAInput;
    public TMP_InputField optionBInput;
    public TMP_InputField optionCInput;
    public TMP_InputField optionDInput;
    public TMP_InputField optionEInput;
    public TMP_InputField answerInput;

    private DatabaseReference dbRef;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void OnContinuePressed()
    {
        QuestionData questionData = new QuestionData
        {
            question = questionInput.text,
            optionA = optionAInput.text,
            optionB = optionBInput.text,
            optionC = optionCInput.text,
            optionD = optionDInput.text,
            optionE = optionEInput.text,
            answer = answerInput.text.ToUpper().Trim() // örn: "C"
        };

        string key = dbRef.Child("questions").Push().Key;
        dbRef.Child("questions").Child(key).SetRawJsonValueAsync(JsonUtility.ToJson(questionData))
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    Debug.Log("Soru başarıyla gönderildi!");
                else
                    Debug.LogError("Hata oluştu: " + task.Exception);
            });
    }
}
