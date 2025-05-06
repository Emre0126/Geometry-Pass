using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Tooltip("Ge�i� yapmak istedi�in sahnenin ad� veya build index'i")]
    public string sceneName;  // ya da int sceneIndex;

    // Butonun OnClick() olay�na bu fonksiyonu ba�layacaks�n
    public void ChangeScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneSwitcher: 'sceneName' bo� b�rak�lm��!");
            return;
        }
        SceneManager.LoadScene(sceneName);
        // E�er build index ile y�klenecekse:
        // SceneManager.LoadScene(sceneIndex);
    }
}
