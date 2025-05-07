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
    private Vector2 lastTopPosition;
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
                CheckKinematicStatus();
            }
        }
    }
    private void AdjustCubeTowardsCopy()
    {
        
        Transform copyCube = transform.Find("CopyCube");
        if (copyCube == null)
        {
            Debug.Log("Copy Cube bulunamadý!");
            return;
        }

        
        Vector3 copyPosition = copyCube.position;

        
        Destroy(copyCube.gameObject);

        
        Vector3 directionToCopy = (copyPosition - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, transform.position + directionToCopy, 1.0f);

       
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y + 1.0f, transform.localScale.z);

        
        Debug.Log($"Orijinal Cube yeni pozisyonu: {transform.position}, yeni scale: {transform.localScale}");
    }
    private void AdjustTopColliders()
    {
        if (!GetComponent<Rigidbody>().isKinematic)
        {
            GameObject[] topObjects = GameObject.FindGameObjectsWithTag("top");
            foreach (GameObject top in topObjects)
            {
                BoxCollider collider = top.GetComponent<BoxCollider>();
                if (collider != null)
                {
                  
                    collider.size = new Vector3(
                        Mathf.Max(0, (float)0.12, collider.size.x * 0.5f), 
                        Mathf.Max(0, collider.size.y * 1f), 
                        Mathf.Max(0, (float)0.12, collider.size.z * 0.5f) 
                    );

                   

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
            if (top.activeSelf) 
            {
                Vector2 position = new Vector2(Mathf.Round(top.transform.position.x), Mathf.Round(top.transform.position.z));
                newTopPositions.Add(position);

                
                HandleFallingCube(position, top);
            }
        }

       
        foreach (var lastPosition in topPositions)
        {
            if (!newTopPositions.Contains(lastPosition))
            {
                
                HandleFallingCube(lastPosition, null);
            }
        }

        
        topPositions = newTopPositions;
    }

    private void HandleFallingCube(Vector2 topPosition, GameObject topObject)
    {
        
        GameObject cube = GetCubeAtPosition(topPosition);

        if (cube != null)
        {
            
            if (topObject == null || !topObject.activeSelf)
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
        
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
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
            moveAudioSource.Play(); 
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
        DisableButtons();
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
                
                ColorBlock colorBlock = button.colors;

                
                colorBlock.disabledColor = colorBlock.normalColor;
                button.colors = colorBlock;

                
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
        if (isRestarting) return;
        if (PlayerPrefs.GetInt("GlobalRestartLock", 0) == 1) return;
        isRestarting = true;
        PlayerPrefs.SetInt("GlobalRestartLock", 1);
        
        if (moveAudioSource != null && moveAudioSource.isPlaying)
        {
            moveAudioSource.Stop();
        }
        StopAllCoroutines();
        int restartCount = PlayerPrefs.GetInt("RestartCount", 0);
        restartCount++;
        int randomThreshold = UnityEngine.Random.Range(4, 6);
        if (restartCount >= randomThreshold)
        {
            PlayerPrefs.SetInt("RestartCount", 0);
            RequestAndShowInterstitialAd();
        }
        else
        {
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

        InterstitialAd.Load(adUnitId, new AdRequest(), (InterstitialAd loadedAd, LoadAdError error) =>
        {
            if (error != null)
            {
                Debug.Log("Interstitial ad failed to load: " + error);
                PlayerPrefs.SetInt("IsRestart", 1);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
            currentInterstitialAd = loadedAd;
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
            Invoke("ShowInterstitialAd", 0f);
        });
    }
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
        topObjects.Sort((a, b) =>
        {
            float distanceA = Vector3.Distance(cubePosition, a.transform.position);
            float distanceB = Vector3.Distance(cubePosition, b.transform.position);
            return distanceA.CompareTo(distanceB);
        });
        for (int i = 0; i < topObjects.Count; i++)
        {
            BoxCollider collider = topObjects[i].GetComponent<BoxCollider>();
            if (i < 1)
            {
                collider.enabled = true;
            }
            else
            {
                collider.enabled = false;
            }
        }
    }
}
