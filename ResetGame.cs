using UnityEngine;

public class GameReset : MonoBehaviour
{
    public void ResetGameData()
    {
        // T�m PlayerPrefs verilerini temizle
        PlayerPrefs.DeleteAll();

        // De�i�iklikleri kaydet
        PlayerPrefs.Save();

        // �ste�e ba�l�: Oyunu ba�tan ba�lat
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
