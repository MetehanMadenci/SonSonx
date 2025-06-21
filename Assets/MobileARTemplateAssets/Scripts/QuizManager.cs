using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{

    [SerializeField] private Text questionText;
    [SerializeField] private List<Text> optionTexts;
    [SerializeField] private List<Button> optionButtons;
    [SerializeField] private Button continueButton;
    [SerializeField] private Text feedbackText;
    [SerializeField] private QuestionList questions;
    [SerializeField] private Text timerText;
    [SerializeField] private Text countdownText;

    private List<QuizQuestion> randomizedQuestions;
    private int currentQuestionIndex = 0;
    private int totalQuestionCount;
    private bool answered = false;

    private float quizTimeRemaining;
    private bool quizTimerRunning = false;
    private float questionStartTime;

    public int trueCounter = 0;
    public int falseCounter = 0;
    private int totalScore = 0;
    private float totalAnswerTime = 0f;

    private const float questionCountdownDuration = 115f;
    private float questionCountdownRemaining = 115f;
    private bool questionCountdownRunning = false;

    void Start()
    {
        int questionCount = PlayerPrefs.GetInt("QuestionCount", 5); // varsay�lan 5
        int timeLimit = PlayerPrefs.GetInt("TimeLimit", 30);
        quizTimeRemaining = timeLimit * 60f;
        quizTimerRunning = true;

        StartQuiz(questionCount);
    }

    public void StartQuiz(int questionCount)
    {
        randomizedQuestions = new List<QuizQuestion>(questions.questionList);
        Shuffle(randomizedQuestions);
        totalQuestionCount = Mathf.Min(questionCount, randomizedQuestions.Count);
        currentQuestionIndex = 0;
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        if (currentQuestionIndex >= totalQuestionCount)
        {
            EndQuiz();
            PlayerPrefs.DeleteKey("QuestionCount");
            return;
        }

        answered = false;
        QuizQuestion question = randomizedQuestions[currentQuestionIndex];
        questionText.text = question.QuestionText;

        string[] options = question.Options;

        for (int i = 0; i < optionTexts.Count; i++)
        {
            optionTexts[i].text = $"{(char)('A' + i)} - {options[i]}";
            int capturedIndex = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => CheckAnswer(capturedIndex));
        }

        feedbackText.text = "";
        continueButton.gameObject.SetActive(false);

        questionStartTime = Time.time;
        questionCountdownRemaining = questionCountdownDuration;
        questionCountdownRunning = true;
    }

    private void CheckAnswer(int selectedIndex)
    {
        if (answered || !quizTimerRunning) return;

        answered = true;
        questionCountdownRunning = false;

        float timeTaken = Time.time - questionStartTime;
        totalAnswerTime += timeTaken; // Toplam s�reye bu sorudaki zaman� ekle

        QuizQuestion question = randomizedQuestions[currentQuestionIndex];
        char correct = char.ToUpper(question.CorrectAnswer);
        char selected = (char)('A' + selectedIndex);

        float timeLeft = questionCountdownDuration - timeTaken;
        int bonus = Mathf.Clamp(Mathf.FloorToInt(timeLeft / 10f), 0, 10);



        if (selected == correct)
        {
            feedbackText.text = "True!";
            trueCounter++;
            totalScore += 10 + bonus;
        }
        else
        {
            feedbackText.text = $"Wrong\nCorrect Answer: {correct}";
            falseCounter++;
            totalScore -= 5;
        }

        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(NextQuestion);
    }

    private void NextQuestion()
    {
        currentQuestionIndex++;
        ShowQuestion();
    }


    private void EndQuiz()
    {
        questionText.text = "Test is over. Total correct count: " + trueCounter + " Wrong count: " + falseCounter;
        feedbackText.text = "";

        foreach (var btn in optionButtons)
            btn.gameObject.SetActive(false);

        continueButton.gameObject.SetActive(false);

        int emptyCount = totalQuestionCount - (trueCounter + falseCounter);

        float averageTimePerQuestion = totalQuestionCount > 0 ? totalAnswerTime / totalQuestionCount : 0f;

        PlayerPrefs.SetInt("CorrectCount", trueCounter);
        PlayerPrefs.SetInt("WrongCount", falseCounter);
        PlayerPrefs.SetInt("EmptyCount", emptyCount);
        PlayerPrefs.SetInt("TotalScore", totalScore);
        PlayerPrefs.SetFloat("AverageAnswerTime", averageTimePerQuestion); 

        SceneManager.LoadScene("ResultScene");
    }


    private static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void UpdateTimerUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void UpdateCountdownUI(float time)
    {
        int seconds = Mathf.CeilToInt(time);
        countdownText.text = $"Bonus Time: {seconds} sn";
    }

    void TimeUp()
    {
        timerText.text = "00:00";
        Debug.Log("Time is over!");
        quizTimerRunning = false;

        foreach (var btn in optionButtons)
            btn.interactable = false;

        EndQuiz();
    }

    private void Update()
    {
        if (quizTimerRunning)
        {
            if (quizTimeRemaining > 0)
            {
                quizTimeRemaining -= Time.deltaTime;
                UpdateTimerUI(quizTimeRemaining);
            }
            else
            {
                quizTimeRemaining = 0;
                quizTimerRunning = false;
                TimeUp();
            }
        }

        if (questionCountdownRunning)
        {
            questionCountdownRemaining -= Time.deltaTime;
            if (questionCountdownRemaining < 0)
            {
                questionCountdownRemaining = 0;
                questionCountdownRunning = false;
            }
            UpdateCountdownUI(questionCountdownRemaining);
        }
    }
}