using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonHandler : MonoBehaviour
{
    public void LoadQuizCreationScene()
    {
        SceneManager.LoadScene("Quiz_Creation");
    }
}
