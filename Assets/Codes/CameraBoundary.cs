using UnityEngine;

public class CameraBoundary : MonoBehaviour
{
    // Bu değişkenler, kameranın gidebileceği minimum ve maksimum X ve Y koordinatlarını belirler.
    // Bu değerleri Unity Editor'dan kolayca ayarlayabileceğiz.
    [Header("Kamera Hareket Limitleri")]
    public float minX = -10f; // Kameranın sola gidebileceği en uzak nokta
    public float maxX = 10f;  // Kameranın sağa gidebileceği en uzak nokta
    public float minY = -5f;  // Kameranın aşağı gidebileceği en uzak nokta
    public float maxY = 5f;   // Kameranın yukarı gidebileceği en uzak nokta

    void LateUpdate() // Bu metod, her karede kamera hareket ettikten sonra çalışır.
    {
        // Kameranın şu anki konumunu alıyoruz
        Vector3 currentPosition = transform.position;

        // X koordinatını belirli sınırlar arasına sıkıştırıyoruz.
        // Örneğin, X 12 ise ve maxX 10 ise, X 10'a çekilir.
        // X -12 ise ve minX -10 ise, X -10'a çekilir.
        currentPosition.x = Mathf.Clamp(currentPosition.x, minX, maxX);

        // Y koordinatını da aynı şekilde sıkıştırıyoruz
        currentPosition.y = Mathf.Clamp(currentPosition.y, minY, maxY);

        // Kameranın konumunu sıkıştırılmış (limitlenmiş) yeni pozisyona ayarlıyoruz.
        transform.position = currentPosition;
    }
}
