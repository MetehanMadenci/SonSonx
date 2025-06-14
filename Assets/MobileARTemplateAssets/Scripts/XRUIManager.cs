using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class XRUIManager : MonoBehaviour
{

    private bool isFadeActive = false;
    private bool isHideActive = false;

    public Transform screenCenterTarget;
    private Dictionary<string, BoneInfo> boneInfoLookup;
    public GameObject rightPanel;
    public GameObject middlePanel;

    public TextMeshProUGUI boneDescriptionTextInScroll;
    public TextMeshProUGUI boneNameText;

    public GameObject skeletonRoot;
    public Camera arCamera;

    public Button fadeButton;
    public Button hideButton;

    public Color normalColor = new Color(0.5f, 0.9f, 1f);
    public Color activeColor = new Color32(100, 180, 255, 80); 

    public GameObject selectedBone;
    private Material selectedMaterial;

    private bool isFaded = false;
    private bool isHidden = false;
    private bool isPanelManuallyHidden = false;
    private GameObject lastBoneForWhichPanelWasHidden = null;

    public static XRUIManager Instance;
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BoneInfo[] allBones = Resources.LoadAll<BoneInfo>("Bones");

        boneInfoLookup = new Dictionary<string, BoneInfo>();
        foreach (var bone in allBones)
        {
            if (!boneInfoLookup.ContainsKey(bone.boneModelName))
                boneInfoLookup.Add(bone.boneModelName, bone);
        }
    }

    public void DisplayBoneInfo(string selectedBoneName)
    {
        if (boneInfoLookup.TryGetValue(selectedBoneName, out BoneInfo boneInfo))
        {
            boneNameText.text = boneInfo.boneName;
            boneDescriptionTextInScroll.text = boneInfo.boneDescription;
        }
        else
        {
            boneNameText.text = "Bilinmiyor";
            boneDescriptionTextInScroll.text = "Açıklama bulunamadı.";
        }

        rightPanel.SetActive(true);
        middlePanel.SetActive(true);
        isRightPanelVisible = true;
        isMiddlePanelVisible = true;
    }

    

    public void ShowBoneInfo(string boneName)
    {
        boneNameText.text = boneName;

        if (boneInfoLookup.ContainsKey(boneName))
            boneDescriptionTextInScroll.text = boneInfoLookup[boneName].boneDescription;
        else
            boneDescriptionTextInScroll.text = "Bu kemiğe ait açıklama bulunamadı.";

        rightPanel.SetActive(true);
        middlePanel.SetActive(true);

        isRightPanelVisible = true;
        isMiddlePanelVisible = true;
    }

    public bool isRightPanelVisible = false;
    public void ToggleRightPanel()
    {
        isRightPanelVisible = !isRightPanelVisible;
        rightPanel.SetActive(isRightPanelVisible);
    }

    public bool isMiddlePanelVisible = false;
    public void ToggleMiddlePanel()
    {
        isMiddlePanelVisible = !isMiddlePanelVisible;
        middlePanel.SetActive(isMiddlePanelVisible);
    }


    public void OnHideButton()
    {
        if (selectedBone == null) return;

        isHidden = !isHidden;
        selectedBone.SetActive(!isHidden);

        // Update button color
        UpdateButtonColors();
    }

    public void OnFadeButton()
    {
        if (selectedBone == null) return;

        isFaded = !isFaded;

        Renderer[] renderers = selectedBone.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Color color = materials[i].color;
                if (isFaded)
                {
                    color.a = 0.2f;
                    EnableTransparency(materials[i]);
                }
                else
                {
                    color.a = 1f;
                }
                materials[i].color = color;
            }
        }

        // Update button color
        UpdateButtonColors();
    }

    private void UpdateButtonColors()
    {
        // Update fade button color
        ColorBlock fadeColors = fadeButton.colors;
        fadeColors.normalColor = isFaded ? activeColor : normalColor;
        fadeColors.selectedColor = isFaded ? activeColor : normalColor;
        fadeColors.highlightedColor = isFaded ? activeColor : normalColor;
        fadeColors.pressedColor = isFaded ? activeColor : normalColor;
        fadeButton.colors = fadeColors;

        // Update hide button color
        ColorBlock hideColors = hideButton.colors;
        hideColors.normalColor = isHidden ? activeColor : normalColor;
        hideColors.selectedColor = isHidden ? activeColor : normalColor;
        hideColors.highlightedColor = isHidden ? activeColor : normalColor;
        hideColors.pressedColor = isHidden ? activeColor : normalColor;
        hideButton.colors = hideColors;
    }

public void SetSelectedBone(GameObject boneObj, Material mat, float isolateDistance)
    {
        selectedBone = boneObj;
        selectedMaterial = mat;

        originalMaterials.Clear();

        Renderer[] renderers = boneObj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            Material[] matsCopy = new Material[r.materials.Length];
            for (int i = 0; i < r.materials.Length; i++)
            {
                matsCopy[i] = new Material(r.materials[i]);
            }
            originalMaterials[r] = matsCopy;
        }

        isFaded = false;
        isHidden = false;
        isPanelManuallyHidden = false;
        lastBoneForWhichPanelWasHidden = null;
        fadeButton.interactable = true;
        hideButton.interactable = true;
        UpdateButtonColors();
    }

    private void EnableTransparency(Material mat)
    {
        if (mat == null) return;

        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    public void SlidePanel(GameObject panel, Vector2 hiddenPos, Vector2 shownPos, float duration = 0.4f)
    {
        StartCoroutine(SlideCoroutine(panel.GetComponent<RectTransform>(), hiddenPos, shownPos, duration));
    }

    private IEnumerator SlideCoroutine(RectTransform panelRect, Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        panelRect.anchoredPosition = from;
        panelRect.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            panelRect.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }

        panelRect.anchoredPosition = to;
    }
    public void OnHideOthersStateChanged(bool state)
{
    // You can add any UI updates needed when hide others state changes
}
public void HidePanels()
{
    rightPanel.SetActive(false);
    middlePanel.SetActive(false);
    isRightPanelVisible = false;
    isMiddlePanelVisible = false;
}

}