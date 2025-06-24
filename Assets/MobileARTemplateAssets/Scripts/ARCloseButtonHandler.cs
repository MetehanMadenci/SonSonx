using UnityEngine;
using UnityEngine.SceneManagement;

public class ARCloseButtonHandler : MonoBehaviour
{
    public string targetSceneName = "HomeScene"; 

    public void CloseARScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}
