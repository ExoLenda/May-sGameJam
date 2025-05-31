using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // TextMeshPro için

public class FinalSceneManager : MonoBehaviour // Sýnýf adý
{
    [Header("Þüpheliler Ayarlarý")]
    public Button[] suspectButtons; // Sahnedeki þüpheli seçim butonlarý (4 adet)
    public int correctAnswerIndex = 0; // Doðru þüphelinin indeksi (0'dan baþlar)

    [Header("Fotoðraf Sandýðý UI")]
    public GameObject photoDisplayPrefab; // InvestigationScene'deki PhotoThumbnail prefab'ini kullanacaðýz
    public Transform photosDisplayContainer; // Fotoðraflarýn gösterileceði Container (FinalPhotosContent)

    [Header("Sonuç UI")]
    public Button confirmSuspectButton; // Suçluyu Seç butonu
    public TextMeshProUGUI resultText; // Sonucu gösterecek metin

    private int selectedSuspectIndex = -1; // Seçilen þüphelinin indeksi

    void Start()
    {
        // ChestManager'ýn mevcut olduðundan emin olun.
        if (ChestManager.Instance != null)
        {
            LoadCollectedPhotos(); // Fotoðraflarý yükle
        }
        else
        {
            Debug.LogError("ChestManager.Instance bulunamadý! Fotoðraflar yüklenemeyecek. Lütfen InvestigationScene'deki GameManager objesinde ChestManager'ýn doðru atandýðýndan ve DontDestroyOnLoad ile sahne geçiþlerinde kaldýðýndan emin olun.");
            // Burada hata alýyorsanýz, ChestManager'ýn InvestigationScene'de doðru ayarlandýðýndan emin olun.
        }

        InitializeSuspectButtons(); // Þüpheli butonlarýný ayarla

        if (confirmSuspectButton != null)
        {
            confirmSuspectButton.onClick.AddListener(ConfirmSuspect); // Onay butonuna týklama iþlevini baðla
        }

        resultText.text = ""; // Baþlangýçta sonuç metnini temizle
    }

    void LoadCollectedPhotos()
    {
        // ChestManager'dan toplanan fotoðraflarý al
        List<Texture2D> collectedPhotos = new List<Texture2D>();
        // Null hatasý almamak için ChestManager.Instance'ý tekrar kontrol edin
        if (ChestManager.Instance != null)
        {
            collectedPhotos = ChestManager.Instance.GetCollectedPhotos();
        }
        else
        {
            Debug.LogError("LoadCollectedPhotos: ChestManager.Instance hala null! Fotoðraflar gösterilemiyor.");
            return; // Fonksiyondan çýk
        }

        // Konteynerdeki mevcut tüm fotoðraflarý temizle
        foreach (Transform child in photosDisplayContainer)
        {
            Destroy(child.gameObject);
        }

        // Her fotoðraf için bir UI elementi oluþtur ve göster
        foreach (Texture2D texture in collectedPhotos)
        {
            GameObject photoGO = Instantiate(photoDisplayPrefab, photosDisplayContainer); // PhotoThumbnail prefab'inden yeni bir örnek oluþtur
            Image photoImage = photoGO.GetComponent<Image>(); // Prefab'deki Image bileþenini al

            if (photoImage != null)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                photoImage.sprite = sprite; // Sprite'ý ata

                // Burasý önemli: SetNativeSize() yerine RectTransform'u manuel olarak boyutlandýrýn
                // ÝSTEDÝÐÝNÝZ BOYUTLARI BURAYA YAZIN (örn: 150f, 150f)
                photoGO.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f); // Fotoðrafý 150x150 piksel yap

                // Eðer PhotoThumbnail prefab'inizde Button bileþeni yoksa bu bloðu kaldýrabilirsiniz.
                // Veya týklanma özelliði istenmiyorsa da kaldýrýlabilir.
                Button photoButton = photoGO.GetComponent<Button>();
                if (photoButton != null)
                {
                    photoButton.onClick.RemoveAllListeners();
                    // Þu an için týklanma iþlevi atamýyoruz, sadece dinleyicileri temizliyoruz.
                }
            }
            else
            {
                Debug.LogError("FinalSceneManager: PhotoThumbnail prefab'inizde 'Image' bileþeni bulunamadý! Lütfen prefab'i kontrol edin.");
            }
        }
    }

    void InitializeSuspectButtons()
    {
        for (int i = 0; i < suspectButtons.Length; i++)
        {
            int index = i; // Lambda ifadesi (closure) için geçici deðiþken
            if (suspectButtons[i] != null)
            {
                suspectButtons[i].onClick.RemoveAllListeners(); // Önceki dinleyicileri temizle
                suspectButtons[i].onClick.AddListener(() => SelectSuspect(index)); // Týklama olayýný baðla
                // Butonlarýn varsayýlan renklerini ayarla (seçili deðilken beyaz)
                if (suspectButtons[i].GetComponent<Image>() != null)
                {
                    suspectButtons[i].GetComponent<Image>().color = Color.white;
                }
            }
        }
    }

    void SelectSuspect(int index)
    {
        selectedSuspectIndex = index; // Seçilen þüphelinin indeksini kaydet
        Debug.Log("Þüpheli seçildi: " + index);

        // Seçilen þüphelinin butonunun rengini deðiþtir (görsel geri bildirim)
        for (int i = 0; i < suspectButtons.Length; i++)
        {
            if (suspectButtons[i] != null)
            {
                if (suspectButtons[i].GetComponent<Image>() != null)
                {
                    // Seçilen butonu sarý yap, diðerlerini beyaz
                    suspectButtons[i].GetComponent<Image>().color = (i == selectedSuspectIndex) ? Color.yellow : Color.white;
                }
            }
        }
    }

    void ConfirmSuspect()
    {
        if (selectedSuspectIndex != -1) // Bir þüpheli seçilmiþse
        {
            // Oyuncuya doðru mu yanlýþ mý olduðunu göstermeyeceðiz.
            // Sadece davanýn kapandýðýný belirten genel bir mesaj gösterelim.
            resultText.text = "Kararýnýz verildi ve dava kapandý. Þimdi sonucunu bekleyin..."; // Genel mesaj
            resultText.color = Color.white; // Metin rengini beyaz yapabilirsiniz

            // Seçim yapýldýktan sonra butonlarý pasifleþtir
            if (confirmSuspectButton != null) confirmSuspectButton.interactable = false;
            foreach (Button btn in suspectButtons)
            {
                if (btn != null) btn.interactable = false;
            }
            // Ýsteðe baðlý: Oyunun bittiðini veya bir sonraki adýma geçileceðini bildirmek için baþka bir fonksiyon çaðýrabilirsiniz.
            // Örneðin: SceneManager.LoadScene("MainMenu");
        }
        else // Hiç þüpheli seçilmemiþse
        {
            resultText.text = "Lütfen bir þüpheli seçin."; // Uyarý mesajý
            resultText.color = Color.yellow;
        }
    }
}
