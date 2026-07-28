using UnityEngine;

public class LoadScene : MonoBehaviour
{
    public void LoadSceneNew(string sceneName)
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MuatScene(sceneName);
        }
        else
        {
            // Fallback (seharusnya tidak terjadi karena ada Bootstrapper)
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    public void LoadCurrentSceneGame()
    {
        Time.timeScale = 1f;
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MuatScene(currentScene);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }
    }
}
