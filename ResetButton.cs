using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public GameObject exitPanel; // Panel
    public GameObject exitImage; // Image
    public GameObject button1;   // �lk buton
    public GameObject button2;   // �kinci buton

    public void ShowExitUI()
    {
        // Paneli g�r�n�r yap
        exitPanel.SetActive(true);

        // Image'i g�r�n�r yap
        exitImage.SetActive(true);

        // Butonlar� g�r�n�r yap
        button1.SetActive(true);
        button2.SetActive(true);
    }
}
