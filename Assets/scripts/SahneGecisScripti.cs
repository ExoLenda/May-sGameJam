using UnityEngine;
using UnityEngine.SceneManagement; // Bu k�t�phaneyi eklemeyi unutma!

public class SahneGecisScripti : MonoBehaviour
{
    // Gitmek istedi�in sahnenin ad�n� buraya yazabilirsin.
    // Public yaparak Unity Inspector'da de�i�tirebilirsin.
    public string sonrakiSahneAdi;

    // Butona t�kland���nda bu fonksiyon �al��acak
    public void SonrakiSahneyeGec()
    {
        // Belirtilen isme sahip sahneyi y�kle
        SceneManager.LoadScene(sonrakiSahneAdi);
    }

    // Ya da sahne index'ini kullanarak da ge�i� yapabilirsin.
    // Bu durumda sonrakiSahneAdi de�i�kenine ihtiyac�n olmaz.
    public void SahneIndexIleGec(int sahneIndex)
    {
        SceneManager.LoadScene(2);
    }
}
