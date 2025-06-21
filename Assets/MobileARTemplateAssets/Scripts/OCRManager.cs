using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using Firebase.Storage;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;

public class OCRManager : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform cropContainer; // Eklemen gerekiyor
    public RawImage previewImage;
    public RawImage cropImageView; // Kırpma için orijinal çözünürlükte görüntü gösterilecek
    public TMP_InputField questionInput, aInput, bInput, cInput, dInput, eInput;
    public GameObject selectionPanel;
    public RectTransform cropAreaUI;
    public GameObject cropPanel;
    public GameObject editPanel;

    private string functionUrl = "https://ocrfromimage-ufmxmu7iha-uc.a.run.app";
    private Texture2D originalTexture;

    void Start()
{
    if (PlayerPrefs.HasKey("LastImagePath"))
    {
        string imagePath = PlayerPrefs.GetString("LastImagePath");
        PlayerPrefs.DeleteKey("LastImagePath");

        byte[] imageData = File.ReadAllBytes(imagePath);
        originalTexture = new Texture2D(2, 2);
        originalTexture.LoadImage(imageData);

        // 🔽 Sınır: cropContainer boyutuna göre maksimumlar
        float maxWidth = cropContainer.rect.width -20;
        float maxHeight = cropContainer.rect.height -20;

        float texWidth = originalTexture.width;
        float texHeight = originalTexture.height;
        float ratio = texWidth / texHeight;

        float width = texWidth;
        float height = texHeight;

        if (width > maxWidth)
        {
            width = maxWidth;
            height = width / ratio;
        }
        if (height > maxHeight)
        {
            height = maxHeight;
            width = height * ratio;
        }

        cropImageView.texture = originalTexture;
        cropImageView.rectTransform.sizeDelta = new Vector2(width, height);
        cropImageView.rectTransform.anchoredPosition = Vector2.zero;

        ShowCropPanel(); // cropPanel açılır, editPanel kapanır
        selectionPanel.SetActive(false); // bu zaten her zaman açık olabilir, istersen silme

    }
}

    public void ShowSelectionPanel()
{
    cropPanel.SetActive(false);       // Diğer paneli kapat
    selectionPanel.SetActive(true);   // Bunu aç
}
public void ShowCropPanel()
{
    editPanel.SetActive(false);       // ❗ editPanel kapatılır
    cropPanel.SetActive(true);        // cropPanel açılır
}
public void ShowEditPanel()
{
    cropPanel.SetActive(false);       // ❗ cropPanel kapatılır
    editPanel.SetActive(true);        // editPanel açılır
}

    public void HideSelectionPanel() => selectionPanel.SetActive(false);

    public void OpenCamera()
    {
        HideSelectionPanel();
        NativeCamera.TakePicture((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                string dst = CopyToPersistent(path);
                PlayerPrefs.SetString("LastImagePath", dst);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }, maxSize: 2048);
    }

    public void OpenGallery()
    {
        HideSelectionPanel();
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                string dst = CopyToPersistent(path);
                PlayerPrefs.SetString("LastImagePath", dst);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }, "Bir soru fotoğrafı seç", "image/*");
    }

    private string CopyToPersistent(string originalPath)
    {
        string filename = Path.GetFileName(originalPath);
        string dstPath = Path.Combine(Application.persistentDataPath, filename);
        File.Copy(originalPath, dstPath, true);
        return dstPath;
    }

    public void ConfirmCropAndSend()
    {
        Texture2D cropped = CropTextureFromUI();
        cropImageView.texture = cropped;
        previewImage.texture = cropped;
        cropPanel.SetActive(false);   // kırpma paneli kapanır
        editPanel.SetActive(true);    // ✅ edit paneli açılır
        string croppedPath = Path.Combine(Application.persistentDataPath, "cropped_image.jpg");
        File.WriteAllBytes(croppedPath, cropped.EncodeToJPG());

        StartCoroutine(UploadAndOCR(croppedPath));
    }

    Texture2D CropTextureFromUI()
    {
        RectTransform imageRect = cropImageView.rectTransform;

        Vector2 imageSize = imageRect.sizeDelta;
        Vector2 cropSize = cropAreaUI.sizeDelta;
        Vector2 cropPos = cropAreaUI.anchoredPosition;

        float textureWidth = originalTexture.width;
        float textureHeight = originalTexture.height;

        float scaleX = textureWidth / imageSize.x;
        float scaleY = textureHeight / imageSize.y;

        float cropX = (cropPos.x + imageSize.x * 0.5f - cropSize.x * 0.5f) * scaleX;
        float cropY = (cropPos.y + imageSize.y * 0.5f - cropSize.y * 0.5f) * scaleY;

        int x = Mathf.Clamp(Mathf.RoundToInt(cropX), 0, originalTexture.width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(cropY), 0, originalTexture.height - 1);
        int w = Mathf.Clamp(Mathf.RoundToInt(cropSize.x * scaleX), 1, originalTexture.width - x);
        int h = Mathf.Clamp(Mathf.RoundToInt(cropSize.y * scaleY), 1, originalTexture.height - y);

        Texture2D cropped = new Texture2D(w, h);
        cropped.SetPixels(originalTexture.GetPixels(x, y, w, h));
        cropped.Apply();
        return cropped;
    }

    IEnumerator UploadAndOCR(string filePath)
    {
        FirebaseStorage storage = FirebaseStorage.DefaultInstance;
        StorageReference storageRef = storage.GetReferenceFromUrl("gs://ocrscene.firebasestorage.app");
        StorageReference imageRef = storageRef.Child("questions/" + Path.GetFileName(filePath));

        byte[] fileBytes = File.ReadAllBytes(filePath);
        var uploadTask = imageRef.PutBytesAsync(fileBytes);
        yield return new WaitUntil(() => uploadTask.IsCompleted);

        if (uploadTask.Exception != null)
        {
            Debug.LogError("Upload Error: " + uploadTask.Exception);
            yield break;
        }

        var getUrlTask = imageRef.GetDownloadUrlAsync();
        yield return new WaitUntil(() => getUrlTask.IsCompleted);

        if (getUrlTask.Exception != null)
        {
            Debug.LogError("URL Error: " + getUrlTask.Exception);
            yield break;
        }

        string imageUrl = getUrlTask.Result.ToString();
        yield return StartCoroutine(CallOCRFunction(imageUrl));
    }

    IEnumerator CallOCRFunction(string imageUrl)
    {
        string jsonBody = JsonUtility.ToJson(new ImageUrlPayload { imageUrl = imageUrl });
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(functionUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("OCR Error: " + request.error);
        }
        else
        {
            string response = request.downloadHandler.text;
            OCRResult result = JsonUtility.FromJson<OCRResult>(response);
            ParseAndDisplay(result.text);
        }
    }

    [System.Serializable]
    public class ImageUrlPayload { public string imageUrl; }
    [System.Serializable]
    public class OCRResult { public string text; }

    void ParseAndDisplay(string rawText)
{
    string[] lines = rawText.Split('\n');
    string question = "";
    string[] options = new string[5];
    int currentOptionIndex = -1;

    for (int i = 0; i < lines.Length; i++)
    {
        string line = lines[i].Trim();

        if (line.StartsWith("A)"))
        {
            currentOptionIndex = 0;
            options[0] = CleanOption(line.Substring(2).Trim());
        }
        else if (line.StartsWith("B)"))
        {
            currentOptionIndex = 1;
            options[1] = CleanOption(line.Substring(2).Trim());
        }
        else if (line.StartsWith("C)"))
        {
            currentOptionIndex = 2;
            options[2] = CleanOption(line.Substring(2).Trim());
        }
        else if (line.StartsWith("D)"))
        {
            currentOptionIndex = 3;
            options[3] = CleanOption(line.Substring(2).Trim());
        }
        else if (line.StartsWith("E)"))
        {
            currentOptionIndex = 4;
            options[4] = CleanOption(line.Substring(2).Trim());
        }
        else if (currentOptionIndex >= 0)
        {
            // Bu satır bir şıkkın devamı
            string trimmed = line.Trim();

            if (options[currentOptionIndex].EndsWith("-"))
            {
                // Tireyle birleşik devam → "-" işareti kalkar, direkt eklenir
                options[currentOptionIndex] = options[currentOptionIndex].TrimEnd('-') + trimmed;
            }
            else
            {
                // Normal devam → boşlukla ekle
                options[currentOptionIndex] += " " + trimmed;
            }
        }
        else
        {
            // ❗ Soru kısmında "-" ile bölünmüşse, tireyi yut ve boşlukla birleştir
            if (question.EndsWith("-"))
            {
                question = question.TrimEnd('-') + line;
            }
            else
            {
                question += " " + line;
            }
        }
    }

    // Sonuçları ata
    questionInput.text = question.Trim();
    aInput.text = options[0];
    bInput.text = options[1];
    cInput.text = options[2];
    dInput.text = options[3];
    eInput.text = options[4];
}


// Fazla tireleri veya satır sonu boşluklarını temizleyici (isteğe bağlı)
string CleanOption(string input)
{
    return input.Replace("\r", "").Replace("\n", "").Trim();
}
public void CancelCropPanel()
{
     ShowEditPanel();
}

// Yeniden fotoğraf seçmek için
public void ReselectImage()
{
    selectionPanel.SetActive(true);
}

}
