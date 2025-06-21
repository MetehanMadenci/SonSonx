using UnityEngine;


[CreateAssetMenu(fileName = "NewQuestion", menuName ="Question")]

public class QuizQuestion : ScriptableObject
{
    [SerializeField] private string questionID;

    [SerializeField] private string questionText;
    public string QuestionText => questionText;

    [SerializeField] private string[] options = new string[5];
    public string[] Options => options;

    [SerializeField] private char correctAnswer;
    public char CorrectAnswer => correctAnswer;
}
