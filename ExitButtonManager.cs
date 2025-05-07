using UnityEngine;

public class ExitButtonManager : MonoBehaviour
{
    public GameObject exitPanel;
    public GameObject exitImage;
    public GameObject button1;
    public GameObject button2;

    public void ShowExitUI()
    {
        exitPanel.SetActive(true);
        exitImage.SetActive(true);
        button1.SetActive(true);
        button2.SetActive(true);
    }
}
