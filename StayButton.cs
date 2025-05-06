using UnityEngine;

public class StayButton : MonoBehaviour
{
    public GameObject exitPanel; // Panel
    public GameObject exitImage; // Image
    public GameObject button1;   // Ýlk buton
    public GameObject button2;   // Ýkinci buton

    public void ShowExitUI()
    {
        // Paneli görünür yap
        exitPanel.SetActive(false);

        // Image'i görünür yap
        exitImage.SetActive(false);

        // Butonlarý görünür yap
        button1.SetActive(false);
        button2.SetActive(false);
    }
}
