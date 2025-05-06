using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectChanger : MonoBehaviour
{
    public GameObject targetObject;
    public GameObject targetObjectCopy;
    public Vector3 targetDifference;
    public float tolerance = 0.1f;

    private Vector3 initialPosition;
    private string restoreStateKey;

    void Awake()
    {
        // Her bile�ene benzersiz anahtar:
        var comps = GetComponents<ObjectChanger>();
        int idx = System.Array.IndexOf(comps, this);
        restoreStateKey = $"{SceneManager.GetActiveScene().name}_{gameObject.name}_{idx}_State";

        // "Yeni oyun" durumunda kendi anahtar�n� temizle
        if (PlayerPrefs.GetInt("IsRestart", 0) == 0)
        {
            PlayerPrefs.DeleteKey(restoreStateKey);
            PlayerPrefs.Save();
        }
    }

    void Start()
    {
        initialPosition = transform.position;

        // SADECE ba�lang��ta kapal� tut:
        targetObject.SetActive(false);
        targetObjectCopy.SetActive(true);
    }

    void Update()
    {
        Vector3 diff = transform.position - initialPosition;
        if (Mathf.Abs(diff.x - targetDifference.x) < tolerance &&
            Mathf.Abs(diff.y - targetDifference.y) < tolerance &&
            Mathf.Abs(diff.z - targetDifference.z) < tolerance)
        {
            // Hedefe ula�t�, g�r�n�rl��� de�i�tir
            targetObject.SetActive(true);
            targetObjectCopy.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Sahne kapan�rken veya restart tetiklenirken otomatik kaydet
        SaveTargetObjectState();
    }

    public void SaveTargetObjectState()
    {
        int isActive = targetObject.activeSelf ? 1 : 0;
        PlayerPrefs.SetInt(restoreStateKey, isActive);
        PlayerPrefs.Save();
    }

    // Restore butonuna bas�ld���nda bunu �a��r�n:
    public void HandleRestore()
    {
        int saved = PlayerPrefs.GetInt(restoreStateKey, -1);
        if (saved == 1)
        {
            targetObject.SetActive(true);
            targetObjectCopy.SetActive(false);
        }
        else if (saved == 0)
        {
            targetObject.SetActive(false);
            targetObjectCopy.SetActive(true);
        }
    }
}
