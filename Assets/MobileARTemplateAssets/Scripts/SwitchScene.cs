using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public void ChangeScene (string sceneName)
    {
        SceneManager.LoadScene (sceneName);
    }
}
