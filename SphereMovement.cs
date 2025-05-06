using UnityEngine;
using UnityEngine.SceneManagement; // Sahneyi yeniden baþlatmak için gerekli
using GoogleMobileAds.Api;
public class SphereMovement : MonoBehaviour
{
    public float moveSpeed = 2.0f;      // Hareket hýzý
    public float rotateSpeed = 200.0f;  // Dönme hýzý
    public float horizontalYPosition = -28.65f;   // Yatay konum için y pozisyonu
    public float verticalYPosition = -28.257f;    // Dikey konum için y pozisyonu
    public float adjustSpeed = 0.5f;    // Y pozisyonu nihaiye yavaþça ulaþma hýzý

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float targetYPosition;      // Y pozisyonunun hedef deðeri
    private bool isMoving = false;
    public AudioSource moveAudioSource;
    private Rigidbody rb; // Rigidbody referansý
    private bool isKinematicDisabled = false; // isKinematic'in kapandýðýný takip etmek için
    private float restartTimer = 0f; // Zamanlayýcý
    private bool canMove = true;
    private bool isRestarting = false;
    private InterstitialAd currentInterstitialAd;
    private void Start()
    {
        PlayerPrefs.SetInt("GlobalRestartLock", 0);
        rb = GetComponent<Rigidbody>(); // Rigidbody referansýný al
        if (rb == null)
        {
            Debug.LogError("Rigidbody bileþeni bulunamadý!");
        }
        moveAudioSource.Stop();
    }

    private void Update()
    {
        if (!rb.isKinematic)
        {
            transform.position += Vector3.down * Time.deltaTime * 10f; // Hýzlý düþüþ
        }
        if (isMoving)
        {
            // X ve Z pozisyonlarýný güncelle
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(targetPosition.x, transform.position.y, targetPosition.z),
                moveSpeed * Time.deltaTime
            );

            // Y pozisyonunu hedef deðere doðru yavaþça güncelle
            transform.position = new Vector3(
                transform.position.x,
                Mathf.MoveTowards(transform.position.y, targetYPosition, adjustSpeed * Time.deltaTime),
                transform.position.z
            );

            // Dönme hareketini gerçekleþtir
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            // Hareket ve dönüþ tamamlandýðýnda durdur
            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z)) < 0.01f
                && Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                isMoving = false;
                transform.position = targetPosition; // Tam olarak hedef pozisyona ayarla
                transform.rotation = targetRotation;
                SetFinalYPosition();
            }
        }

        // isKinematic'in kapandýðýný kontrol et
        if (!rb.isKinematic && !isKinematicDisabled)
        {
            isKinematicDisabled = true;
            restartTimer = 2f; // Zamanlayýcýyý baþlat
        }

        // Zamanlayýcý çalýþýyorsa güncelle
        if (isKinematicDisabled && restartTimer > 0)
        {
            restartTimer -= Time.deltaTime;
            if (restartTimer <= 0)
            {
                RestartScene(); // Sahneyi yeniden baþlat
            }
        }
    }

    public void MoveUp()
    {
        if (!canMove || !gameObject.activeSelf || isMoving) return;

        StartMovement(Vector3.forward, Vector3.right);
        PlayMoveAudio();
    }

    public void MoveDown()
    {
        if (!canMove || !gameObject.activeSelf || isMoving) return;

        StartMovement(Vector3.back, Vector3.left);
        PlayMoveAudio();
    }

    public void MoveLeft()
    {
        if (!canMove || !gameObject.activeSelf || isMoving) return;

        StartMovement(Vector3.left, Vector3.forward);
        PlayMoveAudio();
    }

    public void MoveRight()
    {
        if (!canMove || !gameObject.activeSelf || isMoving) return;

        StartMovement(Vector3.right, Vector3.back);
        PlayMoveAudio();
    }
    public bool CanMove
    {
        get => canMove;
        set => canMove = value;
    }
    private void PlayMoveAudio()
    {
        if (moveAudioSource != null)
        {
            moveAudioSource.Play(); // Ses dosyasýný çal
        }
        else
        {
            Debug.LogWarning("AudioSource atanmadý!");
        }
    }

    private void StartMovement(Vector3 direction, Vector3 rotationAxis)
    {

        FindObjectOfType<RestoreManager>()?.UpdatePreviousStates();
        FindObjectOfType<RestoreManager>()?.SaveAllShapeStates();


        isMoving = true;
        targetPosition = transform.position + direction.normalized; // Yönü 1 birim ilerlet
        targetRotation = Quaternion.AngleAxis(90, rotationAxis) * transform.rotation;

        // Geçerli pozisyon ve hedef pozisyon durumuna göre Y pozisyonunu ayarla
        bool currentIsVertical = IsVerticalPosition();
        bool targetIsVertical = Mathf.Approximately(targetRotation.eulerAngles.x % 180, 90) || Mathf.Approximately(targetRotation.eulerAngles.z % 180, 90);

        if (currentIsVertical && !targetIsVertical)
        {
            targetYPosition = horizontalYPosition;
        }
        else if (!currentIsVertical && targetIsVertical)
        {
            targetYPosition = verticalYPosition;
        }
        else
        {
            targetYPosition = transform.position.y; // Yön deðiþmiyorsa mevcut Y deðeri korunur
        }

    }


    private void SetFinalYPosition()
    {
        // Hareket tamamlandýðýnda nihai Y pozisyonunu ayarla
        if (IsVerticalPosition())
        {
            transform.position = new Vector3(transform.position.x, verticalYPosition, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, horizontalYPosition, transform.position.z);
        }
    }

    private bool IsVerticalPosition()
    {
        // Dikey konum kontrolü
        float xAngle = Mathf.Abs(transform.rotation.eulerAngles.x % 180);
        float zAngle = Mathf.Abs(transform.rotation.eulerAngles.z % 180);
        return Mathf.Approximately(xAngle, 90) || Mathf.Approximately(zAngle, 90);
    }

    private bool IsHorizontalPosition()
    {
        // Yatay konum kontrolü
        float yAngle = Mathf.Abs(transform.rotation.eulerAngles.y % 180);
        return Mathf.Approximately(yAngle, 0) || Mathf.Approximately(yAngle, 180);
    }

    private void RestartScene()
    {
        if (isRestarting) return; // Bu scriptte zaten çalýþtýysa
        if (PlayerPrefs.GetInt("GlobalRestartLock", 0) == 1) return; // Baþka biri zaten baþlattýysa

        isRestarting = true;
        PlayerPrefs.SetInt("GlobalRestartLock", 1);
        
        int restartCount = PlayerPrefs.GetInt("RestartCount", 0);
        restartCount++;
        int randomThreshold = UnityEngine.Random.Range(4, 6); // 4 ile 5 arasýnda (üst sýnýr dahil deðil)

        if (restartCount >= randomThreshold)
        {
            PlayerPrefs.SetInt("RestartCount", 0); // Sayacý sýfýrla
            RequestAndShowInterstitialAd(); // Reklam göster
        }
        else
        {
            // 6'ya ulaþýlmadýysa, sayacý kaydet ve normal reset iþlemini yap.
            PlayerPrefs.SetInt("RestartCount", restartCount);
            Time.timeScale = 1f;
            PlayerPrefs.SetInt("IsRestart", 1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    private void RequestAndShowInterstitialAd()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-8869277409030391/6174358238";
#elif UNITY_IPHONE
            string adUnitId = "YOUR_IOS_AD_UNIT_ID";
#else
            string adUnitId = "unexpected_platform";
#endif

        // Use the updated API that does not involve a local builder.
        InterstitialAd.Load(adUnitId, new AdRequest(), (InterstitialAd loadedAd, LoadAdError error) =>
        {
            if (error != null)
            {
                Debug.Log("Interstitial ad failed to load: " + error);
                PlayerPrefs.SetInt("IsRestart", 1);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            // Store the loaded ad in our class-level field.
            currentInterstitialAd = loadedAd;

            // Set up ad event handlers.
            currentInterstitialAd.OnAdFullScreenContentClosed += () =>
            {
                PlayerPrefs.SetInt("IsRestart", 1);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            };
            currentInterstitialAd.OnAdFullScreenContentFailed += (AdError adError) =>
            {
                PlayerPrefs.SetInt("IsRestart", 1);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            };

            // Now wait 1 second and then show the interstitial.
            Invoke("ShowInterstitialAd", 0f);
        });
    }

    // Class-level method that Invoke can locate.
    private void ShowInterstitialAd()
    {
        if (currentInterstitialAd != null)
        {
            currentInterstitialAd.Show();
        }
        else
        {
            PlayerPrefs.SetInt("IsRestart", 1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}