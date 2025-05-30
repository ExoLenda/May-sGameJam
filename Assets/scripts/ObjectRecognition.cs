using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectRecognition : MonoBehaviour
{
    public Camera photoCamera; // Foto�raf �eken kamera
    public float detectionRange = 50f; // Kameran�n maksimum g�r�� mesafesi

    public void DetectPhotoObjects()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("PhotoTarget");

        foreach (GameObject obj in targets)
        {
            Vector3 viewportPos = photoCamera.WorldToViewportPoint(obj.transform.position);

            // E�er nesne kameran�n g�r�� alan�ndaysa (0-1 aras� Viewport de�erleri)
            if (viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1)
            {
                Debug.Log("Foto�rafa dahil edildi: " + obj.name);
                // Burada oyuncuya puan verebilir veya foto�raf koleksiyonuna ekleyebilirsin.
            }
        }
    }
}

