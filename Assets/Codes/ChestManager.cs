using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChestManager : MonoBehaviour // S�n�f ad� ChestManager
{
    public static ChestManager Instance { get; private set; } // Instance tipi ChestManager oldu

    [Header("Sand�k UI Elementleri")]
    public GameObject photoPrefab; // PhotoThumbnail prefab
    public Transform thumbnailContainer; // K���k �nizlemelerin konulaca�� yer

    [Header("B�y�t�lm�� Foto�raf Paneli")]
    public GameObject largePhotoPanel; // B�y�k foto�raf�n g�r�nd��� panel
    public Image largePhotoDisplay; // B�y�k foto�raf�n kendisi
    public Button closeLargePhotoButton; // B�y�k foto�raf� kapatma butonu

    private List<Texture2D> photoTextures = new List<Texture2D>(); // �ekilen foto�raflar� saklar

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Bu manager sahne ge�i�lerinde kalmal�
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (largePhotoPanel != null)
        {
            largePhotoPanel.SetActive(false); // Ba�lang��ta b�y�k foto�raf paneli kapal� olsun
        }
        if (closeLargePhotoButton != null)
        {
            closeLargePhotoButton.onClick.AddListener(HideLargePhoto); // Kapatma butonuna i�levi ba�la
        }

        // Final Sahnesinde de UI'� g�ncellemek i�in bu sat�r gereklidir.
        // ��nk� sahne de�i�ti�inde UI elementleri tekrar olu�turulur ve yeniden ba�lanmas� gerekir.
        UpdateChestUI(); // BU SATIRI EKL�YORUZ! 
    }

    public void AddPhoto(Texture2D newPhotoTexture)
    {
        photoTextures.Add(newPhotoTexture); // Yeni foto�raf� listeye ekle
        UpdateChestUI(); // UI'� g�ncelle
    }

    void UpdateChestUI() // UI'� g�ncelleyen metot
    {
        // Konteynerdeki eski foto�raflar� sil
        foreach (Transform child in thumbnailContainer)
        {
            Destroy(child.gameObject);
        }

        // Her foto�raf i�in yeni bir �nizleme olu�tur
        foreach (Texture2D texture in photoTextures)
        {
            GameObject photoGO = Instantiate(photoPrefab, thumbnailContainer); // Prefab'i olu�tur
            Image photoImage = photoGO.GetComponent<Image>(); // Image bile�enini al

            if (photoImage != null)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                photoImage.sprite = sprite; // Sprite'� ata
                photoImage.SetNativeSize();
                // Burada SetNativeSize() veya boyut ayarlamas�n� tekrar eklemek isteyebilirsiniz.
                // E�er Investigation Scene'de boyutlar do�ruysa, buraya da ekleyin:
                // photoImage.SetNativeSize(); 
            }

            Button photoButton = photoGO.GetComponent<Button>(); // Button bile�enini al
            if (photoButton != null)
            {
                photoButton.onClick.RemoveAllListeners(); // �nceki dinleyicileri temizle
                photoButton.onClick.AddListener(() => ShowLargePhoto(texture)); // T�klay�nca b�y�k foto�raf� g�ster
            }
        }
    }

    void ShowLargePhoto(Texture2D texture) // B�y�k foto�raf� g�steren metot
    {
        if (largePhotoPanel != null && largePhotoDisplay != null)
        {
            largePhotoPanel.SetActive(true); // Paneli aktif yap
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            largePhotoDisplay.sprite = sprite; // B�y�k foto�raf� ata
            // Burada da SetNativeSize() veya boyut ayarlamas�n� eklemek isteyebilirsiniz.
            // largePhotoDisplay.SetNativeSize(); 
        }
    }

    public void HideLargePhoto() // B�y�k foto�raf� gizleyen metot
    {
        if (largePhotoPanel != null)
        {
            largePhotoPanel.SetActive(false); // Paneli pasif yap
        }
    }

    public List<Texture2D> GetCollectedPhotos() // Toplanan foto�raflar� d�nd�ren metot
    {
        return photoTextures;
    }
}
