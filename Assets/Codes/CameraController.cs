using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Gezinme (Pan) Ayarlarý")]
    public float panSpeed = 20f; // Kamera kaydýrma hýzý
    private Vector3 touchStart;

    [Header("Yakýnlaþtýrma (Zoom) Ayarlarý")]
    public float zoomSpeed = 10f; // Yakýnlaþtýrma hýzý
    public float minZoom = 5f;    // En yakýn zoom seviyesi (daha küçük deðer, daha yakýn)
    public float maxZoom = 20f;   // En uzak zoom seviyesi (daha büyük deðer, daha uzak)

    void Update()
    {
        // Gezinme (Pan) Ýþlemi - Sol Fare Tuþu (Mouse 0)
        if (Input.GetMouseButtonDown(0)) // Fare tuþuna basýldýðýnda baþlangýç noktasýný kaydet
        {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0)) // Fare tuþu basýlýyken kamerayý hareket ettir
        {
            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += direction * panSpeed * Time.deltaTime;

            // Ýsteðe baðlý: Kamera sýnýrlarýný belirle (arka planýn dýþýna çýkmasýný engelle)
            // float minX = -10f, maxX = 10f, minY = -10f, maxY = 10f; // Kendi arka plan boyutlarýna göre ayarla
            // Camera.main.transform.position = new Vector3(
            //     Mathf.Clamp(Camera.main.transform.position.x, minX, maxX),
            //     Mathf.Clamp(Camera.main.transform.position.y, minY, maxY),
            //     Camera.main.transform.z
            // );
        }

        // Yakýnlaþtýrma (Zoom) Ýþlemi - Fare Tekerleði (Mouse ScrollWheel)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Orthographic kamera için 'size' deðerini deðiþtir
            Camera.main.orthographicSize -= scroll * zoomSpeed;
            // Zoom seviyesini minimum ve maksimum sýnýrlar içinde tut
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
        }
    }
}
