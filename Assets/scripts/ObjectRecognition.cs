using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectRecognition : MonoBehaviour
{
    public Camera photoCamera; // Fotoðraf çeken kamera
    public float detectionRange = 50f; // Kameranýn maksimum görüþ mesafesi

    public void DetectPhotoObjects()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("PhotoTarget");

        foreach (GameObject obj in targets)
        {
            Vector3 viewportPos = photoCamera.WorldToViewportPoint(obj.transform.position);

            // Eðer nesne kameranýn görüþ alanýndaysa (0-1 arasý Viewport deðerleri)
            if (viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1)
            {
                Debug.Log("Fotoðrafa dahil edildi: " + obj.name);
                // Burada oyuncuya puan verebilir veya fotoðraf koleksiyonuna ekleyebilirsin.
            }
        }
    }
}

