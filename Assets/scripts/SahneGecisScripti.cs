using UnityEngine;
using UnityEngine.SceneManagement; // Bu kütüphaneyi eklemeyi unutma!

public class SahneGecisScripti : MonoBehaviour
{
    // Gitmek istediğin sahnenin adını buraya yazabilirsin.
    // Public yaparak Unity Inspector'da değiştirebilirsin.
    public string sonrakiSahneAdi;

    // Butona tıklandığında bu fonksiyon çalışacak
    public void SonrakiSahneyeGec()
    {
        // Belirtilen isme sahip sahneyi yükle
        SceneManager.LoadScene(sonrakiSahneAdi);
    }

    // Ya da sahne index'ini kullanarak da geçiş yapabilirsin.
    // Bu durumda sonrakiSahneAdi değişkenine ihtiyacın olmaz.
    public void SahneIndexIleGec(int sahneIndex)
    {
        SceneManager.LoadScene(2);
    }
}
