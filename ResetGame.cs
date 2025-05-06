using UnityEngine;

public class GameReset : MonoBehaviour
{
    public void ResetGameData()
    {
        // Tüm PlayerPrefs verilerini temizle
        PlayerPrefs.DeleteAll();

        // Deðiþiklikleri kaydet
        PlayerPrefs.Save();

        // Ýsteðe baðlý: Oyunu baþtan baþlat
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
