using UnityEngine;
using TMPro;

public class Fps : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    public static Fps Instance;
    private readonly int[] fpsLimits = { 30, 60, 120 };
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject); // Diganti dengan arsitektur Additive CoreScene
    }
    public void NextFPS()
    {
        CycleFpsLimit(1);
        RefreshLabel();
    }

    public void PrevFPS()
    {
        CycleFpsLimit(-1);
        RefreshLabel();
    }

    private void Start()
    {
        RefreshLabel();
    }

    private void CycleFpsLimit(int direction)
{
    int currentFps = SettingManager.Instance.GetPendingFPS();

    int currentIndex = System.Array.IndexOf(fpsLimits, currentFps);

    if (currentIndex == -1)
        currentIndex = 1;

    currentIndex += direction;

    if (currentIndex >= fpsLimits.Length)
        currentIndex = 0;

    if (currentIndex < 0)
        currentIndex = fpsLimits.Length - 1;

    SettingManager.Instance.SetFPS(fpsLimits[currentIndex]);

    RefreshLabel();
}
  public void RefreshLabel()
{
    fpsText.text =
        SettingManager.Instance.GetPendingFPS() + " FPS";
}
}
