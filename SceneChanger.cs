using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Hedef sahnenin ad�n� parametre olarak al�r ve ge�i� yapar.
    public void SwitchScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
}
