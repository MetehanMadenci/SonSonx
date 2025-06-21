using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JoinRoomManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TMP_InputField userNameInput;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private CanvasGroup warningCanvasGroup;

    public void OnJoinRoomButtonClicked()
    {
        string roomCode = roomCodeInput.text.Trim();
        string userName = userNameInput.text.Trim();

        if (roomCode.Length != 6)
        {
            ShowWarning("Check your code!");
            return;
        }

        if (string.IsNullOrEmpty(userName))
        {
            ShowWarning("Enter a nickname!");
            return;
        }

        
        Debug.Log("Giriþ baþarýlý: " + roomCode + " - " + userName);
        //PlayerPrefs.SetString("RoomCode", roomCode);
        //PlayerPrefs.SetString("UserName", userName);
        //SceneManager.LoadScene("WaitingRoom");
    }

    void ShowWarning(string message)
    {
        StopAllCoroutines();
        warningText.text = message;
        StartCoroutine(FadeWarning());
    }

    IEnumerator FadeWarning()
    {
        warningPanel.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(warningCanvasGroup, 0f, 1f, 0.3f)); // Fade In
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeCanvasGroup(warningCanvasGroup, 1f, 0f, 0.5f)); // Fade Out
        warningPanel.SetActive(false);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        cg.alpha = end;
    }
}

