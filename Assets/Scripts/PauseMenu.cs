using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;


public class PauseMenu : MonoBehaviour
{
    public GameObject Container;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Container.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            TogglePauseMenu();
        }
    }
    public void TogglePauseMenu()
    {
        if (Container.activeSelf)
        {
            Container.SetActive(false);
            Time.timeScale = 1f; // Resume the game
        }
        else
        {
            Container.SetActive(true);
            Time.timeScale = 0f; // Pause the game
        }
    }
    public void ResumeGame()
    {
        Container.SetActive(false);
        Time.timeScale = 1f; // Resume the game
    }
}
