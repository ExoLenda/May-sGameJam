using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChestManager : MonoBehaviour // Sýnýf adý ChestManager
{
    public static ChestManager Instance { get; private set; } // Instance tipi ChestManager oldu

    [Header("Sandýk UI Elementleri")]
    public GameObject photoPrefab; // PhotoThumbnail prefab
    public Transform thumbnailContainer; // Küçük önizlemelerin konulacaðý yer

    [Header("Büyütülmüþ Fotoðraf Paneli")]
    public GameObject largePhotoPanel; // Büyük fotoðrafýn göründüðü panel
    public Image largePhotoDisplay; // Büyük fotoðrafýn kendisi
    public Button closeLargePhotoButton; // Büyük fotoðrafý kapatma butonu

    private List<Texture2D> photoTextures = new List<Texture2D>(); // Çekilen fotoðraflarý saklar

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Bu manager sahne geçiþlerinde kalmalý
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
            largePhotoPanel.SetActive(false); // Baþlangýçta büyük fotoðraf paneli kapalý olsun
        }
        if (closeLargePhotoButton != null)
        {
            closeLargePhotoButton.onClick.AddListener(HideLargePhoto); // Kapatma butonuna iþlevi baðla
        }

        // Final Sahnesinde de UI'ý güncellemek için bu satýr gereklidir.
        // Çünkü sahne deðiþtiðinde UI elementleri tekrar oluþturulur ve yeniden baðlanmasý gerekir.
        UpdateChestUI(); // BU SATIRI EKLÝYORUZ! 
    }

    public void AddPhoto(Texture2D newPhotoTexture)
    {
        photoTextures.Add(newPhotoTexture); // Yeni fotoðrafý listeye ekle
        UpdateChestUI(); // UI'ý güncelle
    }

    void UpdateChestUI() // UI'ý güncelleyen metot
    {
        // Konteynerdeki eski fotoðraflarý sil
        foreach (Transform child in thumbnailContainer)
        {
            Destroy(child.gameObject);
        }

        // Her fotoðraf için yeni bir önizleme oluþtur
        foreach (Texture2D texture in photoTextures)
        {
            GameObject photoGO = Instantiate(photoPrefab, thumbnailContainer); // Prefab'i oluþtur
            Image photoImage = photoGO.GetComponent<Image>(); // Image bileþenini al

            if (photoImage != null)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                photoImage.sprite = sprite; // Sprite'ý ata
                photoImage.SetNativeSize();
                // Burada SetNativeSize() veya boyut ayarlamasýný tekrar eklemek isteyebilirsiniz.
                // Eðer Investigation Scene'de boyutlar doðruysa, buraya da ekleyin:
                // photoImage.SetNativeSize(); 
            }

            Button photoButton = photoGO.GetComponent<Button>(); // Button bileþenini al
            if (photoButton != null)
            {
                photoButton.onClick.RemoveAllListeners(); // Önceki dinleyicileri temizle
                photoButton.onClick.AddListener(() => ShowLargePhoto(texture)); // Týklayýnca büyük fotoðrafý göster
            }
        }
    }

    void ShowLargePhoto(Texture2D texture) // Büyük fotoðrafý gösteren metot
    {
        if (largePhotoPanel != null && largePhotoDisplay != null)
        {
            largePhotoPanel.SetActive(true); // Paneli aktif yap
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            largePhotoDisplay.sprite = sprite; // Büyük fotoðrafý ata
            // Burada da SetNativeSize() veya boyut ayarlamasýný eklemek isteyebilirsiniz.
            // largePhotoDisplay.SetNativeSize(); 
        }
    }

    public void HideLargePhoto() // Büyük fotoðrafý gizleyen metot
    {
        if (largePhotoPanel != null)
        {
            largePhotoPanel.SetActive(false); // Paneli pasif yap
        }
    }

    public List<Texture2D> GetCollectedPhotos() // Toplanan fotoðraflarý döndüren metot
    {
        return photoTextures;
    }
}
