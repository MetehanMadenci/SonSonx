using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaitingRoomScript : MonoBehaviour
{
    [SerializeField] private GameObject memberItemPrefab;
    [SerializeField] private Transform contentParent;

    [SerializeField] private TMP_Text buttonText;
    private bool isReady = false;


    public void UpdateMemberList(List<string> userNames)
    {
        // �nce eski elemanlar� temizle
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Yeni kullan�c� isimlerini olu�tur
        foreach (string name in userNames)
        {
            GameObject item = Instantiate(memberItemPrefab, contentParent);
            item.GetComponent<TMP_Text>().text = name;
        }
    }

    void Start()
    {
        List<string> fakeUsers = new List<string> { "Oktay", "Ay�e", "Ali", "Zeynep", "Burak", "Merve", "Yusuf", "Ece", "Tuna", "Arda" };
        UpdateMemberList(fakeUsers);
    }


    public void OnReadyButtonClicked()
    {
        isReady = !isReady;

        if (isReady)
        {
            buttonText.text = "Ready!";
            buttonText.fontStyle = FontStyles.Bold;
        }
        else
        {
            buttonText.text = "Ready?";
            buttonText.fontStyle = FontStyles.Normal;
        }
    }
}
