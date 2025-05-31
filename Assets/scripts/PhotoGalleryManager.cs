using UnityEngine;
using System.Collections.Generic; // List kullanmak için
using UnityEngine.UI; // UI bileþenlerini kullanmak için

public class PhotoGalleryManager : MonoBehaviour
{
    // Fotoðraflarýn saklanacaðý liste
    public List<Texture2D> photoGallery = new List<Texture2D>();

    [Tooltip("Galeride tutulacak maksimum fotoðraf sayýsý.")]
    public int maxPhotos = 10; // Galeride tutulacak maksimum fotoðraf sayýsý

    // UI bileþenleri için referanslar
    [Header("UI Referanslarý")]
    [Tooltip("Fotoðraflarýn ekleneceði UI Panel veya Scroll View Content.")]
    public GameObject galleryUIContainer; // Fotoðraflarýn ekleneceði UI Panel (genellikle bir Scroll View'in Content'i)
    [Tooltip("Galerideki her fotoðraf için kullanýlacak bir Image prefab'ý.")]
    public GameObject photoPrefab; // Galerideki her fotoðraf için kullanýlacak bir Image prefab'ý

    void Awake()
    {
        // Baþlangýçta galeriyi gizle (isteðe baðlý, oyun baþladýðýnda gizli olmasýný isterseniz)
        if (galleryUIContainer != null)
        {
            galleryUIContainer.SetActive(false);
        }
    }

    /// <summary>
    /// Çekilen fotoðrafý galeriye ekler.
    /// Eðer maksimum fotoðraf sayýsýna ulaþýldýysa en eski fotoðrafý siler.
    /// </summary>
    /// <param name="photo">Galeriye eklenecek Texture2D fotoðrafý.</param>
    public void AddPhotoToGallery(Texture2D photo)
    {
        if (photo == null)
        {
            Debug.LogWarning("Galeriye eklenmeye çalýþýlan fotoðraf null!");
            return;
        }

        // Eðer galeri maksimum kapasitesine ulaþtýysa, en eski fotoðrafý (listenin baþýndaki) sil.
        if (photoGallery.Count >= maxPhotos)
        {
            Texture2D oldestPhoto = photoGallery[0];
            photoGallery.RemoveAt(0);
            Destroy(oldestPhoto); // Bellekten serbest býrakmak için
            Debug.Log("Galeride yer açmak için en eski fotoðraf silindi.");
        }

        // Yeni fotoðrafý galeriye ekle
        photoGallery.Add(photo);
        Debug.Log($"Yeni fotoðraf galeriye eklendi. Toplam fotoðraf: {photoGallery.Count}");

        // UI'yý güncelleme fonksiyonunu çaðýr
        UpdateGalleryUI();
    }

    /// <summary>
    /// Galeriyi kullanýcý arayüzünde günceller.
    /// Mevcut tüm fotoðraf objelerini temizler ve yeniden oluþturur.
    /// </summary>
    public void UpdateGalleryUI()
    {
        if (galleryUIContainer == null || photoPrefab == null)
        {
            Debug.LogWarning("Gallery UI Container veya Photo Prefab atanmamýþ! Galeri UI güncellenemiyor.");
            return;
        }

        // Mevcut tüm fotoðraf objelerini temizle
        foreach (Transform child in galleryUIContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Her fotoðraf için yeni bir UI elemaný oluþtur ve ekle
        foreach (Texture2D photo in photoGallery)
        {
            GameObject photoInstance = Instantiate(photoPrefab, galleryUIContainer.transform);
            Image photoImage = photoInstance.GetComponent<Image>();
            if (photoImage != null)
            {
                // Texture2D'yi Sprite'a çevir
                // Rect boyutu Texture2D'nin tamamýný kapsayacak þekilde ayarlanýr.
                photoImage.sprite = Sprite.Create(photo, new Rect(0, 0, photo.width, photo.height), new Vector2(0.5f, 0.5f));
                photoImage.preserveAspect = true; // Oraný koru
                // Ýsteðe baðlý: Her fotoðrafýn adýný da gösterebilirsiniz (örneðin photoInstance içinde bir Text bileþeni varsa)
                // Text photoNameText = photoInstance.GetComponentInChildren<Text>();
                // if (photoNameText != null) photoNameText.text = photo.name; // Eðer Texture2D'ye isim atadýysanýz
            }
            else
            {
                Debug.LogWarning("Photo Prefab'de bir Image bileþeni bulunamadý!");
            }
        }
        Debug.Log("Galeri UI güncellendi.");
    }

    /// <summary>
    /// Galerinin UI görünürlüðünü açýp kapatýr.
    /// </summary>
    public void ToggleGalleryUI()
    {
        if (galleryUIContainer != null)
        {
            galleryUIContainer.SetActive(!galleryUIContainer.activeSelf);
            if (galleryUIContainer.activeSelf)
            {
                UpdateGalleryUI(); // Açýldýðýnda UI'ý güncelle
            }
        }
    }
}