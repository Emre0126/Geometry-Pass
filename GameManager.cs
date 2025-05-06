using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class GameManager : MonoBehaviour
{
    public CubePositionChecker cubeChecker;
    public CylinderPositionChecker cylinderChecker;
    public SpherePositionChecker sphereChecker;

    public GameObject cubeObject;
    public GameObject cylinderObject;
    public GameObject sphereObject;
    public GameObject up;
    public GameObject down;
    public GameObject left;
    public GameObject right;
    public GameObject button1;
    public GameObject button2;
    public GameObject button;
    public GameObject JustSphere;
    public GameObject Pause;
    public AudioSource AudioSource;
    public AudioSource LevelComplete;
    private bool gameStopped = false;
    public string nextSceneName;

    private InterstitialAd interstitialAd;
    private BannerView bannerView;

    private void Start()
    {
        LevelComplete.Stop();
        MobileAds.Initialize(initStatus => { });

        LoadInterstitialAd();
    }

    void Update()
    {
        if (!gameStopped)
        {
            if (cubeChecker.IsInTargetPosition() && cylinderChecker.IsInTargetPosition() && sphereChecker.IsInTargetPosition())
            {
                gameStopped = true;
                StartCoroutine(HandleLevelCompleteSequence());
            }
        }
    }

    IEnumerator HandleLevelCompleteSequence()
    {
        cubeObject.SetActive(true);
        cylinderObject.SetActive(true);
        sphereObject.SetActive(true);
        up.SetActive(false);
        down.SetActive(false);
        left.SetActive(false);
        right.SetActive(false);
        button2.SetActive(false);
        button1.SetActive(false);
        button.SetActive(false);
        JustSphere.SetActive(false);
        Pause.SetActive(false);
        LevelComplete.Play();

        if (AudioSource != null)
        {
            AudioSource.enabled = false;
        }

        UnlockNextLevel();

        
        LoadBannerAd();

        yield return new WaitForSecondsRealtime(5f);

        
        HideBannerAd();

        ShowInterstitialAd();
    }

    void UnlockNextLevel()
    {
        string currentLevel = SceneManager.GetActiveScene().name;
        if (currentLevel.StartsWith("Level"))
        {
            int levelNumber = int.Parse(currentLevel.Replace("Level", ""));
            int nextLevel = levelNumber + 1;

            if (nextLevel <= 20)
            {
                PlayerPrefs.SetInt($"Panel{nextLevel}", 0);
                PlayerPrefs.SetInt($"Lock{nextLevel}", 0);
                PlayerPrefs.Save();
            }
        }
    }

    void LoadInterstitialAd()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-8869277409030391/6174358238"; // Gerçek Interstitial ID
#else
        string adUnitId = "unexpected_platform";
#endif

        var adRequest = new AdRequest();

        InterstitialAd.Load(adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Interstitial reklam yüklenemedi: " + error);
                    return;
                }

                interstitialAd = ad;
                Debug.Log("Interstitial reklam yüklendi.");

                interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    SceneManager.LoadScene(nextSceneName);
                };

                interstitialAd.OnAdFullScreenContentFailed += (AdError adError) =>
                {
                    Debug.Log("Interstitial reklam gösterilemedi: " + adError);
                    SceneManager.LoadScene(nextSceneName);
                };
            });
    }

    void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial reklam hazýr deðil, sahneye geçiliyor.");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    void LoadBannerAd()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-8869277409030391/8847788887"; 
#else
        string adUnitId = "unexpected_platform";
#endif

        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        var adRequest = new AdRequest();
        bannerView.LoadAd(adRequest);
        bannerView.Show();
    }

    void HideBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
    }

    private void OnDestroy()
    {
        interstitialAd?.Destroy();
        
    }
}
