using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void SwitchScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
}
