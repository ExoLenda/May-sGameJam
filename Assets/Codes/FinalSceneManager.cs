using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // TextMeshPro i�in

public class FinalSceneManager : MonoBehaviour // S�n�f ad�
{
    [Header("��pheliler Ayarlar�")]
    public Button[] suspectButtons; // Sahnedeki ��pheli se�im butonlar� (4 adet)
    public int correctAnswerIndex = 0; // Do�ru ��phelinin indeksi (0'dan ba�lar)

    [Header("Foto�raf Sand��� UI")]
    public GameObject photoDisplayPrefab; // InvestigationScene'deki PhotoThumbnail prefab'ini kullanaca��z
    public Transform photosDisplayContainer; // Foto�raflar�n g�sterilece�i Container (FinalPhotosContent)

    [Header("Sonu� UI")]
    public Button confirmSuspectButton; // Su�luyu Se� butonu
    public TextMeshProUGUI resultText; // Sonucu g�sterecek metin

    private int selectedSuspectIndex = -1; // Se�ilen ��phelinin indeksi

    void Start()
    {
        // ChestManager'�n mevcut oldu�undan emin olun.
        if (ChestManager.Instance != null)
        {
            LoadCollectedPhotos(); // Foto�raflar� y�kle
        }
        else
        {
            Debug.LogError("ChestManager.Instance bulunamad�! Foto�raflar y�klenemeyecek. L�tfen InvestigationScene'deki GameManager objesinde ChestManager'�n do�ru atand���ndan ve DontDestroyOnLoad ile sahne ge�i�lerinde kald���ndan emin olun.");
            // Burada hata al�yorsan�z, ChestManager'�n InvestigationScene'de do�ru ayarland���ndan emin olun.
        }

        InitializeSuspectButtons(); // ��pheli butonlar�n� ayarla

        if (confirmSuspectButton != null)
        {
            confirmSuspectButton.onClick.AddListener(ConfirmSuspect); // Onay butonuna t�klama i�levini ba�la
        }

        resultText.text = ""; // Ba�lang��ta sonu� metnini temizle
    }

    void LoadCollectedPhotos()
    {
        // ChestManager'dan toplanan foto�raflar� al
        List<Texture2D> collectedPhotos = new List<Texture2D>();
        // Null hatas� almamak i�in ChestManager.Instance'� tekrar kontrol edin
        if (ChestManager.Instance != null)
        {
            collectedPhotos = ChestManager.Instance.GetCollectedPhotos();
        }
        else
        {
            Debug.LogError("LoadCollectedPhotos: ChestManager.Instance hala null! Foto�raflar g�sterilemiyor.");
            return; // Fonksiyondan ��k
        }

        // Konteynerdeki mevcut t�m foto�raflar� temizle
        foreach (Transform child in photosDisplayContainer)
        {
            Destroy(child.gameObject);
        }

        // Her foto�raf i�in bir UI elementi olu�tur ve g�ster
        foreach (Texture2D texture in collectedPhotos)
        {
            GameObject photoGO = Instantiate(photoDisplayPrefab, photosDisplayContainer); // PhotoThumbnail prefab'inden yeni bir �rnek olu�tur
            Image photoImage = photoGO.GetComponent<Image>(); // Prefab'deki Image bile�enini al

            if (photoImage != null)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                photoImage.sprite = sprite; // Sprite'� ata

                // Buras� �nemli: SetNativeSize() yerine RectTransform'u manuel olarak boyutland�r�n
                // �STED���N�Z BOYUTLARI BURAYA YAZIN (�rn: 150f, 150f)
                photoGO.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f); // Foto�raf� 150x150 piksel yap

                // E�er PhotoThumbnail prefab'inizde Button bile�eni yoksa bu blo�u kald�rabilirsiniz.
                // Veya t�klanma �zelli�i istenmiyorsa da kald�r�labilir.
                Button photoButton = photoGO.GetComponent<Button>();
                if (photoButton != null)
                {
                    photoButton.onClick.RemoveAllListeners();
                    // �u an i�in t�klanma i�levi atam�yoruz, sadece dinleyicileri temizliyoruz.
                }
            }
            else
            {
                Debug.LogError("FinalSceneManager: PhotoThumbnail prefab'inizde 'Image' bile�eni bulunamad�! L�tfen prefab'i kontrol edin.");
            }
        }
    }

    void InitializeSuspectButtons()
    {
        for (int i = 0; i < suspectButtons.Length; i++)
        {
            int index = i; // Lambda ifadesi (closure) i�in ge�ici de�i�ken
            if (suspectButtons[i] != null)
            {
                suspectButtons[i].onClick.RemoveAllListeners(); // �nceki dinleyicileri temizle
                suspectButtons[i].onClick.AddListener(() => SelectSuspect(index)); // T�klama olay�n� ba�la
                // Butonlar�n varsay�lan renklerini ayarla (se�ili de�ilken beyaz)
                if (suspectButtons[i].GetComponent<Image>() != null)
                {
                    suspectButtons[i].GetComponent<Image>().color = Color.white;
                }
            }
        }
    }

    void SelectSuspect(int index)
    {
        selectedSuspectIndex = index; // Se�ilen ��phelinin indeksini kaydet
        Debug.Log("��pheli se�ildi: " + index);

        // Se�ilen ��phelinin butonunun rengini de�i�tir (g�rsel geri bildirim)
        for (int i = 0; i < suspectButtons.Length; i++)
        {
            if (suspectButtons[i] != null)
            {
                if (suspectButtons[i].GetComponent<Image>() != null)
                {
                    // Se�ilen butonu sar� yap, di�erlerini beyaz
                    suspectButtons[i].GetComponent<Image>().color = (i == selectedSuspectIndex) ? Color.yellow : Color.white;
                }
            }
        }
    }

    void ConfirmSuspect()
    {
        if (selectedSuspectIndex != -1) // Bir ��pheli se�ilmi�se
        {
            // Oyuncuya do�ru mu yanl�� m� oldu�unu g�stermeyece�iz.
            // Sadece davan�n kapand���n� belirten genel bir mesaj g�sterelim.
            resultText.text = "Karar�n�z verildi ve dava kapand�. �imdi sonucunu bekleyin..."; // Genel mesaj
            resultText.color = Color.white; // Metin rengini beyaz yapabilirsiniz

            // Se�im yap�ld�ktan sonra butonlar� pasifle�tir
            if (confirmSuspectButton != null) confirmSuspectButton.interactable = false;
            foreach (Button btn in suspectButtons)
            {
                if (btn != null) btn.interactable = false;
            }
            // �ste�e ba�l�: Oyunun bitti�ini veya bir sonraki ad�ma ge�ilece�ini bildirmek i�in ba�ka bir fonksiyon �a��rabilirsiniz.
            // �rne�in: SceneManager.LoadScene("MainMenu");
        }
        else // Hi� ��pheli se�ilmemi�se
        {
            resultText.text = "L�tfen bir ��pheli se�in."; // Uyar� mesaj�
            resultText.color = Color.yellow;
        }
    }
}
