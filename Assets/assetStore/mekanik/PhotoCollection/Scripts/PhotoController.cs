using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro; // TMPro kullanýlmýyorsa silinebilir
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DynamicPhotoCamera
{
    /// Controls photo capture, management, and visualization functionality.
    /// Handles screenshot creation, photo storage, and UI interaction.
    public class PhotoController : MonoBehaviour
    {
        [Header("Custom Events")]
        [Tooltip("Invoked when a new photo (Texture2D) has been captured and is ready.")]
        public UnityEvent<Texture2D> OnPhotoCaptureComplete;

        #region Component References
        // Main input controller reference
        public InputController inputController;
        // Sound manager
        public AudioController audioManager;
        // UI manager for photo-related elements
        public PhotoUIManager uiManager;
        // Controls cursor camera movement
        public CursorCam pointerOnScreen; // Eðer kullanýlmýyorsa kaldýrýlabilir
        #endregion

        #region Photo Settings
        // Total number of stored sprites
        [HideInInspector] public int howManySprites;
        // Temporary photocard reference
        private GameObject photocard;
        // Photo prefab template
        [SerializeField] private GameObject photo; // Photo prefab template
        // Current screenshot sprite (Bu deðiþkene artýk ihtiyacýnýz yok, çünkü doðrudan galeriye Texture2D gönderiyoruz)
        // private Sprite screenShotSprite;
        // Full screenshot texture (Bu deðiþkene artýk ihtiyacýnýz yok)
        // private Texture2D screenShot;
        // Cropped screenshot texture
        private Texture2D croppedScreenshot;
        // Render texture for capture
        private RenderTexture targetTexture;
        #endregion

        #region Capture Settings
        [Header("Capture Settings")]
        /// The size of the cropped portion of the screenshot.
        [Tooltip("Size of the square region cropped from the center of the screenshot in pixels.")]
        [Range(1, 500)]
        [SerializeField] private int cropSize = 100;

        [Tooltip("Select the desired RenderTexture format.")]
        [SerializeField] private RenderTextureFormat renderTextureFormat = RenderTextureFormat.Default;

        [Tooltip("Set the depth value for the RenderTexture.")]
        [SerializeField] private int depth = 24;

        // Horizontal resolution of the screenshots captured in pixels.
        private int resWidth = 906; // Bu deðerler inputController.currentCamera'dan alýnacak
        // Vertical resolution of the screenshots captured in pixels.
        private int resHeight = 419; // Bu deðerler inputController.currentCamera'dan alýnacak

        public float sizeMini = 0.5f; // Bu deðiþkenler ShufflePositions metodunda kullanýlýyor
        public float sizeNormal = 0.7f;
        public float sizeMax = 1f;

        // Position dot return state (Kullanýlmýyorsa silinebilir)
        private bool returnDot;
        #endregion

        #region Photo Management
        // Currently moving photo card (Kullanýlmýyorsa silinebilir)
        [HideInInspector] public PhotoPrefab movingCard;
        // Stored card positions (Kullanýlmýyorsa silinebilir)
        private List<Vector3> positionsCards;
        // All photo card instances
        public List<PhotoPrefab> allCards;
        // Card sequence order
        private List<int> subsequenceCards;
        // Board X position (Kullanýlmýyorsa silinebilir)
        private int boardX;
        #endregion

        #region State Variables
        // Initialization state
        private bool initialReady;
        // State adjustment value (Kullanýlmýyorsa silinebilir)
        private float value;
        #endregion

        #region Constants
        // Photo object reference (Kullanýlmýyorsa silinebilir)
        private GameObject photocardObj;
        // Current mouse position (Sadece MakeScrenshot metodunda parametre olarak kullanýlýyor, burada tanýmlamaya gerek yok)
        // private Vector2 mousePosition;
        #endregion

        // Initializes system and loads saved photos
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);
            var globalData = PhotoStorageManager.LoadGlobalData();
            howManySprites = globalData.photoCount;

            // Null kontrolleri ekleyelim
            if (inputController != null)
            {
                inputController.photoController = this;
            }
            else
            {
                Debug.LogError("[PhotoController] InputController Start metodunda NULL! Atandýðýndan emin olun.");
            }

            if (uiManager != null)
            {
                uiManager.photoController = this;
            }
            else
            {
                Debug.LogError("[PhotoController] UIManager Start metodunda NULL! Atandýðýndan emin olun.");
            }

            if (!initialReady)
            {
                initialReady = true;
                List<GameObject> toTurnOff = new List<GameObject>();

                subsequenceCards = LoadSequence();

                // EXTENSION POINT: Modify photo loading logic here
                for (int i = 0; i < subsequenceCards.Count; i++)
                {
                    // Photo prefab'ý ve Sprite atamasý artýk PhotoGalleryManager tarafýndan yapýlýyor
                    // Bu kýsýmdaki mantýk, eski asset'in kendi galeri sistemini yüklüyor gibi görünüyor.
                    // Eðer sadece PhotoGalleryManager'ý kullanacaksanýz, bu for döngüsünün iþlevini gözden geçirmelisiniz.
                    // Þimdilik hata vermemesi için býrakýyorum.
                    photocard = Instantiate(photo, transform.position, transform.rotation, uiManager.photoHolder.transform);
                    photocard.transform.rotation = new Quaternion(0, 0, 0, 0);
                    photocard.transform.localPosition = new Vector3(0, 0, 0);
                    Sprite loadedSprite = LoadSpriteFromDisk("photo" + subsequenceCards[i]);

                    // Null kontrolü
                    if (photocard.GetComponent<Image>() != null)
                    {
                        photocard.GetComponent<Image>().sprite = loadedSprite;
                    }
                    else
                    {
                        Debug.LogWarning($"[PhotoController] Photo prefab'ýnda Image bileþeni bulunamadý: {photocard.name}");
                    }

                    PhotoPrefab script = photocard.GetComponent<PhotoPrefab>();
                    if (script == null)
                    {
                        Debug.LogError($"[PhotoController] Photo prefab'ýnda PhotoPrefab script'i bulunamadý: {photocard.name}");
                        Destroy(photocard); // Script yoksa objeyi yok et
                        continue; // Döngünün devamýný atla
                    }

                    if (photocard.GetComponent<Image>().sprite == null)
                    {
                        script.thisNumberIndex = subsequenceCards[i];
                        toTurnOff.Add(photocard);
                    }
                    else
                    {
                        script.thisNumberIndex = subsequenceCards[i];
                        allCards.Add(script);
                        script.photoController = this;
                    }
                }
                if (uiManager.photoHolder.transform.childCount > 0)
                {
                    uiManager.temporaltargetSquarePos = uiManager.photoHolder.transform.GetChild(uiManager.photoHolder.transform.childCount - 1).gameObject;
                }
                for (int i = 0; i < toTurnOff.Count; i++)
                {
                    Destroy(toTurnOff[i].gameObject);
                }
                for (int i = 0; i < allCards.Count; i++)
                {
                    allCards[i].TheStart();
                }
                SortPhotos();
                ShufflePositions();
            }
            if (uiManager != null) // Null kontrolü ekle
            {
                uiManager.CanPhotos();
            }
        }

        // Loads sprite from storage
        public Sprite LoadSpriteFromDisk(string fileName, int maxSizeInMB = 10)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);

            if (!File.Exists(filePath))
            {
                // Debug.LogWarning($"[PhotoController] Dosya bulunamadý: {filePath}"); // Her resim için uyarý istemiyorsanýz kapatýn
                return null;
            }

            if (new FileInfo(filePath).Length > maxSizeInMB * 1024 * 1024)
            {
                Debug.LogError($"[PhotoController] Dosya {fileName} {maxSizeInMB}MB limitini aþýyor.");
                return null;
            }

            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes); // Resim boyutunu otomatik ayarlar
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            return sprite;
        }

        // Removes photo from system
        public void DeleteIt(Image thisObj, PhotoPrefab photo)
        {
            if (uiManager != null) // Null kontrolü
            {
                uiManager.CantPhotos();
                uiManager.ResetPhotos();
                uiManager.cantPhoto = true;
            }
            else
            {
                Debug.LogWarning("[PhotoController] UIManager DeleteIt metodunda NULL!");
            }

            string filePathToDelete = Path.Combine(Application.persistentDataPath, "photo" + photo.thisNumberIndex);
            DeleteFile(filePathToDelete);
            allCards.Remove(photo);
            Destroy(photo.gameObject);
            SortPhotos();
            ShufflePositions();

            movingCard = null;
            Debug.Log("Screenshot deleted.");
        }

        // Removes file from disk
        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[PhotoController] Dosya silindi: {filePath}"); // Eklendi
            }
            else
            {
                Debug.LogWarning($"[PhotoController] Silinecek dosya bulunamadý: {filePath}"); // Eklendi
            }
        }

        // Updates photo order
        public void SortPhotos()
        {
            SaveCurrentSequence(allCards);
        }

        // Retrieves saved photo sequence
        public List<int> LoadSequence()
        {
            var globalData = PhotoStorageManager.LoadGlobalData();
            return globalData.sequence;
        }

        // Stores current photo sequence
        public void SaveCurrentSequence(List<PhotoPrefab> allCards)
        {
            subsequenceCards = new List<int>();
            HashSet<int> seenIndices = new HashSet<int>();

            for (int i = 0; i < allCards.Count; i++)
            {
                if (allCards[i] == null) // NULL kontrolü eklendi
                {
                    Debug.LogWarning($"[PhotoController] allCards listesinde NULL bir eleman bulundu, atlanýyor.");
                    continue;
                }
                int currentIndex = allCards[i].thisNumberIndex;
                if (!seenIndices.Contains(currentIndex))
                {
                    subsequenceCards.Add(currentIndex);
                    seenIndices.Add(currentIndex);
                }
            }

            var globalData = PhotoStorageManager.LoadGlobalData();
            globalData.sequence = subsequenceCards;
            PhotoStorageManager.SaveGlobalData(globalData);
        }

        // Updates photo positions
        public void ShufflePositions()
        {
            if (uiManager == null || uiManager.photoHolder == null) // Null kontrolü
            {
                Debug.LogError("[PhotoController] ShufflePositions: UIManager veya PhotoHolder NULL!");
                return;
            }

            for (int i = 0; i < allCards.Count; i++)
            {
                if (allCards[i] == null) // NULL kontrolü eklendi
                {
                    Debug.LogWarning($"[PhotoController] allCards listesinde NULL bir PhotoPrefab bulundu index: {i}, atlanýyor.");
                    continue;
                }
                if (allCards[i].gameObject == null) // NULL kontrolü eklendi
                {
                    Debug.LogWarning($"[PhotoController] allCards[{i}] içindeki gameObject NULL, atlanýyor.");
                    continue;
                }

                allCards[i].gameObject.transform.SetParent(uiManager.photoHolder.transform);
                allCards[i].gameObject.transform.SetSiblingIndex(allCards[i].thisNumberIndex);
                allCards[i].gameObject.transform.localScale = new Vector3(sizeMini, sizeMini, sizeMini);
            }
        }

        // Creates and saves screenshot
        public void MakeScrenshot(Vector2 mousePosition)
        {
            if (movingCard != null || (uiManager != null && uiManager.cantPhoto)) // uiManager null kontrolü eklendi
                return;

            if (inputController == null)
            {
                Debug.LogError("[PhotoController] InputController referansý NULL! Lütfen Inspector'dan atayýn.");
                return;
            }
            if (inputController.currentCamera == null)
            {
                Debug.LogError("[PhotoController] InputController.currentCamera referansý NULL! Fotoðraf çekilemez.");
                return;
            }

            RenderTexture targetTexture = null;
            try
            {
                // Take screenshot
                resWidth = inputController.currentCamera.pixelWidth;
                resHeight = inputController.currentCamera.pixelHeight;
                Debug.Log($"[PhotoController] Kamera çözünürlüðü: {resWidth}x{resHeight}");

                Texture2D screenShotTexture = new Texture2D(resWidth, resHeight); // Yerel deðiþken olarak tanýmlandý

                Debug.Log($"[PhotoController] RenderTexture.GetTemporary çaðrýlýyor...");
                targetTexture = RenderTexture.GetTemporary(resWidth, resHeight, depth, renderTextureFormat);
                Debug.Log($"[PhotoController] targetTexture oluþturuldu: {targetTexture != null}");

                Debug.Log($"[PhotoController] Kameranýn targetTexture'ý ayarlanýyor...");
                inputController.currentCamera.targetTexture = targetTexture;

                Debug.Log($"[PhotoController] Kameranýn Render metodu çaðrýlýyor...");
                inputController.currentCamera.Render();
                Debug.Log($"[PhotoController] RenderTexture.active ayarlanýyor...");
                RenderTexture.active = targetTexture;

                screenShotTexture.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                screenShotTexture.Apply();

                inputController.currentCamera.targetTexture = null; // Kameranýn hedef dokusunu temizle

                // Crop screenshot
                int clickX = Mathf.RoundToInt(mousePosition.x);
                int clickY = Mathf.RoundToInt(mousePosition.y);

                // Ekranýn dýþýna taþmayý önlemek için Clamp
                int startX = Mathf.Clamp(clickX - cropSize / 2, 0, screenShotTexture.width - cropSize);
                int startY = Mathf.Clamp(clickY - cropSize / 2, 0, screenShotTexture.height - cropSize);

                // Eðer cropSize kamera çözünürlüðünden büyükse, hata vermemek için Clamp deðerlerini kontrol edin.
                // Veya GetPixels metodunun boyutunu da clamp edin.
                int actualCropWidth = Mathf.Min(cropSize, screenShotTexture.width - startX);
                int actualCropHeight = Mathf.Min(cropSize, screenShotTexture.height - startY);


                Color[] pixels = screenShotTexture.GetPixels(startX, startY, actualCropWidth, actualCropHeight);
                croppedScreenshot = new Texture2D(actualCropWidth, actualCropHeight); // Boyutlarý actualCropWidth, actualCropHeight olarak ayarlandý
                croppedScreenshot.SetPixels(pixels);
                croppedScreenshot.Apply();

                if (croppedScreenshot != null && croppedScreenshot.width > 0 && croppedScreenshot.height > 0)
                {
                    Debug.Log($"[PhotoController] Cropped Screenshot Oluþturuldu. Boyut: {croppedScreenshot.width}x{croppedScreenshot.height}");

                    // Geçici olarak diske kaydetme (hata ayýklama için çok faydalýdýr!)
                    byte[] bytes = croppedScreenshot.EncodeToPNG();
                    string tempFilePath = Path.Combine(Application.persistentDataPath, "debug_cropped_photo_" + howManySprites + ".png");
                    File.WriteAllBytes(tempFilePath, bytes);
                    Debug.Log($"[PhotoController] Kýrpýlmýþ fotoðraf geçici olarak kaydedildi: {tempFilePath}");
                }
                else
                {
                    Debug.LogError("[PhotoController] Cropped Screenshot NULL veya geçersiz boyutlarda!");
                    return; // Eðer croppedScreenshot geçersizse, kalan iþlemleri yapma
                }

                // >>>>>>>>>>> BURADA TEK SEFER ÇAÐRILSIN! <<<<<<<<<<<
                // Asset'in kendi fotoðraf yönetimi yerine, sizin PhotoGalleryManager'a iletmeniz için bu event yeterli.
                OnPhotoCaptureComplete?.Invoke(croppedScreenshot);


                // Aþaðýdaki kýsýmlar, asset'in kendi galeri yönetimi ve obje tanýma mantýðýdýr.
                // Eðer kendi PhotoGalleryManager ve PhotoCaptureAndDetection'ý kullanýyorsanýz,
                // bu kodlarý tamamen SÝLEBÝLÝRSÝNÝZ. Aksi takdirde, her çaðrýya NULL kontrolü eklemelisiniz.

                // if (screenShotSprite == null) screenShotSprite = Sprite.Create(...); // Bu satýrý kaldýrdýk, çünkü Sprite.Create burada tekrar yapýlýyordu
                // string fileName = "photo" + howManySprites;
                // photocardObj = Instantiate(photo, transform.position, transform.rotation, uiManager.photoHolder.transform);
                // uiManager.temporaltargetSquarePos = uiManager.photoHolder.transform.GetChild(uiManager.photoHolder.transform.childCount - 1).gameObject;

                // PhotoPrefab script = photocardObj.GetComponent<PhotoPrefab>();
                // script.thisNumberIndex = howManySprites;
                // script.photoController = this;
                // script.TheStart();

                // if (!allCards.Contains(script))
                //     allCards.Add(script);

                // photocardObj.transform.localRotation = Quaternion.identity;
                // photocardObj.transform.localPosition = Vector3.zero;

                // howManySprites++;
                // var globalData = PhotoStorageManager.LoadGlobalData();
                // globalData.photoCount = howManySprites;
                // PhotoStorageManager.SaveGlobalData(globalData);

                // SaveSpriteToDisk(screenShotSprite, fileName); // Sprite kaydetme metodu da burada çaðrýlmamalý, galeriye gönderiyoruz.


                // Çift çaðrýlarý ve hatalý olabilecek asset kodunu temizleyelim:
                if (uiManager != null)
                {
                    uiManager.SetSquare();
                    // uiManager.CanPhotos(); // CanPhotos'u Start'ta veya PhotoGalleryManager'da zaten çaðýrýyorsunuz
                }
                else
                {
                    Debug.LogWarning("[PhotoController] UIManager NULL, SetSquare çaðrýlamadý.");
                }

                SortPhotos(); // Kendi SortPhotos'unuzu çaðýrýn
                ShufflePositions(); // Kendi ShufflePositions'unuzu çaðýrýn


                if (inputController != null)
                {
                    inputController.forbiddenInput = true;
                }
                else
                {
                    Debug.LogWarning("[PhotoController] InputController NULL, forbiddenInput ayarlanamadý.");
                }

                // Bu Debug.LogError mesajý asset'ten geliyor. Eðer kendi obje tanýma sisteminiz varsa bunu silebilirsiniz.
                // Debug.LogError("Screenshot saved to collection. But object recognition is not activated. Consider using the extended version: https://u3d.as/3qTN");

                // audioManager ve ObjectRecognition çaðrýlarý:
                // Bu çaðrýlar artýk try bloðunun içinde tek seferlik yapýldýðý için aþaðýdaki tekrar eden satýrlarý SÝLÝYORUM.
                // Hatanýn geldiði 360. satýr burasýydý!
                // audioManager.PlayRandomSound(audioManager.shotSounds); // SÝLÝNDÝ - yukarýda if bloðu içinde zaten var
                // FindObjectOfType<ObjectRecognition>().DetectPhotoObjects(); // SÝLÝNDÝ - yukarýda if bloðu içinde zaten var
            }
            finally
            {
                if (targetTexture != null)
                {
                    Debug.Log($"[PhotoController] RenderTexture.ReleaseTemporary çaðrýlýyor...");
                    RenderTexture.ReleaseTemporary(targetTexture);
                }
                else
                {
                    Debug.LogWarning("[PhotoController] targetTexture null olduðu için ReleaseTemporary çaðrýlmadý.");
                }
            }
        }

        // Stores sprite to disk (Eski asset'in kaydetme metodu. Eðer sadece kendi PhotoStorageManager'ýnýzý kullanýyorsanýz
        // bu metodu ve çaðrýlarýný da silebilirsiniz. Þimdilik býrakýyorum ama çaðrýlmadýðýndan emin olun.)
        public void SaveSpriteToDisk(Sprite sprite, string fileName)
        {
            if (sprite == null)
            {
                Debug.LogError("[PhotoController] SaveSpriteToDisk: Kaydedilecek Sprite NULL!");
                return;
            }
            if (sprite.texture == null)
            {
                Debug.LogError("[PhotoController] SaveSpriteToDisk: Sprite'ýn Texture'ý NULL!");
                return;
            }

            Texture2D texture = sprite.texture;
            byte[] bytes = texture.EncodeToPNG();
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(filePath, bytes);
            // LoadSpriteFromFile(filePath, photocardObj); // photocardObj burada null olabilir, dikkat!
            // Eðer bu kod asset'in eski galeri sistemi içinse, bizim PhotoGalleryManager'ýmýz var.
            // Bu çaðrýyý SÝLMELÝSÝNÝZ.
        }

        // Loads sprite from file to photo card (Eski asset'in yükleme metodu. Eðer sadece kendi PhotoGalleryManager'ýnýzý kullanýyorsanýz
        // bu metodu ve çaðrýlarýný da silebilirsiniz.)
        public void LoadSpriteFromFile(string filePath, GameObject card)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[PhotoController] LoadSpriteFromFile: Dosya bulunamadý: {filePath}");
                return;
            }
            if (card == null)
            {
                Debug.LogError("[PhotoController] LoadSpriteFromFile: Hedef card NULL!");
                return;
            }
            if (card.GetComponent<Image>() == null)
            {
                Debug.LogError("[PhotoController] LoadSpriteFromFile: Hedef card'da Image bileþeni yok!");
                return;
            }

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, (int)fileStream.Length);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                card.GetComponent<Image>().sprite = sprite; // photocardObj yerine card kullanýldý
            }
        }
    }
}