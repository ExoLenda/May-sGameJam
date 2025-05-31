using UnityEngine;
using System.Collections; // Coroutine'ler için
using TMPro; // UI metni için
using UnityEngine.UI; // Butonu devre dýþý býrakmak için

public class ScreenshotManager : MonoBehaviour
{
    public static ScreenshotManager Instance { get; private set; } // Singleton tanýmý

    [Header("Fotoðraf Çekme Ayarlarý")]
    public int maxPhotos = 7; // Çekilebilecek maksimum fotoðraf sayýsý
    private int currentPhotosTaken = 0; // Þu ana kadar çekilen fotoðraf sayýsý

    public TextMeshProUGUI photoCountText; // Fotoðraf sayýsýný gösterecek UI metni
    public Button photoButton; // Fotoðraf çekme butonu (hakký bitince pasifleþtirmek için)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdatePhotoCountUI(); // Oyun baþladýðýnda sayacý güncelle
    }

    public void TakeScreenshot()
    {
        if (currentPhotosTaken < maxPhotos)
        {
            // Ekranýn bir sonraki karede render edilmesini bekle, sonra ekran görüntüsü al
            StartCoroutine(CaptureScreenshotAndAdd());
        }
        else
        {
            Debug.LogWarning("Daha fazla fotoðraf çekme hakkýnýz yok!");
            // Kullanýcýya hakkýnýn bittiðini bildiren bir UI mesajý gösterebilirsiniz.
        }
    }

    IEnumerator CaptureScreenshotAndAdd()
    {
        yield return new WaitForEndOfFrame(); // Bir kare bekle

        Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenTexture.Apply();

        Debug.Log("Fotoðraf çekildi!");
        // Çekilen Texture2D'yi doðrudan ChestManager'a iletiyoruz
        ChestManager.Instance.AddPhoto(screenTexture);

        currentPhotosTaken++;
        UpdatePhotoCountUI();

        // Eðer fotoðraf çekme hakký bittiyse butonu pasifleþtir
        if (currentPhotosTaken >= maxPhotos && photoButton != null)
        {
            photoButton.interactable = false; // Butonu etkileþime kapat
        }
    }

    void UpdatePhotoCountUI()
    {
        if (photoCountText != null)
        {
            photoCountText.text = "Kalan Fotoðraf: " + (maxPhotos - currentPhotosTaken) + "/" + maxPhotos;
        }
    }
}
