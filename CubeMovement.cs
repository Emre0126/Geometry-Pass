using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using GoogleMobileAds.Api;
public class CubeMovement : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float rotateSpeed = 200.0f;
    public float horizontalYPosition = -28.65f;
    public float verticalYPosition = -28.257f;
    public float adjustSpeed = 0.5f;
    public Button[] buttonsToDisable;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float targetYPosition;
    private bool isMoving = false;
    private bool yPositionFrozen = false;
    public AudioSource moveAudioSource;
    private HashSet<Vector2> topPositions = new HashSet<Vector2>();
    private Vector2 lastTopPosition; // Son top tagli nesnenin pozisyonu
    private bool isRestarting = false;
    private bool hasMoveRequest = false;
    private bool canMove = true;
    private float moveCooldown = 0.22f;
    private InterstitialAd currentInterstitialAd;
    private void Start()
    {
        PlayerPrefs.SetInt("GlobalRestartLock", 0);
        RecordTopPositions();
        moveAudioSource.Stop();
    }

    private void Update()
    {
        // 'top' etiketine sahip tüm aktif nesnelerin pozisyonlarýný güncelle
        UpdateTopPositions();

        if (!isMoving && !GetComponent<Rigidbody>().isKinematic)
        {
            DisableButtons();
            AdjustCubeTowardsCopy();
            AdjustTopColliders();

            Invoke("RestartScene", 2f);
            transform.position += Vector3.down * Time.deltaTime * 14f;
        }
        if (!GetComponent<Rigidbody>().isKinematic)
        {
            canMove = false;
            isMoving = false;
        }
        if (isMoving)
        {
            // Hareketin geri kalan kýsmý
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(targetPosition.x, transform.position.y, targetPosition.z),
                moveSpeed * Time.deltaTime
            );

            if (!yPositionFrozen)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    Mathf.MoveTowards(transform.position.y, targetYPosition, adjustSpeed * Time.deltaTime),
                    transform.position.z
                );
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z)) < 0.01f
                && Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                isMoving = false;
                hasMoveRequest = false;
                transform.rotation = targetRotation;
                SetFinalYPosition();
                yPositionFrozen = false;
                CheckKinematicStatus(); // Hareket tamamlandýktan sonra kontrol
            }
        }
    }
    private void AdjustCubeTowardsCopy()
    {
        // Copy Cube'u bul
        Transform copyCube = transform.Find("CopyCube");
        if (copyCube == null)
        {
            Debug.Log("Copy Cube bulunamadý!");
            return;
        }

        // Copy Cube'ü silmeden önce pozisyonunu kaydet
        Vector3 copyPosition = copyCube.position;

        // Copy Cube'ü sil
        Destroy(copyCube.gameObject);

        // Orijinal Cube'ü Copy Cube'e doðru 1 birim yaklaþtýr
        Vector3 directionToCopy = (copyPosition - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, transform.position + directionToCopy, 1.0f);

        // Sadece y ekseninde büyütme
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y + 1.0f, transform.localScale.z);

        // Yeni pozisyonu ve scale'i loglayýn (isteðe baðlý)
        Debug.Log($"Orijinal Cube yeni pozisyonu: {transform.position}, yeni scale: {transform.localScale}");
    }
    private void AdjustTopColliders()
    {
        if (!GetComponent<Rigidbody>().isKinematic) // Eðer isKinematic kapalýysa
        {
            GameObject[] topObjects = GameObject.FindGameObjectsWithTag("top");
            foreach (GameObject top in topObjects)
            {
                BoxCollider collider = top.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    // Collider'ýn boyutlarýný küçült
                    collider.size = new Vector3(
                        Mathf.Max(0, (float)0.12, collider.size.x * 0.5f), // X ekseni boyutunu yarýya indir
                        Mathf.Max(0, collider.size.y * 1f), // Y ekseni boyutunu yarýya indir
                        Mathf.Max(0, (float)0.12, collider.size.z * 0.5f)  // Z ekseni boyutunu yarýya indir
                    );

                    // Collider'ýn merkezini yeniden ayarla (opsiyonel)

                }
            }
        }
    }


    private void UpdateTopPositions()
    {
        HashSet<Vector2> newTopPositions = new HashSet<Vector2>();
        GameObject[] tops = GameObject.FindGameObjectsWithTag("top");

        foreach (GameObject top in tops)
        {
            if (top.activeSelf) // Sadece aktif olanlarý kontrol et
            {
                Vector2 position = new Vector2(Mathf.Round(top.transform.position.x), Mathf.Round(top.transform.position.z));
                newTopPositions.Add(position);

                // Check for any cube above this top that needs to fall
                HandleFallingCube(position, top);
            }
        }

        // Remove any positions that are no longer active
        foreach (var lastPosition in topPositions)
        {
            if (!newTopPositions.Contains(lastPosition))
            {
                // Deactivated top object found, check if any cube above it needs to fall
                HandleFallingCube(lastPosition, null);
            }
        }

        // Update the top positions list
        topPositions = newTopPositions;
    }

    private void HandleFallingCube(Vector2 topPosition, GameObject topObject)
    {
        // Check if there is a cube above the top object at the same position
        GameObject cube = GetCubeAtPosition(topPosition);

        if (cube != null)
        {
            // If the top object is deactivated, set the cube's Rigidbody to non-kinematic to make it fall
            if (topObject == null || !topObject.activeSelf) // If top is deactivated
            {
                Rigidbody rb = cube.GetComponent<Rigidbody>();
                if (rb != null && rb.isKinematic)
                {
                    rb.isKinematic = false;
                    Debug.Log("Cube above deactivated top is now falling.");
                }
            }
            else
            {
                // Ensure the cube is kinematic again when top object is active
                Rigidbody rb = cube.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    rb.isKinematic = false;
                    Debug.Log("Cube above active top is now kinematic.");
                }
            }
        }
    }

    private GameObject GetCubeAtPosition(Vector2 position)
    {
        // Find and return the cube located at the given position
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube"); // Or the appropriate tag for your cubes
        foreach (var cube in cubes)
        {
            Vector2 cubePosition = new Vector2(Mathf.Round(cube.transform.position.x), Mathf.Round(cube.transform.position.z));
            if (cubePosition == position)
            {
                return cube;
            }
        }
        return null;
    }


    public void MoveUp()
    {
        if (!canMove || !gameObject.activeSelf || isMoving) return;
        {
            StartMovement(Vector3.forward, Vector3.right);
            PlayMoveAudio();
        }
    }

    public void MoveDown()
    {
        if (!canMove || !gameObject.activeSelf || isMoving) return;
        {
            StartMovement(Vector3.back, Vector3.left);
            PlayMoveAudio();
        }
    }

    public void MoveLeft()
    {
        if (!canMove || !gameObject.activeSelf || isMoving) return;
        {
            StartMovement(Vector3.left, Vector3.forward);
            PlayMoveAudio();
        }
    }

    public void MoveRight()
    {
        if (!canMove || !gameObject.activeSelf || isMoving) return;
        {
            StartMovement(Vector3.right, Vector3.back);
            PlayMoveAudio();
        }
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


        if (!canMove || isMoving) return;

        canMove = false;
        isMoving = true;
        DisableButtons(); // Butonlarý pasif yap
        Invoke(nameof(ResetMoveLock), moveCooldown);


        bool currentIsVertical = IsVerticalEulerAngles(transform.rotation.eulerAngles);
        targetRotation = Quaternion.AngleAxis(90, rotationAxis) * transform.rotation;
        bool targetIsVertical = IsVerticalEulerAngles(targetRotation.eulerAngles);


        if (currentIsVertical && !targetIsVertical)
        {
            targetPosition = transform.position + direction.normalized * 1.5f;
            targetYPosition = horizontalYPosition;
            yPositionFrozen = false;
        }
        else if (!currentIsVertical && targetIsVertical)
        {
            targetPosition = transform.position + direction.normalized * 1.5f;
            targetYPosition = verticalYPosition;
            yPositionFrozen = false;
        }
        else
        {
            targetPosition = transform.position + direction.normalized * 1.0f;
            targetYPosition = transform.position.y;
            yPositionFrozen = true;
        }

        // Hareket baþlamadan önce son top pozisyonunu kaydet
        lastTopPosition = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.z));
    }
    public bool CanMove
    {
        get => canMove;
        set => canMove = value;
    }

    public void ResetMoveLock()
    {
        canMove = true;
        EnableButtons();
    }

    private void SetFinalYPosition()
    {
        float finalY = IsVerticalPosition() ? verticalYPosition : horizontalYPosition;
        Vector3 correctedPos = new Vector3(
            Mathf.Round(transform.position.x * 1000000f) / 1000000f,
            finalY,
            Mathf.Round(transform.position.z * 1000000f) / 1000000f
        );
        transform.position = correctedPos;

        transform.rotation = Quaternion.Euler(
            Mathf.Round(transform.rotation.eulerAngles.x / 1f) * 1f,
            Mathf.Round(transform.rotation.eulerAngles.y / 1f) * 1f,
            Mathf.Round(transform.rotation.eulerAngles.z / 1f) * 1f
        );
    }


    private bool IsVerticalPosition()
    {
        float xAngle = Mathf.Abs(transform.rotation.eulerAngles.x % 180);
        float zAngle = Mathf.Abs(transform.rotation.eulerAngles.z % 180);
        return Mathf.Approximately(xAngle, 90) || Mathf.Approximately(zAngle, 90);
    }

    private void RecordTopPositions()
    {
        GameObject[] topObjects = GameObject.FindGameObjectsWithTag("top");
        foreach (GameObject top in topObjects)
        {
            Vector2 position = new Vector2(Mathf.Round(top.transform.position.x), Mathf.Round(top.transform.position.z));
            topPositions.Add(position);
        }
    }

    private void CheckKinematicStatus()
    {
        Vector2 cubePosition = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.z));
        bool isHalf = Mathf.Abs(transform.localPosition.x % 1) == 0.5f || Mathf.Abs(transform.localPosition.z % 1) == 0.5f;

        if (isHalf)
        {
            IsHorizontal(transform);
        }
        else
        {
            if (!topPositions.Contains(cubePosition))
            {
                GetComponent<Rigidbody>().isKinematic = false;
                DisableButtons();
                isMoving = false;
                DisableOtherColliders();

            }
            else
            {
                GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }

    public void DisableButtons()
    {
        foreach (Button button in buttonsToDisable)
        {
            if (button != null)
            {
                // Button'un renk bloklarýný al
                ColorBlock colorBlock = button.colors;

                // Button'un normal ve disabled renklerini ayný yap
                colorBlock.disabledColor = colorBlock.normalColor;
                button.colors = colorBlock;

                // Butonu devre dýþý býrak
                button.interactable = false;
            }
        }
    }
    private void EnableButtons()
    {
        foreach (Button button in buttonsToDisable)
        {
            if (button != null)
                button.interactable = true;
        }
    }

    private void RestartScene()
    {
        if (isRestarting) return; // Eðer zaten resetleme baþladýysa, ikinci kez çalýþtýrma
        if (PlayerPrefs.GetInt("GlobalRestartLock", 0) == 1) return; // Baþka biri baþlattýysa çýk
        isRestarting = true; // Bu objede reset baþladý
        PlayerPrefs.SetInt("GlobalRestartLock", 1);
        // Olasý sesleri durdur
        if (moveAudioSource != null && moveAudioSource.isPlaying)
        {
            moveAudioSource.Stop();
        }

        // Tüm hareketleri ve animasyonlarý sýfýrla
        StopAllCoroutines();

        // Zamaný sýfýrla ve sahneyi yükle
        
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
    private void IsHorizontal(Transform targetCube)
    {
        //float axisValue = (Mathf.Abs(targetCube.localPosition.x % 1) == 0.5f) ? targetCube.position.x : targetCube.position.z;
        //float adjustedValuePos = axisValue + 0.5f;
        //float adjustedValueNeg = axisValue - 0.5f;

        //Vector2 posMatch = new Vector2(Mathf.Round(adjustedValuePos), Mathf.Round(targetCube.position.z));
        //Vector2 negMatch = new Vector2(Mathf.Round(adjustedValueNeg), Mathf.Round(targetCube.position.z));

        //int matchCount = (topPositions.Contains(posMatch) ? 1 : 0) + (topPositions.Contains(negMatch) ? 1 : 0);

        float xValue = Mathf.Abs(targetCube.localPosition.x % 1);
        float zValue = Mathf.Abs(targetCube.localPosition.z % 1);

        int matchCount = 3;

        if (xValue == 0.5f)
        {
            float xPlus = Mathf.Abs(targetCube.position.x) + 0.5f;
            float xMinus = Mathf.Abs(targetCube.position.x) - 0.5f;

            Vector2 plusMatch = new Vector2(Mathf.Round(xPlus), Mathf.Round(targetCube.position.z));
            Vector2 minusMatch = new Vector2(Mathf.Round(xMinus), Mathf.Round(targetCube.position.z));

            //int num1 = topPositions.Contains(plusMatch) ? 1 : 0;
            //int num2 = topPositions.Contains(minusMatch) ? 1 : 0;
            int num1 = CheckContaining(topPositions, plusMatch) ? 1 : 0;
            int num2 = CheckContaining(topPositions, minusMatch) ? 1 : 0;

            matchCount = num1 + num2;
        }
        else if (zValue == 0.5f)
        {
            float zPlus = Mathf.Abs(targetCube.position.z) + 0.5f;
            float zMinus = Mathf.Abs(targetCube.position.z) - 0.5f;

            Vector2 plusMatch = new Vector2(Mathf.Round(targetCube.position.x), Mathf.Round(zPlus));
            Vector2 minusMatch = new Vector2(Mathf.Round(targetCube.position.x), Mathf.Round(zMinus));

            //int num1 = topPositions.Contains(plusMatch) ? 1 : 0;
            //int num2 = topPositions.Contains(minusMatch) ? 1 : 0;
            int num1 = CheckContaining(topPositions, plusMatch) ? 1 : 0;
            int num2 = CheckContaining(topPositions, minusMatch) ? 1 : 0;

            matchCount = num1 + num2;
        }

        if (matchCount == 3)
        {
            throw new Exception("Match count reached 3, which is not allowed.");
        }


        if (matchCount == 2)
        {
            targetCube.GetComponent<Rigidbody>().isKinematic = true;
        }
        else
        {
            //Vector3 directionToMove = (topPositions.Contains(posMatch) ? posMatch : negMatch) - new Vector2(targetCube.position.x, targetCube.position.z);
            //targetCube.position = Vector3.MoveTowards(targetCube.position, targetCube.position + new Vector3(directionToMove.x, 0, directionToMove.y), 0.23f);

            DisableOtherColliders();
            DisableButtons();
            targetCube.GetComponent<Rigidbody>().isKinematic = false;
        }
    }
    private bool IsVerticalEulerAngles(Vector3 euler)
    {
        float x = Mathf.Round(euler.x % 180f);
        float z = Mathf.Round(euler.z % 180f);
        return Mathf.Approximately(x, 90f) || Mathf.Approximately(z, 90f);
    }

    private bool CheckContaining(HashSet<Vector2> list, Vector2 vector)
    {
        foreach (var vec in list)
        {
            if (vec.x == vector.x && vec.y == vector.y)
            {
                return true;
            }
        }

        return false;
    }

    private void DisableOtherColliders()
    {
        List<GameObject> topObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("top"));
        Vector3 cubePosition = transform.position;

        // Tüm top nesnelerini mesafelerine göre sýralayalým
        topObjects.Sort((a, b) =>
        {
            float distanceA = Vector3.Distance(cubePosition, a.transform.position);
            float distanceB = Vector3.Distance(cubePosition, b.transform.position);
            return distanceA.CompareTo(distanceB);
        });

        // Ýlk iki en yakýn nesne hariç diðerlerinin collider'larýný kapat
        for (int i = 0; i < topObjects.Count; i++)
        {
            BoxCollider collider = topObjects[i].GetComponent<BoxCollider>();
            if (i < 1)
            {
                collider.enabled = true; // En yakýn iki nesnenin collider'ýný açýk býrak
            }
            else
            {
                collider.enabled = false; // Diðerlerinin collider'larýný kapat
            }
        }
    }
}