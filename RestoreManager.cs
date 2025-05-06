using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using TMPro;

public class RestoreManager : MonoBehaviour
{
    public Button restoreButton;
    public GameObject cube, sphere, cylinder;
    public ObjectChanger[] objectChangers;

    private Vector3 cubePrevPos, spherePrevPos, cylinderPrevPos;
    private Quaternion cubePrevRot, spherePrevRot, cylinderPrevRot;

    private Vector3 lockedCubePos, lockedSpherePos, lockedCylinderPos;
    private Quaternion lockedCubeRot, lockedSphereRot, lockedCylinderRot;
    private bool lockedSnapshotCaptured = false;

    private bool wasCubeKinematic = false, cubeSnapshotLocked = false;
    private bool wasSphereKinematic = false, sphereSnapshotLocked = false;
    private bool wasCylinderKinematic = false, cylinderSnapshotLocked = false;
    private bool globalSnapshotLocked = false;
    private string restoreKey;
    private string rewardedAdUnitId = "ca-app-pub-8869277409030391/9744580349"; // kendi ID'ni koy
    private string interstitialAdUnitId = "ca-app-pub-8869277409030391/6174358238"; // kendi ID'ni koy
    [SerializeField] private TextMeshProUGUI popupText;
    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;
    private Vector3 cubeInitialPos, sphereInitialPos, cylinderInitialPos;

    private void Start()
    {
        restoreKey = SceneManager.GetActiveScene().name + "_RestoreAvailable";

        PlayerPrefs.SetInt("RewardedAdLoaded", 0); // baþta reklam yüklenmedi say
        PlayerPrefs.Save();

        MobileAds.Initialize(initStatus =>
        {
            LoadRewardedAd();
            LoadInterstitialAd();
            if (PlayerPrefs.GetInt("IsRestart", 0) == 1 && PlayerPrefs.GetInt(restoreKey, 0) == 1)
            {
                restoreButton?.gameObject.SetActive(true);
            }
            else
            {
                restoreButton?.gameObject.SetActive(false);
            }
        });

        PlayerPrefs.DeleteKey("IsRestart");

        restoreButton?.onClick.RemoveAllListeners();
        restoreButton?.onClick.AddListener(ShowAdForRestore);
        cubeInitialPos = cube.transform.position;
        sphereInitialPos = sphere.transform.position;
        cylinderInitialPos = cylinder.transform.position;
    }


    private void Update()
    {
        if (PlayerPrefs.GetInt(restoreKey, 0) == 1 && restoreButton.gameObject.activeSelf)
        {
            if (PositionChanged(cube, cubeInitialPos) || PositionChanged(sphere, sphereInitialPos) || PositionChanged(cylinder, cylinderInitialPos))
            {
                Debug.Log("Þekiller hareket etti, restore butonu gizlendi.");
                PlayerPrefs.DeleteKey(restoreKey);
                PlayerPrefs.Save();
                restoreButton?.gameObject.SetActive(false);
            }
        }
    }

    public void UpdatePreviousStates()
    {
        Rigidbody cubeRb = cube.GetComponent<Rigidbody>();
        Rigidbody sphereRb = sphere.GetComponent<Rigidbody>();
        Rigidbody cylinderRb = cylinder.GetComponent<Rigidbody>();

        bool cubeKinematic = cubeRb != null && cubeRb.isKinematic;
        bool sphereKinematic = sphereRb != null && sphereRb.isKinematic;
        bool cylinderKinematic = cylinderRb != null && cylinderRb.isKinematic;

        int kinematicTrueCount = 0;
        if (cubeKinematic) kinematicTrueCount++;
        if (sphereKinematic) kinematicTrueCount++;
        if (cylinderKinematic) kinematicTrueCount++;

        if (kinematicTrueCount == 3)
        {
            SaveSnapshot();
        }

        if (kinematicTrueCount < 3 && !lockedSnapshotCaptured)
        {
            lockedSnapshotCaptured = true;
            Debug.Log("Düþmeden önceki snapshot kilitlendi.");
        }
    }

    private void SaveSnapshot()
    {
        cubePrevPos = cube.transform.position;
        cubePrevRot = cube.transform.rotation;

        spherePrevPos = sphere.transform.position;
        spherePrevRot = sphere.transform.rotation;

        cylinderPrevPos = cylinder.transform.position;
        cylinderPrevRot = cylinder.transform.rotation;

        Debug.Log("Snapshot güncellendi.");
    }

    private bool PositionChanged(GameObject obj, Vector3 originalPos)
    {
        return Vector3.Distance(obj.transform.position, originalPos) > 0.01f;
    }

    private void LoadRewardedAd()
    {
        AdRequest request = new AdRequest();
        RewardedAd.Load(rewardedAdUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error == null && ad != null)
            {
                rewardedAd = ad;
                PlayerPrefs.SetInt("RewardedAdLoaded", 1);
                PlayerPrefs.Save();

                rewardedAd.OnAdFullScreenContentClosed += delegate {
                    LoadRewardedAd();
                    PlayerPrefs.SetInt("RewardedAdLoaded", 0);
                    PlayerPrefs.Save();
                };
            }
            else
            {
                Debug.LogWarning("Rewarded yüklenemedi: " + error?.GetMessage());
                PlayerPrefs.SetInt("RewardedAdLoaded", 0);
                PlayerPrefs.Save();
            }
        });
    }
    private bool adAlreadyShown = false;
    public void ShowAdForRestore()
    {
        StartCoroutine(TryShowRewardedAd());
    }

    private IEnumerator TryShowRewardedAd()
    {
        if (adAlreadyShown) yield break;

        adAlreadyShown = true;
        restoreButton.interactable = false; // Butonu devre dýþý býrak
        ShowPopup("Loading ad, please wait..."); // Popup mesajý göster

        float timeout = 3f;
        float elapsed = 0f;

        if (rewardedAd == null || !rewardedAd.CanShowAd())
        {
            LoadRewardedAd();
        }

        while ((rewardedAd == null || !rewardedAd.CanShowAd()) && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        restoreButton.interactable = true; // Süre dolunca butonu tekrar aktif et
        popupText.gameObject.SetActive(false); // Popup'ý gizle

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show(reward =>
            {
                Debug.Log("Rewarded gösterildi, restore çalýþtýrýldý.");
                RestoreSavedPositions();
            });
        }
        else
        {
            Debug.LogWarning("Rewarded çýkmadý, interstitial gösteriliyor.");
            ShowInterstitialFallback();
        }
    }

    private void ShowInterstitialFallback()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.OnAdFullScreenContentClosed += HandleInterstitialClosedForRestore;
            interstitialAd.Show();
        }
        else
        {
            Debug.LogWarning("Interstitial da hazýr deðil.");
            ShowPopup("Ad is not ready. Please try again later.");
        }
    }

    private void HandleInterstitialClosedForRestore()
    {
        Debug.Log("Interstitial kapandý, restore iþlemi çalýþtýrýlýyor.");
        RestoreSavedPositions();
        interstitialAd.OnAdFullScreenContentClosed -= HandleInterstitialClosedForRestore; // dinlemeyi býrak
    }

    private void LoadInterstitialAd()
    {
        AdRequest request = new AdRequest();
        InterstitialAd.Load(interstitialAdUnitId, request, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error == null && ad != null)
            {
                interstitialAd = ad;
                Debug.Log("Interstitial yüklendi.");
            }
            else
            {
                Debug.LogWarning("Interstitial yüklenemedi: " + error?.GetMessage());
            }
        });
    }

    private void ShowPopup(string message)
    {
        if (popupText != null)
        {
            popupText.text = message;
            popupText.gameObject.SetActive(true);
            StartCoroutine(HidePopupAfterDelay(5f)); // 2 saniye sonra gizle
        }
        else
        {
            Debug.Log("[POPUP] " + message);
        }
    }

    private IEnumerator HidePopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (popupText != null)
        {
            popupText.gameObject.SetActive(false);
        }
    }
    private void HandleAdClosed()
    {
        Debug.Log("Reklam kapatýldý (ödül verilmeyebilir).");
        LoadRewardedAd();
    }

    private void HandleAdFailedToShow(AdError error)
    {
        Debug.LogError("Reklam gösterilemedi: " + error.GetMessage());
        LoadRewardedAd();
    }

    public void SaveAllShapeStates()
    {
        SaveShape("Cube", cubePrevPos, cubePrevRot);
        SaveShape("Sphere", spherePrevPos, spherePrevRot);
        SaveShape("Cylinder", cylinderPrevPos, cylinderPrevRot);
        foreach (var changer in objectChangers)
        {
            changer?.SaveTargetObjectState();
        }
        PlayerPrefs.SetInt(restoreKey, 1);
        PlayerPrefs.Save();
    }

    public void DelayedRestore() => StartCoroutine(DoRestoreWithDelay());

    private IEnumerator DoRestoreWithDelay()
    {
        yield return new WaitForEndOfFrame();
        RestoreSavedPositions();
    }

    private void RestoreSavedPositions()
    {
        LoadShape("Cube", out cubePrevPos, out cubePrevRot);
        LoadShape("Sphere", out spherePrevPos, out spherePrevRot);
        LoadShape("Cylinder", out cylinderPrevPos, out cylinderPrevRot);

        cube.transform.position = cubePrevPos;
        cube.transform.rotation = cubePrevRot;
        sphere.transform.position = spherePrevPos;
        sphere.transform.rotation = spherePrevRot;
        cylinder.transform.position = cylinderPrevPos;
        cylinder.transform.rotation = cylinderPrevRot;

        HandleAllObjectChangers(cube);
        HandleAllObjectChangers(sphere);
        HandleAllObjectChangers(cylinder);

        PlayerPrefs.DeleteKey(restoreKey);
        PlayerPrefs.Save();
        restoreButton?.gameObject.SetActive(false);

        Debug.Log("Restore iþlemi tamamlandý.");
    }

    private void LoadShape(string key, out Vector3 pos, out Quaternion rot)
    {
        pos = new Vector3(
            PlayerPrefs.GetFloat(key + "X", 0f),
            PlayerPrefs.GetFloat(key + "Y", 0f),
            PlayerPrefs.GetFloat(key + "Z", 0f)
        );
        rot = Quaternion.Euler(
            PlayerPrefs.GetFloat(key + "RotX", 0f),
            PlayerPrefs.GetFloat(key + "RotY", 0f),
            PlayerPrefs.GetFloat(key + "RotZ", 0f)
        );
    }

    private void SaveShape(string key, Vector3 pos, Quaternion rot)
    {
        PlayerPrefs.SetFloat(key + "X", pos.x);
        PlayerPrefs.SetFloat(key + "Y", pos.y);
        PlayerPrefs.SetFloat(key + "Z", pos.z);
        PlayerPrefs.SetFloat(key + "RotX", rot.eulerAngles.x);
        PlayerPrefs.SetFloat(key + "RotY", rot.eulerAngles.y);
        PlayerPrefs.SetFloat(key + "RotZ", rot.eulerAngles.z);
    }

    private void HandleAllObjectChangers(GameObject shape)
    {
        if (shape == null) return;
        var changers = shape.GetComponents<ObjectChanger>();
        foreach (var changer in changers)
        {
            changer?.HandleRestore();
        }
    }
}