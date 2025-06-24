using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject readyQuizPanel;
    public GameObject quizUI;
    public GameObject createQuizPanel;

    public void ShowReadyQuizPanel()
    {
        readyQuizPanel.SetActive(true);
        quizUI.SetActive(false);
        createQuizPanel.SetActive(false);
    }


public void ShowQuizUI()
{
    readyQuizPanel.SetActive(false);
    createQuizPanel.SetActive(false); // 🔻 Burası önemli
    quizUI.SetActive(true);           // 🔺 Sadece bu açık kalmalı
}

    
}
