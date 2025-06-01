using UnityEngine;
using UnityEngine.SceneManagement; // Bu kütüphaneyi eklemeyi unutma!

public class SahneGecisScripti : MonoBehaviour
{
    // Gitmek istediðin sahnenin adýný buraya yazabilirsin.
    // Public yaparak Unity Inspector'da deðiþtirebilirsin.
    public string sonrakiSahneAdi;

    // Butona týklandýðýnda bu fonksiyon çalýþacak
    public void SonrakiSahneyeGec()
    {
        // Belirtilen isme sahip sahneyi yükle
        SceneManager.LoadScene(sonrakiSahneAdi);
    }

    // Ya da sahne index'ini kullanarak da geçiþ yapabilirsin.
    // Bu durumda sonrakiSahneAdi deðiþkenine ihtiyacýn olmaz.
    public void SahneIndexIleGec(int sahneIndex)
    {
        SceneManager.LoadScene(2);
    }
}
