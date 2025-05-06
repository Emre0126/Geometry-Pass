using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SphereKinematicControl : MonoBehaviour
{
    public GameObject sphere; // sphere nesnesini atay�n
    public List<Button> buttonsToDisable; // ��levini kapatmak istedi�iniz butonlar� atay�n
    private HashSet<Vector2Int> topPositions = new HashSet<Vector2Int>(); // 'top' pozisyonlar�n� tutmak i�in
    private List<CanvasGroup> buttonCanvasGroups = new List<CanvasGroup>();

    void Start()
    {
        // 'top' etiketine sahip t�m nesnelerin pozisyonlar�n� bul
        GameObject[] tops = GameObject.FindGameObjectsWithTag("top");
        foreach (GameObject top in tops)
        {
            Vector2Int position = new Vector2Int(Mathf.RoundToInt(top.transform.position.x), Mathf.RoundToInt(top.transform.position.z));
            topPositions.Add(position);
        }

        // Butonlara ge�ici bir CanvasGroup ekleyerek buttonCanvasGroups listesine ekle
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
        // 'top' etiketine sahip t�m aktif nesnelerin pozisyonlar�n� g�ncelle
        topPositions.Clear();
        GameObject[] tops = GameObject.FindGameObjectsWithTag("top");
        foreach (GameObject top in tops)
        {
            if (top.activeSelf) // Sadece aktif olanlar� kontrol et
            {
                Vector2Int position = new Vector2Int(Mathf.RoundToInt(top.transform.position.x), Mathf.RoundToInt(top.transform.position.z));
                topPositions.Add(position);
            }
        }

        // sphere nesnesinin atanm�� olup olmad���n� kontrol et
        if (sphere == null)
        {
            Debug.LogError("Sphere nesnesi atanmad�!");
            return;
        }

        Rigidbody rb = sphere.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Sphere nesnesinde Rigidbody bile�eni bulunmuyor!");
            return;
        }

        // sphere x ve z pozisyonlar�n� tam say� olarak al
        int sphereX = Mathf.RoundToInt(sphere.transform.position.x);
        int sphereZ = Mathf.RoundToInt(sphere.transform.position.z);
        Vector2Int spherePosition = new Vector2Int(sphereX, sphereZ);

        // sphere top pozisyonlar�ndan birinde de�ilse isKinematic'i kapat
        if (!topPositions.Contains(spherePosition))
        {
            rb.isKinematic = false;
            foreach (CanvasGroup canvasGroup in buttonCanvasGroups)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = false; // Butonun t�klanabilirli�ini devre d��� b�rak
                }
            }
        }
        else
        {
            rb.isKinematic = true; // Sphere do�ru pozisyona geldi�inde isKinematic'i a�
            foreach (CanvasGroup canvasGroup in buttonCanvasGroups)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = true; // Butonun t�klanabilirli�ini etkinle�tir
                }
            }
        }
    }

}
