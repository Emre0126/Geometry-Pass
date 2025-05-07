using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    
    public string sceneName;

    
    public void ChangeScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            
            return;
        }
        SceneManager.LoadScene(sceneName);
       
        
    }
}
