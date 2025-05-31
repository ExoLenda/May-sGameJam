using UnityEngine;
using TMPro; // TextMeshPro kullanýyorsanýz
using UnityEngine.SceneManagement; // Sahne geçiþleri için

public class GameTimer : MonoBehaviour
{
    [Header("Zaman Ayarlarý")]
    public float gameDuration = 120f; // Saniye cinsinden oyun süresi (örn: 2 dakika)
    public TextMeshProUGUI timerText; // UI'daki TextMeshPro bileþeni

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
                Debug.Log("Süre Doldu! Final Sahnesine geçiliyor.");
                GoToFinalScene(); // Süre dolduðunda Final Sahnesine geç
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

    public void GoToFinalScene() // Fonksiyon adý GoToFinalScene olarak deðiþti
    {
        StopTimer(); // Zamanlayýcýyý durdur
        // "FinalSahnenizinAdi" yerine, Final Sahnenizin Unity'deki tam adýný yazýn.
        // Örneðin: SceneManager.LoadScene("MyFinalScene");
        SceneManager.LoadScene("FinalScene"); // Burayý kendi Final Sahnenizin adýyla güncelleyin!
    }
}
