using UnityEngine;

public class ExitButtonManager : MonoBehaviour
{
    public GameObject exitPanel; // Panel
    public GameObject exitImage; // Image
    public GameObject button1;   // Ýlk buton
    public GameObject button2;   // Ýkinci buton

    public void ShowExitUI()
    {
        // Paneli görünür yap
        exitPanel.SetActive(true);

        // Image'i görünür yap
        exitImage.SetActive(true);

        // Butonlarý görünür yap
        button1.SetActive(true);
        button2.SetActive(true);
    }
}
