// FinalSceneManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // Coroutine'ler için gerekli

public class FinalSceneManager : MonoBehaviour
{
    [Header("Þüpheliler Ayarlarý")]
    public SuspectUIElements[] suspectUIPanels;

    [Header("Hapis Sahnesi Adlarý")]
    public string[] prisonSceneNames;

    [Header("Fotoðraf Sandýðý UI")]
    public GameObject photoDisplayPrefab;
    public Transform photosDisplayContainer;

    [Header("Sonuç UI")]
    public TextMeshProUGUI resultText;

    void Start()
    {
        if (ChestManager.Instance != null)
        {
            LoadCollectedPhotos();
        }
        else
        {
            Debug.LogError("ChestManager.Instance bulunamadý! Fotoðraflar yüklenemeyecek.");
        }

        InitializeSuspectPanels();
        resultText.text = "";
    }

    void LoadCollectedPhotos()
    {
        List<Texture2D> collectedPhotos = new List<Texture2D>();
        if (ChestManager.Instance != null)
        {
            collectedPhotos = ChestManager.Instance.GetCollectedPhotos();
        }
        else
        {
            Debug.LogError("LoadCollectedPhotos: ChestManager.Instance hala null!");
            return;
        }

        foreach (Transform child in photosDisplayContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Texture2D texture in collectedPhotos)
        {
            GameObject photoGO = Instantiate(photoDisplayPrefab, photosDisplayContainer);
            Image photoImage = photoGO.GetComponent<Image>();

            if (photoImage != null)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                photoImage.sprite = sprite;
                photoGO.GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 150f);
            }
            else
            {
                Debug.LogError("FinalSceneManager: PhotoThumbnail prefab'inizde 'Image' bileþeni bulunamadý!");
            }
        }
    }

    void InitializeSuspectPanels()
    {
        for (int i = 0; i < suspectUIPanels.Length; i++)
        {
            int index = i;

            if (suspectUIPanels[index] != null)
            {
                suspectUIPanels[index].ResetImageColor();

                if (suspectUIPanels[index].accuseButton != null)
                {
                    suspectUIPanels[index].accuseButton.onClick.RemoveAllListeners();
                    suspectUIPanels[index].accuseButton.onClick.AddListener(() => AccuseSuspect(index));
                }
                if (suspectUIPanels[index].exonerateButton != null)
                {
                    suspectUIPanels[index].exonerateButton.onClick.RemoveAllListeners();
                    suspectUIPanels[index].exonerateButton.onClick.AddListener(() => ExonerateSuspect(index));
                }

                suspectUIPanels[index].SetButtonsInteractable(true);
            }
        }
    }

    void AccuseSuspect(int suspectIndex)
    {
        if (suspectUIPanels[suspectIndex].suspectImage != null && suspectUIPanels[suspectIndex].suspectImage.color == Color.gray)
        {
            resultText.text = "Bu þüpheli hakkýnda zaten bir karar verildi.";
            resultText.color = Color.blue;
            return;
        }

        resultText.text = "Kararýnýz verildi. Dava kapanýyor...";
        resultText.color = Color.white;

        suspectUIPanels[suspectIndex].DarkenImage();
        suspectUIPanels[suspectIndex].SetButtonsInteractable(false);

        SetAllSuspectButtonsInteractable(false);

        if (suspectIndex >= 0 && suspectIndex < prisonSceneNames.Length)
        {
            // Coroutine'i baþlat
            StartCoroutine(GoToPrisonSceneForSuspect(suspectIndex));
        }
        else
        {
            Debug.LogError("Hapis sahnesi adý 'prisonSceneNames' dizisinde bulunamadý! Ýndeks: " + suspectIndex);
        }
    }

    void ExonerateSuspect(int suspectIndex)
    {
        if (suspectUIPanels[suspectIndex].suspectImage != null && suspectUIPanels[suspectIndex].suspectImage.color == Color.gray)
        {
            resultText.text = "Bu þüpheli hakkýnda zaten bir karar verildi.";
            resultText.color = Color.blue;
            return;
        }

        resultText.text = "Bu þüpheli listeden çýkarýldý.";
        resultText.color = Color.white;

        suspectUIPanels[suspectIndex].DarkenImage();
        suspectUIPanels[suspectIndex].SetButtonsInteractable(false);
    }

    void SetAllSuspectButtonsInteractable(bool interactable)
    {
        for (int i = 0; i < suspectUIPanels.Length; i++)
        {
            if (suspectUIPanels[i] != null)
            {
                suspectUIPanels[i].SetButtonsInteractable(interactable);
            }
        }
    }

    // Coroutine olarak deðiþtirildi
    IEnumerator GoToPrisonSceneForSuspect(int suspectIndex)
    {
        yield return new WaitForSeconds(2f); // 2 saniye bekle

        if (suspectIndex >= 0 && suspectIndex < prisonSceneNames.Length && !string.IsNullOrEmpty(prisonSceneNames[suspectIndex]))
        {
            SceneManager.LoadScene(prisonSceneNames[suspectIndex]);
        }
        else
        {
            Debug.LogError("Hapis sahnesi adý eksik veya geçersiz! Yüklenemiyor. Ýndeks: " + suspectIndex);
        }
    }
}