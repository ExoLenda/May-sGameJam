using UnityEngine;
using TMPro; // TextMeshPro kullan�yorsan�z
using UnityEngine.SceneManagement; // Sahne ge�i�leri i�in

public class GameTimer : MonoBehaviour
{
    [Header("Zaman Ayarlar�")]
    public float gameDuration = 120f; // Saniye cinsinden oyun s�resi (�rn: 2 dakika)
    public TextMeshProUGUI timerText; // UI'daki TextMeshPro bile�eni

    private float currentTime;
    private bool timerIsRunning = false;

    void Start()
    {
        currentTime = gameDuration;
        timerIsRunning = true;
        UpdateTimerUI();
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerUI();
            }
            else
            {
                currentTime = 0;
                timerIsRunning = false;
                Debug.Log("S�re Doldu! Final Sahnesine ge�iliyor.");
                GoToFinalScene(); // S�re doldu�unda Final Sahnesine ge�
            }
        }
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void StopTimer()
    {
        timerIsRunning = false;
    }

    public void GoToFinalScene() // Fonksiyon ad� GoToFinalScene olarak de�i�ti
    {
        StopTimer(); // Zamanlay�c�y� durdur
        // "FinalSahnenizinAdi" yerine, Final Sahnenizin Unity'deki tam ad�n� yaz�n.
        // �rne�in: SceneManager.LoadScene("MyFinalScene");
        SceneManager.LoadScene("FinalScene"); // Buray� kendi Final Sahnenizin ad�yla g�ncelleyin!
    }
}
