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

    // Ýsteðe baðlý: Menüden çýkýþ veya belirli bir ana menü sahnesine dönüþ için
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // "MainMenu" sizin ana menü sahnenizin adý olmalý
        Debug.Log("Ana menüye dönülüyor.");
    }
}
