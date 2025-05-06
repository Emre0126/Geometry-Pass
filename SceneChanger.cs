using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Hedef sahnenin adýný parametre olarak alýr ve geçiþ yapar.
    public void SwitchScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
}
