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

    public void GoToSuspectScene()
    {
        // Bir sonraki sahneye ge�
        SceneManager.LoadScene(3);
        Debug.Log($"Bir sonraki sahneye ge�iliyor:");
    }

    // �ste�e ba�l�: Men�den ��k�� veya belirli bir ana men� sahnesine d�n�� i�in
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // "MainMenu" sizin ana men� sahnenizin ad� olmal�
        Debug.Log("Ana men�ye d�n�l�yor.");
    }
    public void SahneIndexIleGec(int sahneIndex)
    {
        SceneManager.LoadScene(sahneIndex);
    }
    public void GoToSceneAfterDelay(int sceneIndex)
    {
        // Coroutine ba�lat�l�yor
        StartCoroutine(LoadSceneAfterDelayCoroutine(sceneIndex));
    }
    private IEnumerator LoadSceneAfterDelayCoroutine(int sceneIndex)
    {
        Debug.Log($"Sahne {sceneIndex}'e {3} saniye sonra ge�ilecek...");
        yield return new WaitForSeconds(3); // Belirtilen s�re kadar bekle

        // Gecikme bittikten sonra sahneye ge�
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Ge�ersiz sahne index'i: {sceneIndex}. Build Settings'deki sahne say�s�n� kontrol edin.");
            yield break; // Coroutine'i sonland�r
        }
        SceneManager.LoadScene(sceneIndex);
        Debug.Log($"Gecikme sonras� sahneye ge�ildi: {sceneIndex}");
    }
}
