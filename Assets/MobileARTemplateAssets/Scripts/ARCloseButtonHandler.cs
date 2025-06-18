using UnityEngine;
using UnityEngine.SceneManagement;

public class ARCloseButtonHandler : MonoBehaviour
{
    public string targetSceneName = "HomeScene"; // Gidilecek sahnenin adı

    public void CloseARScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}
