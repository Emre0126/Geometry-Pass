using UnityEngine;

public class ExitApplication : MonoBehaviour
{
    public void ExitGame()
    {
        // Oyun içindeyken çalýþtýrýldýðýnda log mesajý verir
        Debug.Log("Uygulama kapatýlýyor...");

        // Uygulamadan çýk
        Application.Quit();
    }
}
