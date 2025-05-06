using UnityEngine;

public class StayButton : MonoBehaviour
{
    public GameObject exitPanel; // Panel
    public GameObject exitImage; // Image
    public GameObject button1;   // �lk buton
    public GameObject button2;   // �kinci buton

    public void ShowExitUI()
    {
        // Paneli g�r�n�r yap
        exitPanel.SetActive(false);

        // Image'i g�r�n�r yap
        exitImage.SetActive(false);

        // Butonlar� g�r�n�r yap
        button1.SetActive(false);
        button2.SetActive(false);
    }
}
