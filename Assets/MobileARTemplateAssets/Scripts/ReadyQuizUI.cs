using UnityEngine;

public class ReadyQuizUI : MonoBehaviour
{
    public QuizListLoader quizListLoader;
    public GameObject quizItemPrefab;
    public Transform quizListParent;
    public QuizRunner quizRunner;

    void Start()
    {
        quizListLoader.LoadAllQuizLists((allQuizzes) =>
        {
            foreach (var quiz in allQuizzes)
            {
                GameObject item = Instantiate(quizItemPrefab, quizListParent);
                item.GetComponent<QuizListItemUI>().Setup(quiz, (selectedQuiz) =>
                {
                    quizRunner.StartQuizFromList(selectedQuiz);
                });
            }
        });
    }
}