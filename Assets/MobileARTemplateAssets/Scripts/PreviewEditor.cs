using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PreviewEditorManager : MonoBehaviour
{
    private QuestionDataPre currentQuestionData;


    [SerializeField] private TMP_InputField questionInput;
    [SerializeField] private List<TMP_InputField> choiceInputs;
    [SerializeField] private TMP_InputField correctAnswerInput;

    void Start()
    {

        questionInput.interactable = false;

        foreach (var input in choiceInputs)
        {
            input.interactable = false;
        }

        correctAnswerInput.interactable = false;
    }

    public QuestionDataPre GetEditedQuestionData()
    {
        QuestionDataPre data = new QuestionDataPre();
        data.question = questionInput.text;

        foreach (var input in choiceInputs)
        {
            if (input != null)
                data.choices.Add(input.text);
        }

        data.correctAnswer = correctAnswerInput.text;

        return data;
    }

    public void EnableEditing()
    {
        questionInput.interactable = true;

        foreach (var input in choiceInputs)
        {
            if (input != null)
                input.interactable = true;
        }

        correctAnswerInput.interactable = true;
    }

    public void OnClickContinue()
    {
        currentQuestionData = new QuestionDataPre();
        currentQuestionData.question = questionInput.text;

        currentQuestionData.choices = new List<string>();
        foreach (var input in choiceInputs)
        {
            currentQuestionData.choices.Add(input.text);
        }

        currentQuestionData.correctAnswer = correctAnswerInput.text;

        Debug.Log("Soru: " + currentQuestionData.question);
        for (int i = 0; i < currentQuestionData.choices.Count; i++)
        {
            Debug.Log("Seçenek " + (char)('A' + i) + ": " + currentQuestionData.choices[i]);
        }
        Debug.Log("Doğru Cevap: " + currentQuestionData.correctAnswer);

        
    }
}
