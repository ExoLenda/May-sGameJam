using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Gezinme (Pan) Ayarlar�")]
    public float panSpeed = 20f; // Kamera kayd�rma h�z�
    private Vector3 touchStart;

    [Header("Yak�nla�t�rma (Zoom) Ayarlar�")]
    public float zoomSpeed = 10f; // Yak�nla�t�rma h�z�
    public float minZoom = 5f;    // En yak�n zoom seviyesi (daha k���k de�er, daha yak�n)
    public float maxZoom = 20f;   // En uzak zoom seviyesi (daha b�y�k de�er, daha uzak)

    void Update()
    {
        // Gezinme (Pan) ��lemi - Sol Fare Tu�u (Mouse 0)
        if (Input.GetMouseButtonDown(0)) // Fare tu�una bas�ld���nda ba�lang�� noktas�n� kaydet
        {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0)) // Fare tu�u bas�l�yken kameray� hareket ettir
        {
            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += direction * panSpeed * Time.deltaTime;

            // �ste�e ba�l�: Kamera s�n�rlar�n� belirle (arka plan�n d���na ��kmas�n� engelle)
            // float minX = -10f, maxX = 10f, minY = -10f, maxY = 10f; // Kendi arka plan boyutlar�na g�re ayarla
            // Camera.main.transform.position = new Vector3(
            //     Mathf.Clamp(Camera.main.transform.position.x, minX, maxX),
            //     Mathf.Clamp(Camera.main.transform.position.y, minY, maxY),
            //     Camera.main.transform.z
            // );
        }

        // Yak�nla�t�rma (Zoom) ��lemi - Fare Tekerle�i (Mouse ScrollWheel)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Orthographic kamera i�in 'size' de�erini de�i�tir
            Camera.main.orthographicSize -= scroll * zoomSpeed;
            // Zoom seviyesini minimum ve maksimum s�n�rlar i�inde tut
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
        }
    }
}
