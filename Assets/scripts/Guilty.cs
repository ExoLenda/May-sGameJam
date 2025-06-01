using UnityEngine;
using UnityEngine.UI;

public class Guilty : MonoBehaviour
{
    public Image targetButtonImage;
    public float targetAlpha = 1f;

    void Start()
    {
        // Eðer atanmýþsa, baþlangýçta opaklýðý 0 yap
        if (targetButtonImage != null)
        {
            Color initialColor = targetButtonImage.color;
            targetButtonImage.color = new Color(initialColor.r, initialColor.r, initialColor.b, 0f); // Baþlangýçta Alpha 0
        }
    }

    public void SetImageAlphaToFull()
    {
        if (targetButtonImage != null)
        {
            Color currentColor = targetButtonImage.color;
            targetButtonImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);

            Debug.Log("SucluIsaretleyiciDugme'nin opaklýðý " + (targetAlpha * 100) + "% olarak ayarlandý.");
        }
        else
        {
            Debug.LogWarning("Target Button Image Inspector'da atanmamýþ!");
        }
    }
}