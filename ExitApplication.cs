using UnityEngine;

public class ExitApplication : MonoBehaviour
{
    public void ExitGame()
    {
        // Oyun i�indeyken �al��t�r�ld���nda log mesaj� verir
        Debug.Log("Uygulama kapat�l�yor...");

        // Uygulamadan ��k
        Application.Quit();
    }
}
