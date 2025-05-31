using UnityEngine;

public class CameraBoundary : MonoBehaviour
{
    // Bu deðiþkenler, kameranýn gidebileceði minimum ve maksimum X ve Y koordinatlarýný belirler.
    // Bu deðerleri Unity Editor'dan kolayca ayarlayabileceðiz.
    [Header("Kamera Hareket Limitleri")]
    public float minX = -10f; // Kameranýn sola gidebileceði en uzak nokta
    public float maxX = 10f;  // Kameranýn saða gidebileceði en uzak nokta
    public float minY = -5f;  // Kameranýn aþaðý gidebileceði en uzak nokta
    public float maxY = 5f;   // Kameranýn yukarý gidebileceði en uzak nokta

    void LateUpdate() // Bu metod, her karede kamera hareket ettikten sonra çalýþýr.
    {
        // Kameranýn þu anki konumunu alýyoruz
        Vector3 currentPosition = transform.position;

        // X koordinatýný belirli sýnýrlar arasýna sýkýþtýrýyoruz.
        // Örneðin, X 12 ise ve maxX 10 ise, X 10'a çekilir.
        // X -12 ise ve minX -10 ise, X -10'a çekilir.
        currentPosition.x = Mathf.Clamp(currentPosition.x, minX, maxX);

        // Y koordinatýný da ayný þekilde sýkýþtýrýyoruz
        currentPosition.y = Mathf.Clamp(currentPosition.y, minY, maxY);

        // Kameranýn konumunu sýkýþtýrýlmýþ (limitlenmiþ) yeni pozisyona ayarlýyoruz.
        transform.position = currentPosition;
    }
}
