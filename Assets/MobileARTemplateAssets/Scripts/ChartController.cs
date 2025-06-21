using UnityEngine;
using UnityEngine.UI;

public class ChartController : MonoBehaviour
{
    [SerializeField] private Image correctSlice;
    [SerializeField] private Image wrongSlice;
    [SerializeField] private Image emptySlice;
    [SerializeField] private Text correctAnswers;
    [SerializeField] private Text incorrectAnswers;
    [SerializeField] private Text score;
    [SerializeField] private Text totalScore;
    [SerializeField] private Text averageTime;

    void Start()
    {
        int correct = PlayerPrefs.GetInt("CorrectCount", 0);
        int wrong = PlayerPrefs.GetInt("WrongCount", 0);
        int empty = PlayerPrefs.GetInt("EmptyCount", 0);
        int scoreVal = PlayerPrefs.GetInt("TotalScore", 0);
        float avgTime = PlayerPrefs.GetFloat("AverageAnswerTime", 0f);
        int total = correct + wrong + empty;

        correctAnswers.text = $"Correct answers: {correct}";
        incorrectAnswers.text = $"Incorrect answers: {wrong}";

        float scorePercent = (total == 0) ? 0 : (float)correct / total * 100f;
        score.text = $"Accuracy: {scorePercent:0.0}%";
        totalScore.text = $"Score: {scoreVal}";
        averageTime.text = $"Avg. Time per Question: {avgTime:0.0} sec";

        SetChart(correct, wrong, empty);
    }

    public void SetChart(int correct, int wrong, int empty)
    {
        int total = correct + wrong + empty;
        if (total == 0) total = 1;

        float correctPercent = (float)correct / total;
        float wrongPercent = (float)wrong / total;
        float emptyPercent = (float)empty / total;

        correctSlice.fillAmount = correctPercent;
        wrongSlice.fillAmount = correctPercent + wrongPercent;
        emptySlice.fillAmount = correctPercent + wrongPercent + emptyPercent;
    }
}
