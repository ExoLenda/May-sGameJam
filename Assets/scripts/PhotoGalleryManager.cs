using UnityEngine;
using UnityEngine.UI; // UI elemanlarý için gerekli
using System.IO; // Dosya iþlemleri için gerekli
using System.Collections.Generic; // Listeler için gerekli
using DynamicPhotoCamera; // PhotoController'a eriþmek için gerekli

public class PhotoGalleryManager : MonoBehaviour
{
    // --- INSPECTOR REFERANSLARI ---
    [Header("UI Referanslarý")]
    [Tooltip("Galeriyi içeren ana UI Paneli.")]
    public GameObject galleryPanel;
    [Tooltip("Fotoðraf kartlarýnýn ekleneceði kaydýrýlabilir alanýn 'Content' objesi.")]
    public Transform photoCardHolder; // ScrollView'in Content objesi
    [Tooltip("Galeride gösterilecek tek bir fotoðraf kartýnýn prefab'ý.")]
    public GameObject photoDisplayItemPrefab; // Daha önce oluþturduðumuz PhotoDisplayItem prefab'ý

    [Tooltip("Büyük fotoðrafý gösteren ana UI Paneli.")]
    public GameObject largePhotoDisplayPanel;
    [Tooltip("Büyük fotoðrafýn gösterileceði Image bileþeni.")]
    public Image largePhotoViewer;

    [Header("Diðer Referanslar")]
    [Tooltip("Sahnedeki PhotoController script'ine referans.")]
    public PhotoController photoController; // PhotoController'dan gelen Texture2D'yi yakalamak için

    // --- ÖZEL DEÐÝÞKENLER ---
    private List<Sprite> loadedPhotoSprites = new List<Sprite>(); // Yüklenen tüm sprite'larý tutar
    private const string PHOTO_FOLDER_NAME = "/Photos/"; // Fotoðraflarýn kaydedileceði klasör adý
    private string photosFolderPath; // Fotoðraflarýn tam yolu

    // --- UNITY YAÞAM DÖNGÜSÜ METOTLARI ---
    void Awake()
    {
        // Fotoðraf klasör yolunu belirle
        photosFolderPath = Application.persistentDataPath + PHOTO_FOLDER_NAME;

        // Klasör yoksa oluþtur
        if (!Directory.Exists(photosFolderPath))
        {
            Directory.CreateDirectory(photosFolderPath);
            Debug.Log($"[PhotoGalleryManager] Fotoðraf klasörü oluþturuldu: {photosFolderPath}");
        }

        // Baþlangýçta galeri ve büyük fotoðraf panelini gizle
        if (galleryPanel != null) galleryPanel.SetActive(false);
        if (largePhotoDisplayPanel != null) largePhotoDisplayPanel.SetActive(false);
    }

    void Start()
    {
        // PhotoController'ýn OnPhotoCaptureComplete event'ine abone ol
        // Bu sayede PhotoController fotoðraf çektiðinde bizim metodumuz çalýþacak
        if (photoController != null)
        {
            photoController.OnPhotoCaptureComplete.AddListener(AddPhotoToGallery);
            Debug.Log("[PhotoGalleryManager] PhotoController'ýn OnPhotoCaptureComplete event'ine abone olundu.");
        }
        else
        {
            Debug.LogError("[PhotoGalleryManager] PhotoController referansý atanmadý! Lütfen Inspector'dan atayýn.");
        }

        // Kayýtlý fotoðraflarý yükle
        LoadAllPhotosFromDisk();
    }

    void OnDestroy()
    {
        // Script yok edildiðinde event'ten aboneliði kaldýr
        if (photoController != null)
        {
            photoController.OnPhotoCaptureComplete.RemoveListener(AddPhotoToGallery);
            Debug.Log("[PhotoGalleryManager] PhotoController'ýn OnPhotoCaptureComplete event'inden abonelik kaldýrýldý.");
        }
    }

    // --- FOTOÐRAF KAYDETME / YÜKLEME METOTLARI ---

    /// <summary>
    /// PhotoController'dan gelen Texture2D'yi alýr, kaydeder ve galeriye ekler.
    /// </summary>
    /// <param name="photoTexture">Çekilen fotoðrafýn Texture2D hali.</param>
    public void AddPhotoToGallery(Texture2D photoTexture)
    {
        if (photoTexture == null)
        {
            Debug.LogError("[PhotoGalleryManager] AddPhotoToGallery metoduna null Texture2D geldi!");
            return;
        }


        // Benzersiz bir dosya adý oluþtur
        string fileName = $"photo_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")}.png"; // Tarih ve saat ile benzersiz isim
        string filePath = Path.Combine(photosFolderPath, fileName);

        // Texture2D'yi PNG olarak diske kaydet
        byte[] bytes = photoTexture.EncodeToPNG();
        try
        {
            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"[PhotoGalleryManager] Fotoðraf kaydedildi: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PhotoGalleryManager] Fotoðraf kaydedilirken hata oluþtu: {e.Message}");
            return;
        }

        // Yeni kaydedilen fotoðrafý galeri UI'ýna ekle
        Sprite newSprite = Sprite.Create(photoTexture, new Rect(0, 0, photoTexture.width, photoTexture.height), Vector2.one * 0.5f);

        // ÖNEMLÝ: Texture2D artýk Sprite'a dönüþtürüldü, bu yüzden original Texture2D'yi yok edebiliriz.
        // Ancak bu Texture2D PhotoController'dan geldiði için, PhotoController'ýn kendisinin
        // uygun zamanda ReleaseTemporary veya Destroy yapmasý daha iyidir.
        // Eðer PhotoController'da Destroy edilmiyorsa burada yapabiliriz: Destroy(photoTexture);

        loadedPhotoSprites.Add(newSprite); // Listeye ekle

        // UI'a yeni fotoðrafý ekle
        CreatePhotoCardUI(newSprite);
    }

    /// <summary>
    /// Diskteki tüm kayýtlý fotoðraflarý yükler ve galeriye ekler.
    /// </summary>
    private void LoadAllPhotosFromDisk()
    {
        // Öncelikle mevcut tüm fotoðraflarý temizle
        foreach (Transform child in photoCardHolder)
        {
            Destroy(child.gameObject);
        }
        loadedPhotoSprites.Clear();

        if (!Directory.Exists(photosFolderPath))
        {
            Debug.LogWarning("[PhotoGalleryManager] Fotoðraf klasörü bulunamadý. Yüklenecek fotoðraf yok.");
            return;
        }

        string[] photoFiles = Directory.GetFiles(photosFolderPath, "*.png"); // Sadece .png dosyalarýný al

        foreach (string filePath in photoFiles)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(1, 1);
            if (texture.LoadImage(bytes))
            {
                Sprite loadedSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                loadedPhotoSprites.Add(loadedSprite);
                CreatePhotoCardUI(loadedSprite);
            }
            else
            {
                Debug.LogError($"[PhotoGalleryManager] Dosya yüklenemedi veya geçersiz format: {filePath}");
                Destroy(texture); // Yüklenemediyse texture'ý yok et
            }
        }
        Debug.Log($"[PhotoGalleryManager] Diskten {loadedPhotoSprites.Count} fotoðraf yüklendi.");
    }

    /// <summary>
    /// Bir Sprite'tan yeni bir UI fotoðraf kartý oluþturur ve galeriye ekler.
    /// </summary>
    /// <param name="sprite">Gösterilecek fotoðrafýn Sprite'ý.</param>
    private void CreatePhotoCardUI(Sprite sprite)
    {
        if (photoDisplayItemPrefab == null)
        {
            Debug.LogError("[PhotoGalleryManager] PhotoDisplayItem Prefab'ý atanmadý! Galeriye fotoðraf eklenemiyor.");
            return;
        }
        if (photoCardHolder == null)
        {
            Debug.LogError("[PhotoGalleryManager] Photo Card Holder (Content) referansý atanmadý! Galeriye fotoðraf eklenemiyor.");
            return;
        }

        GameObject photoCard = Instantiate(photoDisplayItemPrefab, photoCardHolder);
        Image photoImage = photoCard.GetComponent<Image>();

        if (photoImage != null)
        {
            photoImage.sprite = sprite;
        }
        else
        {
            Debug.LogError($"[PhotoGalleryManager] PhotoDisplayItem Prefab'ýnda Image bileþeni bulunamadý! Prefab'ý kontrol edin: {photoCard.name}");
            Destroy(photoCard); // Yanlýþ prefab ise yok et
            return;
        }

        // Büyük fotoðrafý göstermek için Buton listener ekle
        Button viewButton = photoCard.GetComponentInChildren<Button>();
        if (viewButton != null)
        {
            // Lambda ifadesi kullanarak sprite'ý butona ekle
            viewButton.onClick.AddListener(() => ViewLargePhoto(sprite));
        }
        else
        {
            Debug.LogWarning($"[PhotoGalleryManager] PhotoDisplayItem'da 'ViewPhotoButton' bulunamadý. Prefab'ý kontrol edin: {photoCard.name}");
        }
    }

    // --- GALERÝ UI ÝÞLEVLERÝ ---

    /// <summary>
    /// Galeriyi görünür yapar. Bu metot bir UI butonuna atanabilir.
    /// </summary>
    public void OpenGallery()
    {
        if (galleryPanel != null)
        {
            galleryPanel.SetActive(true);
            Debug.Log("[PhotoGalleryManager] Galeri açýldý.");
        }
    }

    /// <summary>
    /// Galeriyi gizler. Bu metot bir UI butonuna atanabilir.
    /// </summary>
    public void CloseGallery()
    {
        if (galleryPanel != null)
        {
            galleryPanel.SetActive(false);
            Debug.Log("[PhotoGalleryManager] Galeri kapatýldý.");
        }
    }

    /// <summary>
    /// Seçilen fotoðrafý büyük boyutta gösterir.
    /// </summary>
    /// <param name="photoSprite">Büyütülecek fotoðrafýn Sprite'ý.</param>
    public void ViewLargePhoto(Sprite photoSprite)
    {
        if (largePhotoDisplayPanel != null && largePhotoViewer != null)
        {
            largePhotoViewer.sprite = photoSprite;
            largePhotoDisplayPanel.SetActive(true);
            Debug.Log("[PhotoGalleryManager] Büyük fotoðraf gösteriliyor.");
        }
        else
        {
            Debug.LogError("[PhotoGalleryManager] LargePhotoDisplayPanel veya LargePhotoViewer referansý atanmadý.");
        }
    }

    /// <summary>
    /// Büyük fotoðraf görüntüleme panelini gizler. Bu metot bir UI butonuna atanabilir.
    /// </summary>
    public void CloseLargePhoto()
    {
        if (largePhotoDisplayPanel != null)
        {
            largePhotoDisplayPanel.SetActive(false);
            largePhotoViewer.sprite = null; // Bellek temizliði için sprite'ý kaldýr
            Debug.Log("[PhotoGalleryManager] Büyük fotoðraf kapatýldý.");
        }
    }
}
