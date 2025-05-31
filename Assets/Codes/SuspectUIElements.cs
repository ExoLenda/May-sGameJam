using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable] // Bu, Unity Editor'da bu sýnýfýn görünmesini saðlar.
public class SuspectUIElements
{
    public Image suspectImage; // Þüphelinin robot fotoðrafýný gösteren Image
    public TextMeshProUGUI suspectNameText; // Þüphelinin adýný gösteren Text
    public Button accuseButton; // Bu þüpheliyi suçlama butonu
    public Button exonerateButton; // Bu þüpheliyi aklama butonu

    // Þüphelinin butonlarýnýn etkileþimini ayarlayan yardýmcý metot
    public void SetButtonsInteractable(bool interactable)
    {
        if (accuseButton != null) accuseButton.interactable = interactable;
        if (exonerateButton != null) exonerateButton.interactable = interactable;
    }

    // Þüphelinin fotoðrafýný karartan metot
    public void DarkenImage()
    {
        if (suspectImage != null)
        {
            suspectImage.color = Color.gray; // Karartmak için griye çevir
        }
    }

    // Þüphelinin fotoðraf rengini sýfýrlayan metot (beyaz/normal hale getirir)
    public void ResetImageColor()
    {
        if (suspectImage != null)
        {
            suspectImage.color = Color.white; // Varsayýlan renge döndür (beyaz)
        }
    }
}
