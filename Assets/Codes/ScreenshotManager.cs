using UnityEngine;
using System.Collections; // Coroutine'ler i�in
using TMPro; // UI metni i�in
using UnityEngine.UI; // Butonu devre d��� b�rakmak i�in

public class ScreenshotManager : MonoBehaviour
{
    public static ScreenshotManager Instance { get; private set; } // Singleton tan�m�

    [Header("Foto�raf �ekme Ayarlar�")]
    public int maxPhotos = 7; // �ekilebilecek maksimum foto�raf say�s�
    private int currentPhotosTaken = 0; // �u ana kadar �ekilen foto�raf say�s�

    public TextMeshProUGUI photoCountText; // Foto�raf say�s�n� g�sterecek UI metni
    public Button photoButton; // Foto�raf �ekme butonu (hakk� bitince pasifle�tirmek i�in)

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
        UpdatePhotoCountUI(); // Oyun ba�lad���nda sayac� g�ncelle
    }

    public void TakeScreenshot()
    {
        if (currentPhotosTaken < maxPhotos)
        {
            // Ekran�n bir sonraki karede render edilmesini bekle, sonra ekran g�r�nt�s� al
            StartCoroutine(CaptureScreenshotAndAdd());
        }
        else
        {
            Debug.LogWarning("Daha fazla foto�raf �ekme hakk�n�z yok!");
            // Kullan�c�ya hakk�n�n bitti�ini bildiren bir UI mesaj� g�sterebilirsiniz.
        }
    }

    IEnumerator CaptureScreenshotAndAdd()
    {
        yield return new WaitForEndOfFrame(); // Bir kare bekle

        Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenTexture.Apply();

        Debug.Log("Foto�raf �ekildi!");
        // �ekilen Texture2D'yi do�rudan ChestManager'a iletiyoruz
        ChestManager.Instance.AddPhoto(screenTexture);

        currentPhotosTaken++;
        UpdatePhotoCountUI();

        // E�er foto�raf �ekme hakk� bittiyse butonu pasifle�tir
        if (currentPhotosTaken >= maxPhotos && photoButton != null)
        {
            photoButton.interactable = false; // Butonu etkile�ime kapat
        }
    }

    void UpdatePhotoCountUI()
    {
        if (photoCountText != null)
        {
            photoCountText.text = "Kalan Foto�raf: " + (maxPhotos - currentPhotosTaken) + "/" + maxPhotos;
        }
    }
}
