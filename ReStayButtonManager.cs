using UnityEngine;

public class ReStayButtonManager : MonoBehaviour
{
    public GameObject exitPanel;
    public GameObject exitImage;
    public GameObject button1;
    public GameObject button2;

    public void ShowExitUI()
    {
        exitPanel.SetActive(false);
        exitImage.SetActive(false);
        button1.SetActive(false);
        button2.SetActive(false);
    }
}
