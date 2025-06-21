using UnityEngine;
using UnityEngine.SceneManagement;

public class OCRSceneLoader : MonoBehaviour
{
    public string sceneName = "OCRScene"; // Ge�ilecek sahne ismi

    public void LoadARScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
