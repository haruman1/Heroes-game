using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public TMP_Text panelName;

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetPanelName(string name)
    {
        panelName.text = name;
    }
}
