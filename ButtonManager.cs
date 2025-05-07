using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public GameObject button1;
    public GameObject button2;
    public GameObject sphere;
    public GameObject Cube;
    public GameObject Cylinder;
    public GameObject target1;
    public GameObject target2;
    public GameObject target3;
    public GameObject target4;
    public void OnButtonClick()
    {
       
        if (button1 != null)
            button1.SetActive(false);
        if (sphere != null)
            sphere.SetActive(false);
        if (button2 != null)
            button2.SetActive(false);
        if (Cube != null)
            Cube.SetActive(false);
        if (Cylinder != null)
            Cylinder.SetActive(false);
        if (target1 != null)
            target1.SetActive(false);
        if (target2 != null)
            target2.SetActive(false);
        if (target3 != null)
            target3.SetActive(false);
        if(target4 != null)
            target4.SetActive(false);
    }
}
