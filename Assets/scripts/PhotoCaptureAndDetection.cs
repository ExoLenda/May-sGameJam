using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System.Collections.Generic; // List kullanmak i�in

public class PhotoCaptureAndDetection : MonoBehaviour
{
    // Dynamic Photo Camera Mini'nin kamera bile�eni
    public Camera photoCamera;
    public DynamicPhotoCamera.PhotoController photoControllerAsset; // Asset'in Namespace'i ve Script Ad�
    // Galeri ve Envanter Y�neticileri (Daha �nceki �rneklerden)
    public PhotoGalleryManager galleryManager;
    public InventoryManager inventoryManager;

    // Foto�raf �ekildi�inde bu metodu �a��r�n (Dynamic Photo Camera Mini entegrasyonu)

    void Start()
    {
        if (photoControllerAsset != null)
        {
            // OnPhotoCaptureComplete event'ine OnPhotoCaptured metodumuzu ekle
            photoControllerAsset.OnPhotoCaptureComplete.AddListener(OnPhotoCaptured);
            Debug.Log("PhotoCaptureAndDetection, PhotoController'�n OnPhotoCaptureComplete event'ine abone oldu.");
        }
        else
        {
            Debug.LogError("PhotoControllerAsset referans� NULL! L�tfen Inspector'dan atay�n. Foto�raf yakalama event'ine abone olunamad�.");
        }
    }

    // Script yok edildi�inde abonelikten ��kmak iyi bir pratiktir
    void OnDestroy()
    {
        if (photoControllerAsset != null)
        {
            photoControllerAsset.OnPhotoCaptureComplete.RemoveListener(OnPhotoCaptured);
        }
    }
    public void OnPhotoCaptured(Texture2D capturedPhoto)
    {
        Debug.Log("�zel OnPhotoCaptured metodumuz �a�r�ld�!");
        // 1. Foto�raf� Galeriye Kaydet
        if (galleryManager != null)
        {
            galleryManager.AddPhotoToGallery(capturedPhoto);
        }
        else
        {
            Debug.LogError("GalleryManager referans� NULL! Foto�raf galeriye eklenemedi.");
        }

        // 2. Foto�raf� �ekilebilir Objeleri Alg�la
        if (capturedPhoto != null)
        {
            DetectTaggableObjectsInPhoto(capturedPhoto);
        }
        else
        {
            Debug.LogWarning("Yakalanan foto�raf null oldu�u i�in objeler alg�lanamad�.");
        }
    }

    private void DetectTaggableObjectsInPhoto(Texture2D photo)
    {
        // Kameran�n g�r�� alan�ndaki objeleri alg�lamak i�in daha do�ru bir yol: Raycasting
        // Burada ger�ek foto�raftan piksel okumak yerine,
        // kameran�n perspektifinden sahnedeki objeleri tespit edece�iz.

        List<string> detectedItems = new List<string>();

        // Kameran�n g�r�� alan� i�indeki t�m renderlanabilir objeleri bulma
        // Daha performansl� olabilir, ancak t�m sahneyi de�il, sadece g�r�nenleri hedefleriz.
        // Bounds camBounds = photoCamera.GetFrustumBounds(); // Unity 2021.2+
        // Ya da kameran�n g�r�� d�zlemlerini kullanarak manuel kontrol.

        // Basit bir yakla��m: Kameran�n �n�ndeki belli bir alanda Raycast atmak
        // Veya daha detayl�: Render edilen her piksel i�in Raycast yapmak (performansl� de�il!)
        // En iyi yakla��m: Raycasting'i sadece odaklan�lan b�lge i�in veya belirli aral�klarla yapmak.

        // �rne�in, kameran�n g�r�� alan�n� temsil eden �zgara noktalar�na Raycast atmak
        // Bu, genel bir fikir verir. Daha hassas alg�lama i�in gridi s�kla�t�rabilirsiniz.
        int rayCountX = 5; // X ekseninde at�lacak ���n say�s�
        int rayCountY = 5; // Y ekseninde at�lacak ���n say�s�

        for (int x = 0; x < rayCountX; x++)
        {
            for (int y = 0; y < rayCountY; y++)
            {
                // Ekran koordinatlar�n� y�zde olarak hesapla (0-1 aras�)
                float viewportX = (float)x / (rayCountX - 1);
                float viewportY = (float)y / (rayCountY - 1);

                // Kameran�n g�r�� alan�ndan bir ���n olu�tur
                Ray ray = photoCamera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0));
                RaycastHit hit;

                // I��n bir �eye �arpt� m�?
                if (Physics.Raycast(ray, out hit, 100f)) // 100f: maksimum ���n mesafesi
                {
                    // �arpan objenin "Foto�raf��ekilebilir" tag'ine sahip olup olmad���n� kontrol et 
                    if (hit.collider.CompareTag("Foto�raf��ekilebilir"))
                    {
                        // Ayn� objeyi birden fazla kez alg�lamamak i�in kontrol
                        if (!detectedItems.Contains(hit.collider.name))
                        {
                            Debug.Log($"Alg�land�: {hit.collider.name} (Tag: Foto�raf��ekilebilir)");
                            detectedItems.Add(hit.collider.name);

                            // Envantere ekle
                            if (inventoryManager != null)
                            {
                                inventoryManager.AddItemToInventory(hit.collider.name);
                            }
                        }
                    }
                }
            }
        }

        if (detectedItems.Count == 0)
        {
            Debug.Log("Foto�rafta 'Foto�raf��ekilebilir' tag'ine sahip hi�bir obje alg�lanmad�.");
        }
    }
}
