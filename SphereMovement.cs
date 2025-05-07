using UnityEngine;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;
public class SphereMovement : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float rotateSpeed = 200.0f;
    public float horizontalYPosition = -28.65f;
    public float verticalYPosition = -28.257f;
    public float adjustSpeed = 0.5f;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float targetYPosition;
    private bool isMoving = false;
    public AudioSource moveAudioSource;
    private Rigidbody rb;
    private bool isKinematicDisabled = false;
    private float restartTimer = 0f;
    private bool canMove = true;
    private bool isRestarting = false;
    private InterstitialAd currentInterstitialAd;
    private void Start()
    {
        PlayerPrefs.SetInt("GlobalRestartLock", 0);
        rb = GetComponent<Rigidbody>();
        moveAudioSource.Stop();
    }
    private void Update()
    {
        if (!rb.isKinematic)
        {
            transform.position += Vector3.down * Time.deltaTime * 10f;
        }
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(targetPosition.x, transform.position.y, targetPosition.z),
                moveSpeed * Time.deltaTime
            );
            transform.position = new Vector3(
                transform.position.x,
                Mathf.MoveTowards(transform.position.y, targetYPosition, adjustSpeed * Time.deltaTime),
                transform.position.z
            );
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z)) < 0.01f
                && Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                isMoving = false;
                transform.position = targetPosition;
                transform.rotation = targetRotation;
                SetFinalYPosition();
            }
        }
        if (!rb.isKinematic && !isKinematicDisabled)
        {
            isKinematicDisabled = true;
            restartTimer = 2f;
        }
        if (isKinematicDisabled && restartTimer > 0)
        {
            restartTimer -= Time.deltaTime;
            if (restartTimer <= 0)
            {
                RestartScene();
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
            moveAudioSource.Play();
        }
    }
    private void StartMovement(Vector3 direction, Vector3 rotationAxis)
    {
        FindObjectOfType<RestoreManager>()?.UpdatePreviousStates();
        FindObjectOfType<RestoreManager>()?.SaveAllShapeStates();
        isMoving = true;
        targetPosition = transform.position + direction.normalized;
        targetRotation = Quaternion.AngleAxis(90, rotationAxis) * transform.rotation;
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
            targetYPosition = transform.position.y;
        }
    }
    private void SetFinalYPosition()
    {
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
        float xAngle = Mathf.Abs(transform.rotation.eulerAngles.x % 180);
        float zAngle = Mathf.Abs(transform.rotation.eulerAngles.z % 180);
        return Mathf.Approximately(xAngle, 90) || Mathf.Approximately(zAngle, 90);
    }
    private bool IsHorizontalPosition()
    {
        float yAngle = Mathf.Abs(transform.rotation.eulerAngles.y % 180);
        return Mathf.Approximately(yAngle, 0) || Mathf.Approximately(yAngle, 180);
    }
    private void RestartScene()
    {
        if (isRestarting) return;
        if (PlayerPrefs.GetInt("GlobalRestartLock", 0) == 1) return;
        isRestarting = true;
        PlayerPrefs.SetInt("GlobalRestartLock", 1);
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
}
