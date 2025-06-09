using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class XRUIManager : MonoBehaviour
{

    public Transform screenCenterTarget;
    private Dictionary<string, BoneInfo> boneInfoLookup;
    public GameObject rightPanel;
    public GameObject middlePanel;

    public TextMeshProUGUI boneDescriptionTextInScroll;

    public TextMeshProUGUI boneNameText;


    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;


    public GameObject skeletonRoot;
    public Camera arCamera;

    public Button hideOthersButton;
    public Button fadeButton;
    public Button hideButton;

    public Color normalColor = new Color(0.5f, 0.9f, 1f);
    public Color activeColor = new Color(0f, 0f, 0.8f);

    public GameObject selectedBone;
    private Material selectedMaterial;

    private Vector3 savedBoneWorldPosition;
    private Quaternion savedBoneWorldRotation;
    private Vector3 savedModelWorldPosition;
    private Quaternion savedModelWorldRotation;

    private Vector3 initialModelPosition;
    private Quaternion initialModelRotation;

    private bool isFaded = false;
    private bool isHidden = false;
    public bool othersHidden = false;
    private bool isPanelManuallyHidden = false;
    private GameObject lastBoneForWhichPanelWasHidden = null;
    public static XRUIManager Instance;
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    // Yeni global değişkenler (UIManager.cs'de en üste)
    private Quaternion isolatedPivotInitialRotation;
    private Vector3 isolatedPivotInitialPosition;
    private Vector3 isolatedBoneInitialPosition;
    private Quaternion isolatedBoneInitialRotation;

    private Vector3 fullModelInitialPosition;
    private Quaternion fullModelInitialRotation;
    private Dictionary<Transform, Vector3> boneInitialPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Quaternion> boneInitialRotations = new Dictionary<Transform, Quaternion>();
    private bool initialStateCaptured = false;

    void Awake()
    {
        Instance = this;
    }

    private Dictionary<string, string> boneDescriptions = new Dictionary<string, string>()
    {
        { "Vertebra T9", "Göğüs omurlarından biridir." },
        { "Femur", "Uyluk kemiği, vücuttaki en uzun kemiktir." }
    };

    void Start()
    {
        if (!initialStateCaptured && skeletonRoot != null)
        {
            fullModelInitialPosition = skeletonRoot.transform.position;
            fullModelInitialRotation = skeletonRoot.transform.rotation;

            foreach (Transform bone in skeletonRoot.GetComponentsInChildren<Transform>(true))
            {
                boneInitialPositions[bone] = bone.position;
                boneInitialRotations[bone] = bone.rotation;
            }

            initialStateCaptured = true;
        }

        BoneInfo[] allBones = Resources.LoadAll<BoneInfo>("Bones");

        boneInfoLookup = new Dictionary<string, BoneInfo>();
        foreach (var bone in allBones)
        {
            if (!boneInfoLookup.ContainsKey(bone.boneModelName))
                boneInfoLookup.Add(bone.boneModelName, bone);
        }

        if (skeletonRoot != null)
        {
            initialModelPosition = skeletonRoot.transform.position;
            initialModelRotation = skeletonRoot.transform.rotation;
        }

        if (Camera.main != null)
        {
            initialCameraPosition = Camera.main.transform.position;
            initialCameraRotation = Camera.main.transform.rotation;
        }
    }





    public Vector3 GetModelBoundsCenter()
    {
        Renderer[] renderers = skeletonRoot.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return skeletonRoot.transform.position;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer rend in renderers)
            bounds.Encapsulate(rend.bounds);

        return bounds.center;
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





    public void SetSelectedBone(GameObject boneObj, Material mat, float isolateDistance)
    {
        selectedBone = boneObj;
        selectedMaterial = mat;

        // Malzemeleri yedekle
        originalMaterials.Clear();

        Renderer[] renderers = boneObj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            // Malzemeleri tek tek kopyalayarak sakla
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
        hideOthersButton.interactable = true;
        fadeButton.interactable = true;
        hideButton.interactable = true;
    }

    public void ShowBoneInfo(string boneName)
    {
        boneNameText.text = boneName;

        if (boneDescriptions.ContainsKey(boneName))
            boneDescriptionTextInScroll.text = boneDescriptions[boneName];
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

        isHidden = !selectedBone.activeSelf;
        selectedBone.SetActive(isHidden);

        var colorCtrl = hideButton.GetComponent<ButtonColorController>();
        if (isHidden)
            colorCtrl?.SetActiveState();
        else
            colorCtrl?.ResetToNormalState();
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

        var colorCtrl = fadeButton.GetComponent<ButtonColorController>();
        if (isFaded)
            colorCtrl?.SetActiveState();
        else
            colorCtrl?.ResetToNormalState();
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

   

    private Vector3 modelHiddenPosition = new Vector3(9999, 9999, 9999); // gizlemek için
    private Vector3 modelOriginalPosition;
    private Quaternion modelOriginalRotation;

    public void OnHideOthersButton()
    {
        if (selectedBone == null || skeletonRoot == null) return;

        var colorCtrl = hideOthersButton.GetComponent<ButtonColorController>();

        if (!othersHidden)
        {
            othersHidden = true;
            SetButtonColor(hideOthersButton, activeColor);
            colorCtrl?.SetActiveState();

            //  KEMİK VE MODEL ROTASYONUNU PİVOT OLUŞTURMADAN ÖNCE KAYDET
            SaveTransforms(selectedBone.transform);

            modelOriginalPosition = skeletonRoot.transform.position;
            modelOriginalRotation = skeletonRoot.transform.rotation;

            if (selectedBone.name != "IsolatedBonePivot")
            {
                Transform boneRef = selectedBone.transform;

                //  Kemiğin orijinal poz/rot bilgisini kaydet
                isolatedBoneInitialPosition = boneRef.position;
                isolatedBoneInitialRotation = boneRef.rotation;

                Vector3 pivotPos = GetRendererBoundsCenter(boneRef.gameObject);
                GameObject pivotWrapper = new GameObject("IsolatedBonePivot");

                pivotWrapper.transform.position = pivotPos;
                pivotWrapper.transform.rotation = Quaternion.LookRotation(arCamera.transform.forward);
                pivotWrapper.transform.SetParent(null);

                boneRef.SetParent(pivotWrapper.transform);

                if (!pivotWrapper.TryGetComponent(out DragRotate rotator))
                    pivotWrapper.AddComponent<DragRotate>();

                selectedBone = pivotWrapper;
            }



            // Kemikleri gizle
            Transform[] allBones = skeletonRoot.GetComponentsInChildren<Transform>(true);
            foreach (Transform bone in allBones)
            {
                if (bone == skeletonRoot.transform) continue;
                bone.gameObject.SetActive(false);
            }

            selectedBone.gameObject.SetActive(true);
            foreach (Transform child in selectedBone.transform)
                child.gameObject.SetActive(true);

            // Modeli sahnede uzağa taşı
            skeletonRoot.transform.position = modelHiddenPosition;
        }
        else
        {
            othersHidden = false;
            SetButtonColor(hideOthersButton, normalColor);
            colorCtrl?.ResetToNormalState();

            skeletonRoot.transform.position = modelOriginalPosition;
            skeletonRoot.transform.rotation = modelOriginalRotation;

            Transform[] allBones = skeletonRoot.GetComponentsInChildren<Transform>(true);
            foreach (Transform bone in allBones)
            {
                if (bone == skeletonRoot.transform) continue;
                bone.gameObject.SetActive(true);
            }

            RestoreBoneAndModelToSavedPose();
        }
    }



    private Vector3 GetRendererBoundsCenter(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);

        List<Renderer> validRenderers = new List<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r != null && r.enabled && r.gameObject.activeInHierarchy)
                validRenderers.Add(r);
        }

        if (validRenderers.Count == 0)
        {
            Debug.LogWarning(" GetRendererBoundsCenter: No valid renderers found on " + go.name);
            return go.transform.position;
        }

        Bounds bounds = validRenderers[0].bounds;
        for (int i = 1; i < validRenderers.Count; i++)
        {
            bounds.Encapsulate(validRenderers[i].bounds);
        }

        return bounds.center;
    }


    private Vector3 GetZoomTargetCenter()
    {
        GameObject bone = UIManager.Instance.selectedBone;

        if (UIManager.Instance.othersHidden && bone != null)
        {
            if (bone.name == "IsolatedBonePivot" && bone.transform.childCount > 0)
            {
                GameObject actualBone = bone.transform.GetChild(0).gameObject;
                return GetRendererBoundsCenter(actualBone);
            }
            else
            {
                return GetRendererBoundsCenter(bone);
            }
        }
        else
        {
            return UIManager.Instance.GetModelBoundsCenter();
        }
    }


    private void SaveTransforms(Transform boneRef)
    {
        savedBoneWorldPosition = boneRef.position;
        savedBoneWorldRotation = boneRef.rotation;
        savedModelWorldPosition = skeletonRoot.transform.position;
        savedModelWorldRotation = skeletonRoot.transform.rotation;

        Debug.Log(" Kayıtlı Kemik: Pos=" + savedBoneWorldPosition + " | Rot=" + savedBoneWorldRotation.eulerAngles);
    }

    private void RestoreBoneAndModelToSavedPose()
    {
        if (selectedBone.name == "IsolatedBonePivot")
        {
            Transform originalBone = selectedBone.transform.GetChild(0);

            //  Pivotu resetle (zoom/rotate etkisi gider)
            selectedBone.transform.SetPositionAndRotation(isolatedPivotInitialPosition, isolatedPivotInitialRotation);

            // Kemiği çıkart, model ve kemik dönüşünü sıfırla
            originalBone.SetParent(null);
            skeletonRoot.transform.SetPositionAndRotation(savedModelWorldPosition, savedModelWorldRotation);
            originalBone.SetPositionAndRotation(savedBoneWorldPosition, savedBoneWorldRotation);
            originalBone.SetParent(skeletonRoot.transform);

            Destroy(selectedBone);
            selectedBone = originalBone.gameObject;
        }


        else
        {
            selectedBone.transform.SetParent(null);
            skeletonRoot.transform.SetPositionAndRotation(savedModelWorldPosition, savedModelWorldRotation);
            selectedBone.transform.SetPositionAndRotation(savedBoneWorldPosition, savedBoneWorldRotation);
            selectedBone.transform.SetParent(skeletonRoot.transform);
        }
    }




    private IEnumerator DisableAllButtonsForSeconds(float duration)
    {
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (var btn in allButtons)
            btn.interactable = false;

        yield return new WaitForSeconds(duration);

        foreach (var btn in allButtons)
            btn.interactable = true;
    }

    private void SetButtonColor(Button btn, Color color)
    {
        if (btn != null && btn.targetGraphic != null)
            btn.targetGraphic.color = color;
    }

    private IEnumerator MoveModelToCameraView(Transform model, Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Vector3 startPos = model.position;
        Quaternion startRot = model.rotation;
        float time = 0f;

        while (time < duration)
        {
            model.position = Vector3.Lerp(startPos, targetPos, time / duration);
            model.rotation = Quaternion.Slerp(startRot, targetRot, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        model.position = targetPos;
        model.rotation = targetRot;
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
}