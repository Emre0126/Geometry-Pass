using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SphereKinematicControl : MonoBehaviour
{
    public GameObject sphere;
    public List<Button> buttonsToDisable;
    private HashSet<Vector2Int> topPositions = new HashSet<Vector2Int>();
    private List<CanvasGroup> buttonCanvasGroups = new List<CanvasGroup>();
    void Start()
    {
        GameObject[] tops = GameObject.FindGameObjectsWithTag("top");
        foreach (GameObject top in tops)
        {
            Vector2Int position = new Vector2Int(Mathf.RoundToInt(top.transform.position.x), Mathf.RoundToInt(top.transform.position.z));
            topPositions.Add(position);
        }
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
        topPositions.Clear();
        GameObject[] tops = GameObject.FindGameObjectsWithTag("top");
        foreach (GameObject top in tops)
        {
            if (top.activeSelf)
            {
                Vector2Int position = new Vector2Int(Mathf.RoundToInt(top.transform.position.x), Mathf.RoundToInt(top.transform.position.z));
                topPositions.Add(position);
            }
        }
        if (sphere == null)
        {          
            return;
        }
        Rigidbody rb = sphere.GetComponent<Rigidbody>();
        if (rb == null)
        {

            return;
        }
        int sphereX = Mathf.RoundToInt(sphere.transform.position.x);
        int sphereZ = Mathf.RoundToInt(sphere.transform.position.z);
        Vector2Int spherePosition = new Vector2Int(sphereX, sphereZ);
        if (!topPositions.Contains(spherePosition))
        {
            rb.isKinematic = false;
            foreach (CanvasGroup canvasGroup in buttonCanvasGroups)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = false;
                }
            }
        }
        else
        {
            rb.isKinematic = true;
            foreach (CanvasGroup canvasGroup in buttonCanvasGroups)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = true;
                }
            }
        }
    }

}
