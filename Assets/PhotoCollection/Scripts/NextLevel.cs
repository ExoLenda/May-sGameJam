using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NextLevel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToNextScene()
    {
        // Mevcut sahnenin build index'ini al
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // Bir sonraki sahneye ge�
        SceneManager.LoadScene(currentSceneIndex + 1);
        Debug.Log($"Bir sonraki sahneye ge�iliyor: {currentSceneIndex + 1}");
    }

    // �ste�e ba�l�: Men�den ��k�� veya belirli bir ana men� sahnesine d�n�� i�in
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // "MainMenu" sizin ana men� sahnenizin ad� olmal�
        Debug.Log("Ana men�ye d�n�l�yor.");
    }
}
