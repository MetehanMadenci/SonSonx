using UnityEngine;
using UnityEngine.SceneManagement;

public class ARSceneLoader : MonoBehaviour
{
    public string sceneName = "ARScene"; // Ge�ilecek sahne ismi

    public void LoadARScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
