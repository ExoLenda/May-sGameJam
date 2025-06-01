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
        // Bir sonraki sahneye geç
        SceneManager.LoadScene(currentSceneIndex + 1);
        Debug.Log($"Bir sonraki sahneye geçiliyor: {currentSceneIndex + 1}");
    }

    public void GoToSuspectScene()
    {
        // Bir sonraki sahneye geç
        SceneManager.LoadScene(3);
        Debug.Log($"Bir sonraki sahneye geçiliyor:");
    }

    // Ýsteðe baðlý: Menüden çýkýþ veya belirli bir ana menü sahnesine dönüþ için
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // "MainMenu" sizin ana menü sahnenizin adý olmalý
        Debug.Log("Ana menüye dönülüyor.");
    }
    public void SahneIndexIleGec(int sahneIndex)
    {
        SceneManager.LoadScene(sahneIndex);
    }
    public void GoToSceneAfterDelay(int sceneIndex)
    {
        // Coroutine baþlatýlýyor
        StartCoroutine(LoadSceneAfterDelayCoroutine(sceneIndex));
    }
    private IEnumerator LoadSceneAfterDelayCoroutine(int sceneIndex)
    {
        Debug.Log($"Sahne {sceneIndex}'e {3} saniye sonra geçilecek...");
        yield return new WaitForSeconds(3); // Belirtilen süre kadar bekle

        // Gecikme bittikten sonra sahneye geç
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Geçersiz sahne index'i: {sceneIndex}. Build Settings'deki sahne sayýsýný kontrol edin.");
            yield break; // Coroutine'i sonlandýr
        }
        SceneManager.LoadScene(sceneIndex);
        Debug.Log($"Gecikme sonrasý sahneye geçildi: {sceneIndex}");
    }
}
