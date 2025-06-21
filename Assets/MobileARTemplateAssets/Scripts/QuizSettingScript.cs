using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class QuizSettingScript : MonoBehaviour
{
    public TMP_Dropdown numberOfQuestionsDropdown;
    public TMP_Dropdown minutesDropdown;

    private int selectedNumberOfQuestions;
    private int selectedMinutes;

    private const string QUESTIONS_KEY = "QuestionCount";
    private const string MINUTES_KEY = "TimeLimit";


    void Start()
    {

        if (numberOfQuestionsDropdown != null)
        {
            //SetDropdownValue(numberOfQuestionsDropdown, selectedNumberOfQuestions);

            numberOfQuestionsDropdown.onValueChanged.AddListener(delegate {
                OnNumberOfQuestionsDropdownChanged(numberOfQuestionsDropdown);
            });
        }

        if (minutesDropdown != null)
        {
            //SetDropdownValue(minutesDropdown, selectedMinutes);

            minutesDropdown.onValueChanged.AddListener(delegate {
                OnMinutesDropdownChanged(minutesDropdown);
            });
        }
    }

    private void SetDropdownValue(TMP_Dropdown dropdown, int valueToSet)
    {
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (int.TryParse(dropdown.options[i].text, out int optionValue) && optionValue == valueToSet)
            {
                dropdown.value = i;
                return;
            }
        }
        Debug.LogWarning($"Dropdown'da {valueToSet} deðeri bulunamadý. Varsayýlan deðer kullanýlacak.");
        dropdown.value = 0;
    }

    public void OnNumberOfQuestionsDropdownChanged(TMP_Dropdown dropdown)
    {
        string selectedOptionText = dropdown.options[dropdown.value].text;

        if (int.TryParse(selectedOptionText, out selectedNumberOfQuestions))
        {
            Debug.Log("Seçilen Soru Sayýsý: " + selectedNumberOfQuestions);
            PlayerPrefs.SetInt(QUESTIONS_KEY, selectedNumberOfQuestions);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError("Soru sayýsý dropdown'ýndan geçerli bir sayý alýnamadý: " + selectedOptionText);
        }
    }

    public void OnMinutesDropdownChanged(TMP_Dropdown dropdown)
    {
        string selectedOptionText = dropdown.options[dropdown.value].text;

        if (int.TryParse(selectedOptionText, out selectedMinutes))
        {
            Debug.Log("Seçilen Dakika: " + selectedMinutes);
            PlayerPrefs.SetInt(MINUTES_KEY, selectedMinutes);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError("Dakika dropdown'ýndan geçerli bir sayý alýnamadý: " + selectedOptionText);
        }
    }

    public void ChangeScene(string SceneName)
    {
        Debug.Log("START Butonuna Basýldý!");
        Debug.Log("Kaydedilen Soru Sayýsý: " + selectedNumberOfQuestions);
        Debug.Log("Kaydedilen Dakika: " + selectedMinutes);

        SceneManager.LoadScene(SceneName);
    }

    //public int GetSelectedNumberOfQuestions()
    //{
    //    return selectedNumberOfQuestions;
    //}

    //public int GetSelectedMinutes()
    //{
    //    return selectedMinutes;
    //}

    [ContextMenu("Clear All PlayerPrefs")]
    private void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Tüm PlayerPrefs verileri silindi.");
        selectedNumberOfQuestions = 1;
        selectedMinutes = 1;
        //if (numberOfQuestionsDropdown != null) SetDropdownValue(numberOfQuestionsDropdown, selectedNumberOfQuestions);
        //if (minutesDropdown != null) SetDropdownValue(minutesDropdown, selectedMinutes);
    }
}