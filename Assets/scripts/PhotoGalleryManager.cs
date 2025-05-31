using UnityEngine;
using UnityEngine.UI; // UI elemanlar� i�in gerekli
using System.IO; // Dosya i�lemleri i�in gerekli
using System.Collections.Generic; // Listeler i�in gerekli
using DynamicPhotoCamera; // PhotoController'a eri�mek i�in gerekli

public class PhotoGalleryManager : MonoBehaviour
{
    // --- INSPECTOR REFERANSLARI ---
    [Header("UI Referanslar�")]
    [Tooltip("Galeriyi i�eren ana UI Paneli.")]
    public GameObject galleryPanel;
    [Tooltip("Foto�raf kartlar�n�n eklenece�i kayd�r�labilir alan�n 'Content' objesi.")]
    public Transform photoCardHolder; // ScrollView'in Content objesi
    [Tooltip("Galeride g�sterilecek tek bir foto�raf kart�n�n prefab'�.")]
    public GameObject photoDisplayItemPrefab; // Daha �nce olu�turdu�umuz PhotoDisplayItem prefab'�

    [Tooltip("B�y�k foto�raf� g�steren ana UI Paneli.")]
    public GameObject largePhotoDisplayPanel;
    [Tooltip("B�y�k foto�raf�n g�sterilece�i Image bile�eni.")]
    public Image largePhotoViewer;

    [Header("Di�er Referanslar")]
    [Tooltip("Sahnedeki PhotoController script'ine referans.")]
    public PhotoController photoController; // PhotoController'dan gelen Texture2D'yi yakalamak i�in

    // --- �ZEL DE���KENLER ---
    private List<Sprite> loadedPhotoSprites = new List<Sprite>(); // Y�klenen t�m sprite'lar� tutar
    private const string PHOTO_FOLDER_NAME = "/Photos/"; // Foto�raflar�n kaydedilece�i klas�r ad�
    private string photosFolderPath; // Foto�raflar�n tam yolu

    // --- UNITY YA�AM D�NG�S� METOTLARI ---
    void Awake()
    {
        // Foto�raf klas�r yolunu belirle
        photosFolderPath = Application.persistentDataPath + PHOTO_FOLDER_NAME;

        // Klas�r yoksa olu�tur
        if (!Directory.Exists(photosFolderPath))
        {
            Directory.CreateDirectory(photosFolderPath);
            Debug.Log($"[PhotoGalleryManager] Foto�raf klas�r� olu�turuldu: {photosFolderPath}");
        }

        // Ba�lang��ta galeri ve b�y�k foto�raf panelini gizle
        if (galleryPanel != null) galleryPanel.SetActive(false);
        if (largePhotoDisplayPanel != null) largePhotoDisplayPanel.SetActive(false);
    }

    void Start()
    {
        // PhotoController'�n OnPhotoCaptureComplete event'ine abone ol
        // Bu sayede PhotoController foto�raf �ekti�inde bizim metodumuz �al��acak
        if (photoController != null)
        {
            photoController.OnPhotoCaptureComplete.AddListener(AddPhotoToGallery);
            Debug.Log("[PhotoGalleryManager] PhotoController'�n OnPhotoCaptureComplete event'ine abone olundu.");
        }
        else
        {
            Debug.LogError("[PhotoGalleryManager] PhotoController referans� atanmad�! L�tfen Inspector'dan atay�n.");
        }

        // Kay�tl� foto�raflar� y�kle
        LoadAllPhotosFromDisk();
    }

    void OnDestroy()
    {
        // Script yok edildi�inde event'ten aboneli�i kald�r
        if (photoController != null)
        {
            photoController.OnPhotoCaptureComplete.RemoveListener(AddPhotoToGallery);
            Debug.Log("[PhotoGalleryManager] PhotoController'�n OnPhotoCaptureComplete event'inden abonelik kald�r�ld�.");
        }
    }

    // --- FOTO�RAF KAYDETME / Y�KLEME METOTLARI ---

    /// <summary>
    /// PhotoController'dan gelen Texture2D'yi al�r, kaydeder ve galeriye ekler.
    /// </summary>
    /// <param name="photoTexture">�ekilen foto�raf�n Texture2D hali.</param>
    public void AddPhotoToGallery(Texture2D photoTexture)
    {
        if (photoTexture == null)
        {
            Debug.LogError("[PhotoGalleryManager] AddPhotoToGallery metoduna null Texture2D geldi!");
            return;
        }


        // Benzersiz bir dosya ad� olu�tur
        string fileName = $"photo_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")}.png"; // Tarih ve saat ile benzersiz isim
        string filePath = Path.Combine(photosFolderPath, fileName);

        // Texture2D'yi PNG olarak diske kaydet
        byte[] bytes = photoTexture.EncodeToPNG();
        try
        {
            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"[PhotoGalleryManager] Foto�raf kaydedildi: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PhotoGalleryManager] Foto�raf kaydedilirken hata olu�tu: {e.Message}");
            return;
        }

        // Yeni kaydedilen foto�raf� galeri UI'�na ekle
        Sprite newSprite = Sprite.Create(photoTexture, new Rect(0, 0, photoTexture.width, photoTexture.height), Vector2.one * 0.5f);

        // �NEML�: Texture2D art�k Sprite'a d�n��t�r�ld�, bu y�zden original Texture2D'yi yok edebiliriz.
        // Ancak bu Texture2D PhotoController'dan geldi�i i�in, PhotoController'�n kendisinin
        // uygun zamanda ReleaseTemporary veya Destroy yapmas� daha iyidir.
        // E�er PhotoController'da Destroy edilmiyorsa burada yapabiliriz: Destroy(photoTexture);

        loadedPhotoSprites.Add(newSprite); // Listeye ekle

        // UI'a yeni foto�raf� ekle
        CreatePhotoCardUI(newSprite);
    }

    /// <summary>
    /// Diskteki t�m kay�tl� foto�raflar� y�kler ve galeriye ekler.
    /// </summary>
    private void LoadAllPhotosFromDisk()
    {
        // �ncelikle mevcut t�m foto�raflar� temizle
        foreach (Transform child in photoCardHolder)
        {
            Destroy(child.gameObject);
        }
        loadedPhotoSprites.Clear();

        if (!Directory.Exists(photosFolderPath))
        {
            Debug.LogWarning("[PhotoGalleryManager] Foto�raf klas�r� bulunamad�. Y�klenecek foto�raf yok.");
            return;
        }

        string[] photoFiles = Directory.GetFiles(photosFolderPath, "*.png"); // Sadece .png dosyalar�n� al

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
                Debug.LogError($"[PhotoGalleryManager] Dosya y�klenemedi veya ge�ersiz format: {filePath}");
                Destroy(texture); // Y�klenemediyse texture'� yok et
            }
        }
        Debug.Log($"[PhotoGalleryManager] Diskten {loadedPhotoSprites.Count} foto�raf y�klendi.");
    }

    /// <summary>
    /// Bir Sprite'tan yeni bir UI foto�raf kart� olu�turur ve galeriye ekler.
    /// </summary>
    /// <param name="sprite">G�sterilecek foto�raf�n Sprite'�.</param>
    private void CreatePhotoCardUI(Sprite sprite)
    {
        if (photoDisplayItemPrefab == null)
        {
            Debug.LogError("[PhotoGalleryManager] PhotoDisplayItem Prefab'� atanmad�! Galeriye foto�raf eklenemiyor.");
            return;
        }
        if (photoCardHolder == null)
        {
            Debug.LogError("[PhotoGalleryManager] Photo Card Holder (Content) referans� atanmad�! Galeriye foto�raf eklenemiyor.");
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
            Debug.LogError($"[PhotoGalleryManager] PhotoDisplayItem Prefab'�nda Image bile�eni bulunamad�! Prefab'� kontrol edin: {photoCard.name}");
            Destroy(photoCard); // Yanl�� prefab ise yok et
            return;
        }

        // B�y�k foto�raf� g�stermek i�in Buton listener ekle
        Button viewButton = photoCard.GetComponentInChildren<Button>();
        if (viewButton != null)
        {
            // Lambda ifadesi kullanarak sprite'� butona ekle
            viewButton.onClick.AddListener(() => ViewLargePhoto(sprite));
        }
        else
        {
            Debug.LogWarning($"[PhotoGalleryManager] PhotoDisplayItem'da 'ViewPhotoButton' bulunamad�. Prefab'� kontrol edin: {photoCard.name}");
        }
    }

    // --- GALER� UI ��LEVLER� ---

    /// <summary>
    /// Galeriyi g�r�n�r yapar. Bu metot bir UI butonuna atanabilir.
    /// </summary>
    public void OpenGallery()
    {
        if (galleryPanel != null)
        {
            galleryPanel.SetActive(true);
            Debug.Log("[PhotoGalleryManager] Galeri a��ld�.");
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
            Debug.Log("[PhotoGalleryManager] Galeri kapat�ld�.");
        }
    }

    /// <summary>
    /// Se�ilen foto�raf� b�y�k boyutta g�sterir.
    /// </summary>
    /// <param name="photoSprite">B�y�t�lecek foto�raf�n Sprite'�.</param>
    public void ViewLargePhoto(Sprite photoSprite)
    {
        if (largePhotoDisplayPanel != null && largePhotoViewer != null)
        {
            largePhotoViewer.sprite = photoSprite;
            largePhotoDisplayPanel.SetActive(true);
            Debug.Log("[PhotoGalleryManager] B�y�k foto�raf g�steriliyor.");
        }
        else
        {
            Debug.LogError("[PhotoGalleryManager] LargePhotoDisplayPanel veya LargePhotoViewer referans� atanmad�.");
        }
    }

    /// <summary>
    /// B�y�k foto�raf g�r�nt�leme panelini gizler. Bu metot bir UI butonuna atanabilir.
    /// </summary>
    public void CloseLargePhoto()
    {
        if (largePhotoDisplayPanel != null)
        {
            largePhotoDisplayPanel.SetActive(false);
            largePhotoViewer.sprite = null; // Bellek temizli�i i�in sprite'� kald�r
            Debug.Log("[PhotoGalleryManager] B�y�k foto�raf kapat�ld�.");
        }
    }
}
