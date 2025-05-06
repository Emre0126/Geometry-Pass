using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SphereKinematicControl : MonoBehaviour
{
    public GameObject sphere; // sphere nesnesini atayýn
    public List<Button> buttonsToDisable; // Ýþlevini kapatmak istediðiniz butonlarý atayýn
    private HashSet<Vector2Int> topPositions = new HashSet<Vector2Int>(); // 'top' pozisyonlarýný tutmak için
    private List<CanvasGroup> buttonCanvasGroups = new List<CanvasGroup>();

    void Start()
    {
        // 'top' etiketine sahip tüm nesnelerin pozisyonlarýný bul
        GameObject[] tops = GameObject.FindGameObjectsWithTag("top");
        foreach (GameObject top in tops)
        {
            Vector2Int position = new Vector2Int(Mathf.RoundToInt(top.transform.position.x), Mathf.RoundToInt(top.transform.position.z));
            topPositions.Add(position);
        }

        // Butonlara geçici bir CanvasGroup ekleyerek buttonCanvasGroups listesine ekle
        foreach (Button button in buttonsToDisable)
        {
            if (button != null)
            {
                CanvasGroup canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
                buttonCanvasGroups.Add(canvasGroup);
            }
        }
    }

    void Update()
    {
        // 'top' etiketine sahip tüm aktif nesnelerin pozisyonlarýný güncelle
        topPositions.Clear();
        GameObject[] tops = GameObject.FindGameObjectsWithTag("top");
        foreach (GameObject top in tops)
        {
            if (top.activeSelf) // Sadece aktif olanlarý kontrol et
            {
                Vector2Int position = new Vector2Int(Mathf.RoundToInt(top.transform.position.x), Mathf.RoundToInt(top.transform.position.z));
                topPositions.Add(position);
            }
        }

        // sphere nesnesinin atanmýþ olup olmadýðýný kontrol et
        if (sphere == null)
        {
            Debug.LogError("Sphere nesnesi atanmadý!");
            return;
        }

        Rigidbody rb = sphere.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Sphere nesnesinde Rigidbody bileþeni bulunmuyor!");
            return;
        }

        // sphere x ve z pozisyonlarýný tam sayý olarak al
        int sphereX = Mathf.RoundToInt(sphere.transform.position.x);
        int sphereZ = Mathf.RoundToInt(sphere.transform.position.z);
        Vector2Int spherePosition = new Vector2Int(sphereX, sphereZ);

        // sphere top pozisyonlarýndan birinde deðilse isKinematic'i kapat
        if (!topPositions.Contains(spherePosition))
        {
            rb.isKinematic = false;
            foreach (CanvasGroup canvasGroup in buttonCanvasGroups)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = false; // Butonun týklanabilirliðini devre dýþý býrak
                }
            }
        }
        else
        {
            rb.isKinematic = true; // Sphere doðru pozisyona geldiðinde isKinematic'i aç
            foreach (CanvasGroup canvasGroup in buttonCanvasGroups)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = true; // Butonun týklanabilirliðini etkinleþtir
                }
            }
        }
    }

}
