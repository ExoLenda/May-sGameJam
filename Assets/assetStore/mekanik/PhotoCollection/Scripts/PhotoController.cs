using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro; // TMPro kullan�lm�yorsa silinebilir
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
        public CursorCam pointerOnScreen; // E�er kullan�lm�yorsa kald�r�labilir
        #endregion

        #region Photo Settings
        // Total number of stored sprites
        [HideInInspector] public int howManySprites;
        // Temporary photocard reference
        private GameObject photocard;
        // Photo prefab template
        [SerializeField] private GameObject photo; // Photo prefab template
        // Current screenshot sprite (Bu de�i�kene art�k ihtiyac�n�z yok, ��nk� do�rudan galeriye Texture2D g�nderiyoruz)
        // private Sprite screenShotSprite;
        // Full screenshot texture (Bu de�i�kene art�k ihtiyac�n�z yok)
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
        private int resWidth = 906; // Bu de�erler inputController.currentCamera'dan al�nacak
        // Vertical resolution of the screenshots captured in pixels.
        private int resHeight = 419; // Bu de�erler inputController.currentCamera'dan al�nacak

        public float sizeMini = 0.5f; // Bu de�i�kenler ShufflePositions metodunda kullan�l�yor
        public float sizeNormal = 0.7f;
        public float sizeMax = 1f;

        // Position dot return state (Kullan�lm�yorsa silinebilir)
        private bool returnDot;
        #endregion

        #region Photo Management
        // Currently moving photo card (Kullan�lm�yorsa silinebilir)
        [HideInInspector] public PhotoPrefab movingCard;
        // Stored card positions (Kullan�lm�yorsa silinebilir)
        private List<Vector3> positionsCards;
        // All photo card instances
        public List<PhotoPrefab> allCards;
        // Card sequence order
        private List<int> subsequenceCards;
        // Board X position (Kullan�lm�yorsa silinebilir)
        private int boardX;
        #endregion

        #region State Variables
        // Initialization state
        private bool initialReady;
        // State adjustment value (Kullan�lm�yorsa silinebilir)
        private float value;
        #endregion

        #region Constants
        // Photo object reference (Kullan�lm�yorsa silinebilir)
        private GameObject photocardObj;
        // Current mouse position (Sadece MakeScrenshot metodunda parametre olarak kullan�l�yor, burada tan�mlamaya gerek yok)
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
                Debug.LogError("[PhotoController] InputController Start metodunda NULL! Atand���ndan emin olun.");
            }

            if (uiManager != null)
            {
                uiManager.photoController = this;
            }
            else
            {
                Debug.LogError("[PhotoController] UIManager Start metodunda NULL! Atand���ndan emin olun.");
            }

            if (!initialReady)
            {
                initialReady = true;
                List<GameObject> toTurnOff = new List<GameObject>();

                subsequenceCards = LoadSequence();

                // EXTENSION POINT: Modify photo loading logic here
                for (int i = 0; i < subsequenceCards.Count; i++)
                {
                    // Photo prefab'� ve Sprite atamas� art�k PhotoGalleryManager taraf�ndan yap�l�yor
                    // Bu k�s�mdaki mant�k, eski asset'in kendi galeri sistemini y�kl�yor gibi g�r�n�yor.
                    // E�er sadece PhotoGalleryManager'� kullanacaksan�z, bu for d�ng�s�n�n i�levini g�zden ge�irmelisiniz.
                    // �imdilik hata vermemesi i�in b�rak�yorum.
                    photocard = Instantiate(photo, transform.position, transform.rotation, uiManager.photoHolder.transform);
                    photocard.transform.rotation = new Quaternion(0, 0, 0, 0);
                    photocard.transform.localPosition = new Vector3(0, 0, 0);
                    Sprite loadedSprite = LoadSpriteFromDisk("photo" + subsequenceCards[i]);

                    // Null kontrol�
                    if (photocard.GetComponent<Image>() != null)
                    {
                        photocard.GetComponent<Image>().sprite = loadedSprite;
                    }
                    else
                    {
                        Debug.LogWarning($"[PhotoController] Photo prefab'�nda Image bile�eni bulunamad�: {photocard.name}");
                    }

                    PhotoPrefab script = photocard.GetComponent<PhotoPrefab>();
                    if (script == null)
                    {
                        Debug.LogError($"[PhotoController] Photo prefab'�nda PhotoPrefab script'i bulunamad�: {photocard.name}");
                        Destroy(photocard); // Script yoksa objeyi yok et
                        continue; // D�ng�n�n devam�n� atla
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
            if (uiManager != null) // Null kontrol� ekle
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
                // Debug.LogWarning($"[PhotoController] Dosya bulunamad�: {filePath}"); // Her resim i�in uyar� istemiyorsan�z kapat�n
                return null;
            }

            if (new FileInfo(filePath).Length > maxSizeInMB * 1024 * 1024)
            {
                Debug.LogError($"[PhotoController] Dosya {fileName} {maxSizeInMB}MB limitini a��yor.");
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
            if (uiManager != null) // Null kontrol�
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
                Debug.LogWarning($"[PhotoController] Silinecek dosya bulunamad�: {filePath}"); // Eklendi
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
                if (allCards[i] == null) // NULL kontrol� eklendi
                {
                    Debug.LogWarning($"[PhotoController] allCards listesinde NULL bir eleman bulundu, atlan�yor.");
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
            if (uiManager == null || uiManager.photoHolder == null) // Null kontrol�
            {
                Debug.LogError("[PhotoController] ShufflePositions: UIManager veya PhotoHolder NULL!");
                return;
            }

            for (int i = 0; i < allCards.Count; i++)
            {
                if (allCards[i] == null) // NULL kontrol� eklendi
                {
                    Debug.LogWarning($"[PhotoController] allCards listesinde NULL bir PhotoPrefab bulundu index: {i}, atlan�yor.");
                    continue;
                }
                if (allCards[i].gameObject == null) // NULL kontrol� eklendi
                {
                    Debug.LogWarning($"[PhotoController] allCards[{i}] i�indeki gameObject NULL, atlan�yor.");
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
            if (movingCard != null || (uiManager != null && uiManager.cantPhoto)) // uiManager null kontrol� eklendi
                return;

            if (inputController == null)
            {
                Debug.LogError("[PhotoController] InputController referans� NULL! L�tfen Inspector'dan atay�n.");
                return;
            }
            if (inputController.currentCamera == null)
            {
                Debug.LogError("[PhotoController] InputController.currentCamera referans� NULL! Foto�raf �ekilemez.");
                return;
            }

            RenderTexture targetTexture = null;
            try
            {
                // Take screenshot
                resWidth = inputController.currentCamera.pixelWidth;
                resHeight = inputController.currentCamera.pixelHeight;
                Debug.Log($"[PhotoController] Kamera ��z�n�rl���: {resWidth}x{resHeight}");

                Texture2D screenShotTexture = new Texture2D(resWidth, resHeight); // Yerel de�i�ken olarak tan�mland�

                Debug.Log($"[PhotoController] RenderTexture.GetTemporary �a�r�l�yor...");
                targetTexture = RenderTexture.GetTemporary(resWidth, resHeight, depth, renderTextureFormat);
                Debug.Log($"[PhotoController] targetTexture olu�turuldu: {targetTexture != null}");

                Debug.Log($"[PhotoController] Kameran�n targetTexture'� ayarlan�yor...");
                inputController.currentCamera.targetTexture = targetTexture;

                Debug.Log($"[PhotoController] Kameran�n Render metodu �a�r�l�yor...");
                inputController.currentCamera.Render();
                Debug.Log($"[PhotoController] RenderTexture.active ayarlan�yor...");
                RenderTexture.active = targetTexture;

                screenShotTexture.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                screenShotTexture.Apply();

                inputController.currentCamera.targetTexture = null; // Kameran�n hedef dokusunu temizle

                // Crop screenshot
                int clickX = Mathf.RoundToInt(mousePosition.x);
                int clickY = Mathf.RoundToInt(mousePosition.y);

                // Ekran�n d���na ta�may� �nlemek i�in Clamp
                int startX = Mathf.Clamp(clickX - cropSize / 2, 0, screenShotTexture.width - cropSize);
                int startY = Mathf.Clamp(clickY - cropSize / 2, 0, screenShotTexture.height - cropSize);

                // E�er cropSize kamera ��z�n�rl���nden b�y�kse, hata vermemek i�in Clamp de�erlerini kontrol edin.
                // Veya GetPixels metodunun boyutunu da clamp edin.
                int actualCropWidth = Mathf.Min(cropSize, screenShotTexture.width - startX);
                int actualCropHeight = Mathf.Min(cropSize, screenShotTexture.height - startY);


                Color[] pixels = screenShotTexture.GetPixels(startX, startY, actualCropWidth, actualCropHeight);
                croppedScreenshot = new Texture2D(actualCropWidth, actualCropHeight); // Boyutlar� actualCropWidth, actualCropHeight olarak ayarland�
                croppedScreenshot.SetPixels(pixels);
                croppedScreenshot.Apply();

                if (croppedScreenshot != null && croppedScreenshot.width > 0 && croppedScreenshot.height > 0)
                {
                    Debug.Log($"[PhotoController] Cropped Screenshot Olu�turuldu. Boyut: {croppedScreenshot.width}x{croppedScreenshot.height}");

                    // Ge�ici olarak diske kaydetme (hata ay�klama i�in �ok faydal�d�r!)
                    byte[] bytes = croppedScreenshot.EncodeToPNG();
                    string tempFilePath = Path.Combine(Application.persistentDataPath, "debug_cropped_photo_" + howManySprites + ".png");
                    File.WriteAllBytes(tempFilePath, bytes);
                    Debug.Log($"[PhotoController] K�rp�lm�� foto�raf ge�ici olarak kaydedildi: {tempFilePath}");
                }
                else
                {
                    Debug.LogError("[PhotoController] Cropped Screenshot NULL veya ge�ersiz boyutlarda!");
                    return; // E�er croppedScreenshot ge�ersizse, kalan i�lemleri yapma
                }

                // >>>>>>>>>>> BURADA TEK SEFER �A�RILSIN! <<<<<<<<<<<
                // Asset'in kendi foto�raf y�netimi yerine, sizin PhotoGalleryManager'a iletmeniz i�in bu event yeterli.
                OnPhotoCaptureComplete?.Invoke(croppedScreenshot);


                // A�a��daki k�s�mlar, asset'in kendi galeri y�netimi ve obje tan�ma mant���d�r.
                // E�er kendi PhotoGalleryManager ve PhotoCaptureAndDetection'� kullan�yorsan�z,
                // bu kodlar� tamamen S�LEB�L�RS�N�Z. Aksi takdirde, her �a�r�ya NULL kontrol� eklemelisiniz.

                // if (screenShotSprite == null) screenShotSprite = Sprite.Create(...); // Bu sat�r� kald�rd�k, ��nk� Sprite.Create burada tekrar yap�l�yordu
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

                // SaveSpriteToDisk(screenShotSprite, fileName); // Sprite kaydetme metodu da burada �a�r�lmamal�, galeriye g�nderiyoruz.


                // �ift �a�r�lar� ve hatal� olabilecek asset kodunu temizleyelim:
                if (uiManager != null)
                {
                    uiManager.SetSquare();
                    // uiManager.CanPhotos(); // CanPhotos'u Start'ta veya PhotoGalleryManager'da zaten �a��r�yorsunuz
                }
                else
                {
                    Debug.LogWarning("[PhotoController] UIManager NULL, SetSquare �a�r�lamad�.");
                }

                SortPhotos(); // Kendi SortPhotos'unuzu �a��r�n
                ShufflePositions(); // Kendi ShufflePositions'unuzu �a��r�n


                if (inputController != null)
                {
                    inputController.forbiddenInput = true;
                }
                else
                {
                    Debug.LogWarning("[PhotoController] InputController NULL, forbiddenInput ayarlanamad�.");
                }

                // Bu Debug.LogError mesaj� asset'ten geliyor. E�er kendi obje tan�ma sisteminiz varsa bunu silebilirsiniz.
                // Debug.LogError("Screenshot saved to collection. But object recognition is not activated. Consider using the extended version: https://u3d.as/3qTN");

                // audioManager ve ObjectRecognition �a�r�lar�:
                // Bu �a�r�lar art�k try blo�unun i�inde tek seferlik yap�ld��� i�in a�a��daki tekrar eden sat�rlar� S�L�YORUM.
                // Hatan�n geldi�i 360. sat�r buras�yd�!
                // audioManager.PlayRandomSound(audioManager.shotSounds); // S�L�ND� - yukar�da if blo�u i�inde zaten var
                // FindObjectOfType<ObjectRecognition>().DetectPhotoObjects(); // S�L�ND� - yukar�da if blo�u i�inde zaten var
            }
            finally
            {
                if (targetTexture != null)
                {
                    Debug.Log($"[PhotoController] RenderTexture.ReleaseTemporary �a�r�l�yor...");
                    RenderTexture.ReleaseTemporary(targetTexture);
                }
                else
                {
                    Debug.LogWarning("[PhotoController] targetTexture null oldu�u i�in ReleaseTemporary �a�r�lmad�.");
                }
            }
        }

        // Stores sprite to disk (Eski asset'in kaydetme metodu. E�er sadece kendi PhotoStorageManager'�n�z� kullan�yorsan�z
        // bu metodu ve �a�r�lar�n� da silebilirsiniz. �imdilik b�rak�yorum ama �a�r�lmad���ndan emin olun.)
        public void SaveSpriteToDisk(Sprite sprite, string fileName)
        {
            if (sprite == null)
            {
                Debug.LogError("[PhotoController] SaveSpriteToDisk: Kaydedilecek Sprite NULL!");
                return;
            }
            if (sprite.texture == null)
            {
                Debug.LogError("[PhotoController] SaveSpriteToDisk: Sprite'�n Texture'� NULL!");
                return;
            }

            Texture2D texture = sprite.texture;
            byte[] bytes = texture.EncodeToPNG();
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(filePath, bytes);
            // LoadSpriteFromFile(filePath, photocardObj); // photocardObj burada null olabilir, dikkat!
            // E�er bu kod asset'in eski galeri sistemi i�inse, bizim PhotoGalleryManager'�m�z var.
            // Bu �a�r�y� S�LMEL�S�N�Z.
        }

        // Loads sprite from file to photo card (Eski asset'in y�kleme metodu. E�er sadece kendi PhotoGalleryManager'�n�z� kullan�yorsan�z
        // bu metodu ve �a�r�lar�n� da silebilirsiniz.)
        public void LoadSpriteFromFile(string filePath, GameObject card)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[PhotoController] LoadSpriteFromFile: Dosya bulunamad�: {filePath}");
                return;
            }
            if (card == null)
            {
                Debug.LogError("[PhotoController] LoadSpriteFromFile: Hedef card NULL!");
                return;
            }
            if (card.GetComponent<Image>() == null)
            {
                Debug.LogError("[PhotoController] LoadSpriteFromFile: Hedef card'da Image bile�eni yok!");
                return;
            }

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, (int)fileStream.Length);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                card.GetComponent<Image>().sprite = sprite; // photocardObj yerine card kullan�ld�
            }
        }
    }
}