using UnityEngine;
using UnityEngine.SceneManagement;

public class ARSceneLoader : MonoBehaviour
{
    public string sceneName = "ARScene"; 

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
