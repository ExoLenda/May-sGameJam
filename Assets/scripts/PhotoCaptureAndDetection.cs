using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System.Collections.Generic; // List kullanmak için

public class PhotoCaptureAndDetection : MonoBehaviour
{
    // Dynamic Photo Camera Mini'nin kamera bileþeni
    public Camera photoCamera;

    // Galeri ve Envanter Yöneticileri (Daha önceki örneklerden)
    public PhotoGalleryManager galleryManager;
    public InventoryManager inventoryManager;

    // Fotoðraf çekildiðinde bu metodu çaðýrýn (Dynamic Photo Camera Mini entegrasyonu)
    public void OnPhotoCaptured(Texture2D capturedPhoto)
    {
        // 1. Fotoðrafý Galeriye Kaydet
        if (galleryManager != null)
        {
            galleryManager.AddPhotoToGallery(capturedPhoto);
        }

        // 2. Fotoðrafý Çekilebilir Objeleri Algýla
        DetectTaggableObjectsInPhoto(capturedPhoto);
    }

    private void DetectTaggableObjectsInPhoto(Texture2D photo)
    {
        // Kameranýn görüþ alanýndaki objeleri algýlamak için daha doðru bir yol: Raycasting
        // Burada gerçek fotoðraftan piksel okumak yerine,
        // kameranýn perspektifinden sahnedeki objeleri tespit edeceðiz.

        List<string> detectedItems = new List<string>();

        // Kameranýn görüþ alaný içindeki tüm renderlanabilir objeleri bulma
        // Daha performanslý olabilir, ancak tüm sahneyi deðil, sadece görünenleri hedefleriz.
        // Bounds camBounds = photoCamera.GetFrustumBounds(); // Unity 2021.2+
        // Ya da kameranýn görüþ düzlemlerini kullanarak manuel kontrol.

        // Basit bir yaklaþým: Kameranýn önündeki belli bir alanda Raycast atmak
        // Veya daha detaylý: Render edilen her piksel için Raycast yapmak (performanslý deðil!)
        // En iyi yaklaþým: Raycasting'i sadece odaklanýlan bölge için veya belirli aralýklarla yapmak.

        // Örneðin, kameranýn görüþ alanýný temsil eden ýzgara noktalarýna Raycast atmak
        // Bu, genel bir fikir verir. Daha hassas algýlama için gridi sýklaþtýrabilirsiniz.
        int rayCountX = 5; // X ekseninde atýlacak ýþýn sayýsý
        int rayCountY = 5; // Y ekseninde atýlacak ýþýn sayýsý

        for (int x = 0; x < rayCountX; x++)
        {
            for (int y = 0; y < rayCountY; y++)
            {
                // Ekran koordinatlarýný yüzde olarak hesapla (0-1 arasý)
                float viewportX = (float)x / (rayCountX - 1);
                float viewportY = (float)y / (rayCountY - 1);

                // Kameranýn görüþ alanýndan bir ýþýn oluþtur
                Ray ray = photoCamera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0));
                RaycastHit hit;

                // Iþýn bir þeye çarptý mý?
                if (Physics.Raycast(ray, out hit, 100f)) // 100f: maksimum ýþýn mesafesi
                {
                    // Çarpan objenin "FotoðrafýÇekilebilir" tag'ine sahip olup olmadýðýný kontrol et
                    if (hit.collider.CompareTag("FotoðrafýÇekilebilir"))
                    {
                        // Ayný objeyi birden fazla kez algýlamamak için kontrol
                        if (!detectedItems.Contains(hit.collider.name))
                        {
                            Debug.Log($"Algýlandý: {hit.collider.name} (Tag: FotoðrafýÇekilebilir)");
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
            Debug.Log("Fotoðrafta 'FotoðrafýÇekilebilir' tag'ine sahip hiçbir obje algýlanmadý.");
        }
    }
}
