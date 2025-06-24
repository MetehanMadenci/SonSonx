using UnityEngine;
using UnityEngine.SceneManagement;

public class CloseButtonManager : MonoBehaviour
{
    public void OnClickClose()
    {
        SceneManager.LoadScene("Quiz_Creation");
    }
}
