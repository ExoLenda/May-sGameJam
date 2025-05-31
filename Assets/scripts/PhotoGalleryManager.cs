using UnityEngine;
using System.Collections.Generic; // List kullanmak i�in
using UnityEngine.UI; // UI bile�enlerini kullanmak i�in

public class PhotoGalleryManager : MonoBehaviour
{
    // Foto�raflar�n saklanaca�� liste
    public List<Texture2D> photoGallery = new List<Texture2D>();

    [Tooltip("Galeride tutulacak maksimum foto�raf say�s�.")]
    public int maxPhotos = 10; // Galeride tutulacak maksimum foto�raf say�s�

    // UI bile�enleri i�in referanslar
    [Header("UI Referanslar�")]
    [Tooltip("Foto�raflar�n eklenece�i UI Panel veya Scroll View Content.")]
    public GameObject galleryUIContainer; // Foto�raflar�n eklenece�i UI Panel (genellikle bir Scroll View'in Content'i)
    [Tooltip("Galerideki her foto�raf i�in kullan�lacak bir Image prefab'�.")]
    public GameObject photoPrefab; // Galerideki her foto�raf i�in kullan�lacak bir Image prefab'�

    void Awake()
    {
        // Ba�lang��ta galeriyi gizle (iste�e ba�l�, oyun ba�lad���nda gizli olmas�n� isterseniz)
        if (galleryUIContainer != null)
        {
            galleryUIContainer.SetActive(false);
        }
    }

    /// <summary>
    /// �ekilen foto�raf� galeriye ekler.
    /// E�er maksimum foto�raf say�s�na ula��ld�ysa en eski foto�raf� siler.
    /// </summary>
    /// <param name="photo">Galeriye eklenecek Texture2D foto�raf�.</param>
    public void AddPhotoToGallery(Texture2D photo)
    {
        if (photo == null)
        {
            Debug.LogWarning("Galeriye eklenmeye �al���lan foto�raf null!");
            return;
        }

        // E�er galeri maksimum kapasitesine ula�t�ysa, en eski foto�raf� (listenin ba��ndaki) sil.
        if (photoGallery.Count >= maxPhotos)
        {
            Texture2D oldestPhoto = photoGallery[0];
            photoGallery.RemoveAt(0);
            Destroy(oldestPhoto); // Bellekten serbest b�rakmak i�in
            Debug.Log("Galeride yer a�mak i�in en eski foto�raf silindi.");
        }

        // Yeni foto�raf� galeriye ekle
        photoGallery.Add(photo);
        Debug.Log($"Yeni foto�raf galeriye eklendi. Toplam foto�raf: {photoGallery.Count}");

        // UI'y� g�ncelleme fonksiyonunu �a��r
        UpdateGalleryUI();
    }

    /// <summary>
    /// Galeriyi kullan�c� aray�z�nde g�nceller.
    /// Mevcut t�m foto�raf objelerini temizler ve yeniden olu�turur.
    /// </summary>
    public void UpdateGalleryUI()
    {
        if (galleryUIContainer == null || photoPrefab == null)
        {
            Debug.LogWarning("Gallery UI Container veya Photo Prefab atanmam��! Galeri UI g�ncellenemiyor.");
            return;
        }

        // Mevcut t�m foto�raf objelerini temizle
        foreach (Transform child in galleryUIContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Her foto�raf i�in yeni bir UI eleman� olu�tur ve ekle
        foreach (Texture2D photo in photoGallery)
        {
            GameObject photoInstance = Instantiate(photoPrefab, galleryUIContainer.transform);
            Image photoImage = photoInstance.GetComponent<Image>();
            if (photoImage != null)
            {
                // Texture2D'yi Sprite'a �evir
                // Rect boyutu Texture2D'nin tamam�n� kapsayacak �ekilde ayarlan�r.
                photoImage.sprite = Sprite.Create(photo, new Rect(0, 0, photo.width, photo.height), new Vector2(0.5f, 0.5f));
                photoImage.preserveAspect = true; // Oran� koru
                // �ste�e ba�l�: Her foto�raf�n ad�n� da g�sterebilirsiniz (�rne�in photoInstance i�inde bir Text bile�eni varsa)
                // Text photoNameText = photoInstance.GetComponentInChildren<Text>();
                // if (photoNameText != null) photoNameText.text = photo.name; // E�er Texture2D'ye isim atad�ysan�z
            }
            else
            {
                Debug.LogWarning("Photo Prefab'de bir Image bile�eni bulunamad�!");
            }
        }
        Debug.Log("Galeri UI g�ncellendi.");
    }

    /// <summary>
    /// Galerinin UI g�r�n�rl���n� a��p kapat�r.
    /// </summary>
    public void ToggleGalleryUI()
    {
        if (galleryUIContainer != null)
        {
            galleryUIContainer.SetActive(!galleryUIContainer.activeSelf);
            if (galleryUIContainer.activeSelf)
            {
                UpdateGalleryUI(); // A��ld���nda UI'� g�ncelle
            }
        }
    }
}