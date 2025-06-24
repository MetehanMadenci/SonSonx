using UnityEngine;
using UnityEngine.SceneManagement;

public class OCRSceneLoader : MonoBehaviour
{
    public string sceneName = "OCRScene";

    public void LoadARScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
