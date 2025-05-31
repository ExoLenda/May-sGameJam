using UnityEngine;

public class CameraBoundary : MonoBehaviour
{
    // Bu de�i�kenler, kameran�n gidebilece�i minimum ve maksimum X ve Y koordinatlar�n� belirler.
    // Bu de�erleri Unity Editor'dan kolayca ayarlayabilece�iz.
    [Header("Kamera Hareket Limitleri")]
    public float minX = -10f; // Kameran�n sola gidebilece�i en uzak nokta
    public float maxX = 10f;  // Kameran�n sa�a gidebilece�i en uzak nokta
    public float minY = -5f;  // Kameran�n a�a�� gidebilece�i en uzak nokta
    public float maxY = 5f;   // Kameran�n yukar� gidebilece�i en uzak nokta

    void LateUpdate() // Bu metod, her karede kamera hareket ettikten sonra �al���r.
    {
        // Kameran�n �u anki konumunu al�yoruz
        Vector3 currentPosition = transform.position;

        // X koordinat�n� belirli s�n�rlar aras�na s�k��t�r�yoruz.
        // �rne�in, X 12 ise ve maxX 10 ise, X 10'a �ekilir.
        // X -12 ise ve minX -10 ise, X -10'a �ekilir.
        currentPosition.x = Mathf.Clamp(currentPosition.x, minX, maxX);

        // Y koordinat�n� da ayn� �ekilde s�k��t�r�yoruz
        currentPosition.y = Mathf.Clamp(currentPosition.y, minY, maxY);

        // Kameran�n konumunu s�k��t�r�lm�� (limitlenmi�) yeni pozisyona ayarl�yoruz.
        transform.position = currentPosition;
    }
}
