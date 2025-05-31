using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable] // Bu, Unity Editor'da bu s�n�f�n g�r�nmesini sa�lar.
public class SuspectUIElements
{
    public Image suspectImage; // ��phelinin robot foto�raf�n� g�steren Image
    public TextMeshProUGUI suspectNameText; // ��phelinin ad�n� g�steren Text
    public Button accuseButton; // Bu ��pheliyi su�lama butonu
    public Button exonerateButton; // Bu ��pheliyi aklama butonu

    // ��phelinin butonlar�n�n etkile�imini ayarlayan yard�mc� metot
    public void SetButtonsInteractable(bool interactable)
    {
        if (accuseButton != null) accuseButton.interactable = interactable;
        if (exonerateButton != null) exonerateButton.interactable = interactable;
    }

    // ��phelinin foto�raf�n� karartan metot
    public void DarkenImage()
    {
        if (suspectImage != null)
        {
            suspectImage.color = Color.gray; // Karartmak i�in griye �evir
        }
    }

    // ��phelinin foto�raf rengini s�f�rlayan metot (beyaz/normal hale getirir)
    public void ResetImageColor()
    {
        if (suspectImage != null)
        {
            suspectImage.color = Color.white; // Varsay�lan renge d�nd�r (beyaz)
        }
    }
}
