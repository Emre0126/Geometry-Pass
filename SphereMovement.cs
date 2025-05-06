using UnityEngine;
using UnityEngine.SceneManagement; // Sahneyi yeniden ba�latmak i�in gerekli
using GoogleMobileAds.Api;
public class SphereMovement : MonoBehaviour
{
    public float moveSpeed = 2.0f;      // Hareket h�z�
    public float rotateSpeed = 200.0f;  // D�nme h�z�
    public float horizontalYPosition = -28.65f;   // Yatay konum i�in y pozisyonu
    public float verticalYPosition = -28.257f;    // Dikey konum i�in y pozisyonu
    public float adjustSpeed = 0.5f;    // Y pozisyonu nihaiye yava��a ula�ma h�z�

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float targetYPosition;      // Y pozisyonunun hedef de�eri
    private bool isMoving = false;
    public AudioSource moveAudioSource;
    private Rigidbody rb; // Rigidbody referans�
    private bool isKinematicDisabled = false; // isKinematic'in kapand���n� takip etmek i�in
    private float restartTimer = 0f; // Zamanlay�c�
    private bool canMove = true;
    private bool isRestarting = false;
    private InterstitialAd currentInterstitialAd;
    private void Start()
    {
        PlayerPrefs.SetInt("GlobalRestartLock", 0);
        rb = GetComponent<Rigidbody>(); // Rigidbody referans�n� al
        if (rb == null)
        {
            Debug.LogError("Rigidbody bile�eni bulunamad�!");
        }
        moveAudioSource.Stop();
    }

    private void Update()
    {
        if (!rb.isKinematic)
        {
            transform.position += Vector3.down * Time.deltaTime * 10f; // H�zl� d����
        }
        if (isMoving)
        {
            // X ve Z pozisyonlar�n� g�ncelle
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(targetPosition.x, transform.position.y, targetPosition.z),
                moveSpeed * Time.deltaTime
            );

            // Y pozisyonunu hedef de�ere do�ru yava��a g�ncelle
            transform.position = new Vector3(
                transform.position.x,
                Mathf.MoveTowards(transform.position.y, targetYPosition, adjustSpeed * Time.deltaTime),
                transform.position.z
            );

            // D�nme hareketini ger�ekle�tir
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            // Hareket ve d�n�� tamamland���nda durdur
            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z)) < 0.01f
                && Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                isMoving = false;
                transform.position = targetPosition; // Tam olarak hedef pozisyona ayarla
                transform.rotation = targetRotation;
                SetFinalYPosition();
            }
        }

        // isKinematic'in kapand���n� kontrol et
        if (!rb.isKinematic && !isKinematicDisabled)
        {
            isKinematicDisabled = true;
            restartTimer = 2f; // Zamanlay�c�y� ba�lat
        }

        // Zamanlay�c� �al���yorsa g�ncelle
        if (isKinematicDisabled && restartTimer > 0)
        {
            restartTimer -= Time.deltaTime;
            if (restartTimer <= 0)
            {
                RestartScene(); // Sahneyi yeniden ba�lat
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
            moveAudioSource.Play(); // Ses dosyas�n� �al
        }
        else
        {
            Debug.LogWarning("AudioSource atanmad�!");
        }
    }

    private void StartMovement(Vector3 direction, Vector3 rotationAxis)
    {

        FindObjectOfType<RestoreManager>()?.UpdatePreviousStates();
        FindObjectOfType<RestoreManager>()?.SaveAllShapeStates();


        isMoving = true;
        targetPosition = transform.position + direction.normalized; // Y�n� 1 birim ilerlet
        targetRotation = Quaternion.AngleAxis(90, rotationAxis) * transform.rotation;

        // Ge�erli pozisyon ve hedef pozisyon durumuna g�re Y pozisyonunu ayarla
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
            targetYPosition = transform.position.y; // Y�n de�i�miyorsa mevcut Y de�eri korunur
        }

    }


    private void SetFinalYPosition()
    {
        // Hareket tamamland���nda nihai Y pozisyonunu ayarla
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
        // Dikey konum kontrol�
        float xAngle = Mathf.Abs(transform.rotation.eulerAngles.x % 180);
        float zAngle = Mathf.Abs(transform.rotation.eulerAngles.z % 180);
        return Mathf.Approximately(xAngle, 90) || Mathf.Approximately(zAngle, 90);
    }

    private bool IsHorizontalPosition()
    {
        // Yatay konum kontrol�
        float yAngle = Mathf.Abs(transform.rotation.eulerAngles.y % 180);
        return Mathf.Approximately(yAngle, 0) || Mathf.Approximately(yAngle, 180);
    }

    private void RestartScene()
    {
        if (isRestarting) return; // Bu scriptte zaten �al��t�ysa
        if (PlayerPrefs.GetInt("GlobalRestartLock", 0) == 1) return; // Ba�ka biri zaten ba�latt�ysa

        isRestarting = true;
        PlayerPrefs.SetInt("GlobalRestartLock", 1);
        
        int restartCount = PlayerPrefs.GetInt("RestartCount", 0);
        restartCount++;
        int randomThreshold = UnityEngine.Random.Range(4, 6); // 4 ile 5 aras�nda (�st s�n�r dahil de�il)

        if (restartCount >= randomThreshold)
        {
            PlayerPrefs.SetInt("RestartCount", 0); // Sayac� s�f�rla
            RequestAndShowInterstitialAd(); // Reklam g�ster
        }
        else
        {
            // 6'ya ula��lmad�ysa, sayac� kaydet ve normal reset i�lemini yap.
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