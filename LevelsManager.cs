using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject[] panels;
    public GameObject[] locks;

    private void Start()
    {
        UnlockLevels();
    }

    void UnlockLevels()
    {
        for (int i = 2; i <= 20; i++)
        {
            if (PlayerPrefs.GetInt($"Panel{i}", 1) == 0)
            {
                panels[i - 2].SetActive(false);
            }

            if (PlayerPrefs.GetInt($"Lock{i}", 1) == 0)
            {
                locks[i - 2].SetActive(false);
            }
        }
    }
}
