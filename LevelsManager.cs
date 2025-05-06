using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject[] panels; // Panel objelerini burada saklayacaðýz
    public GameObject[] locks;  // Lock objelerini burada saklayacaðýz

    private void Start()
    {
        UnlockLevels();
    }

    void UnlockLevels()
    {
        for (int i = 2; i <= 20; i++) // Level2'den Level20'ye kadar kontrol et
        {
            if (PlayerPrefs.GetInt($"Panel{i}", 1) == 0) // Panel açýlmýþ mý?
            {
                panels[i - 2].SetActive(false);
            }

            if (PlayerPrefs.GetInt($"Lock{i}", 1) == 0) // Lock kaldýrýlmýþ mý?
            {
                locks[i - 2].SetActive(false);
            }
        }
    }
}
