using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Tooltip("Geçiþ yapmak istediðin sahnenin adý veya build index'i")]
    public string sceneName;  // ya da int sceneIndex;

    // Butonun OnClick() olayýna bu fonksiyonu baðlayacaksýn
    public void ChangeScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneSwitcher: 'sceneName' boþ býrakýlmýþ!");
            return;
        }
        SceneManager.LoadScene(sceneName);
        // Eðer build index ile yüklenecekse:
        // SceneManager.LoadScene(sceneIndex);
    }
}
