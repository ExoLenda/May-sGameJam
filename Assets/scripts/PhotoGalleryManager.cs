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
    public Transform photoCardHolder; // ScrollView'in Content objesi (GridLayoutGroup burada olmal�)
    [Tooltip("Galeride g�sterilecek tek bir foto�raf kart�n�n prefab'�.")]
    public GameObject photoDisplayItemPrefab; // Daha �nce olu�turdu�umuz PhotoDisplayItem prefab'�

    [Tooltip("B�y�k foto�raf� g�steren ana UI Paneli.")]
    public GameObject largePhotoDisplayPanel;
    [Tooltip("B�y�k foto�raf�n g�sterilece�i Image bile�eni.")]
    public Image largePhotoViewer;

    [Header("Di�er Referanslar")]
    [Tooltip("Sahnedeki PhotoController script'ine referans.")]
    public PhotoController photoController; // PhotoController'dan gelen Texture2D'yi yakalamak i�in

    [Header("Galeri Ayarlar�")]
    [Tooltip("Oyun ba�lad���nda diskteki foto�raflar� otomatik y�kle?")]
    public bool loadPhotosOnStart = false; // BU AYAR �STE��N�ZE G�RE DE���T�R�LMEL�
    [Tooltip("Oyun her ba�lad���nda diskteki t�m foto�raflar� sil? (Uygulama yolu alt�ndan)")]
    public bool clearPhotosOnGameStart = false; // YEN� EKLEND� - D�SKTEM DE S�LME �STE��N�Z ���N

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

        // YEN� EKLEND�: Oyun ba�lad���nda diskteki foto�raflar� sil
        if (clearPhotosOnGameStart)
        {
            ClearAllPhotosFromDisk(); // Diskten de sil
        }

        // YEN� EKLEND�: E�er 'loadPhotosOnStart' true ise kay�tl� foto�raflar� y�kle
        if (loadPhotosOnStart)
        {
            LoadAllPhotosFromDisk();
        }
        else
        {
            // E�er diskten y�kleme yap�lm�yorsa, UI'� temizle (zaten LoadAllPhotosFromDisk i�inde var ama burada da garantileyelim)
            ClearGalleryUI();
        }
    }

    void OnDestroy()
    {
        // Script yok edildi�inde event'ten aboneli�i kald�r
        if (photoController != null)
        {
            photoController.OnPhotoCaptureComplete.RemoveListener(AddPhotoToGallery);
            Debug.Log("[PhotoGalleryManager] PhotoController'�n OnPhotoCaptureComplete event'inden abonelik kald�r�ld�.");
        }
        // Uygulama kapan�rken sprite'lar� ve ili�kili texture'lar� temizle
        // Bu, belle�i d�zg�n bir �ekilde serbest b�rak�r.
        foreach (Sprite sprite in loadedPhotoSprites)
        {
            if (sprite != null && sprite.texture != null)
            {
                Destroy(sprite.texture);
            }
            Destroy(sprite);
        }
        loadedPhotoSprites.Clear();
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

        loadedPhotoSprites.Add(newSprite); // Listeye ekle

        // UI'a yeni foto�raf� ekle
        CreatePhotoCardUI(newSprite);
    }

    /// <summary>
    /// Diskteki t�m kay�tl� foto�raflar� y�kler ve galeriye ekler.
    /// </summary>
    public void LoadAllPhotosFromDisk()
    {
        // �ncelikle mevcut t�m foto�raflar� ve UI objelerini temizle
        ClearGalleryUI(); // YEN� EKLEND�: UI'� temizlemek i�in ayr� bir metot �a��r
        loadedPhotoSprites.Clear(); // Sprite listesini de temizle

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
    /// Diskteki t�m foto�raf dosyalar�n� siler.
    /// </summary>
    public void ClearAllPhotosFromDisk() // YEN� METOT
    {
        ClearGalleryUI(); // �nce UI'daki objeleri temizle
        loadedPhotoSprites.Clear(); // Bellekteki sprite'lar� temizle

        if (Directory.Exists(photosFolderPath))
        {
            try
            {
                // Klas�rdeki t�m dosyalar� sil
                string[] photoFiles = Directory.GetFiles(photosFolderPath, "*.png");
                foreach (string filePath in photoFiles)
                {
                    File.Delete(filePath);
                }
                Debug.Log($"[PhotoGalleryManager] Diskteki t�m foto�raflar silindi: {photosFolderPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PhotoGalleryManager] Diskteki foto�raflar silinirken hata olu�tu: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Galerideki t�m foto�raf kart� UI objelerini temizler.
    /// </summary>
    private void ClearGalleryUI() // AYRI B�R METOT HAL�NE GET�R�LD�
    {
        // photoCardHolder alt�ndaki t�m �ocuk objeleri sil
        // Tersten gitmek g�venlidir, ��nk� objeler silindik�e indeksleri de�i�ir.
        for (int i = photoCardHolder.childCount - 1; i >= 0; i--)
        {
            GameObject objToDestroy = photoCardHolder.GetChild(i).gameObject;
            // E�er objede bir SpriteRenderer veya Image bile�eni varsa,
            // atanan Sprite ve Texture'� da temizlemek bellek s�z�nt�lar�n� �nler.
            Image img = objToDestroy.GetComponent<Image>();
            if (img != null && img.sprite != null && img.sprite.texture != null)
            {
                Destroy(img.sprite.texture); // Texture'� yok et
                Destroy(img.sprite); // Sprite'� yok et
            }
            Destroy(objToDestroy);
        }
        Debug.Log("[PhotoGalleryManager] Galeri UI'� temizlendi.");
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
        // Prefab�n kendisinde Image varsa do�rudan GetComponent, yoksa Children i�inde ara
        Image photoImage = photoCard.GetComponent<Image>();
        if (photoImage == null) // E�er prefab'�n kendisi Image de�ilse �ocuklar�nda ara
        {
            photoImage = photoCard.GetComponentInChildren<Image>(true);
        }

        if (photoImage != null)
        {
            photoImage.sprite = sprite;
        }
        else
        {
            Debug.LogError($"[PhotoGalleryManager] PhotoDisplayItem Prefab'�nda veya �ocuklar�nda Image bile�eni bulunamad�! Prefab'� kontrol edin: {photoCard.name}");
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
            // E�er galeri a��ld���nda yeni �ekilen foto�raflar� g�rmek istiyorsan�z,
            // veya diskteki mevcut foto�raflar� tekrar y�klemek istiyorsan�z burada �a��rabilirsiniz:
            // LoadAllPhotosFromDisk();
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