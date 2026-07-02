using UnityEngine;
using TMPro;

public class Fps : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private readonly int[] fpsLimits = { 30, 60, 120 };

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
        GameSettingsData settings = DatabaseManager.GetOrCreateInstance().GetSettingsData();
        if (settings == null) return;

        int currentIndex = System.Array.IndexOf(fpsLimits, settings.FpsLimit);
        if (currentIndex == -1) currentIndex = 1; // Default to 60 FPS

        currentIndex += direction;
        if (currentIndex >= fpsLimits.Length) currentIndex = 0;
        if (currentIndex < 0) currentIndex = fpsLimits.Length - 1;

        settings.FpsLimit = fpsLimits[currentIndex];
        DatabaseManager.GetOrCreateInstance().SaveSettings(settings);
    }

    void RefreshLabel()
    {
        if (fpsText == null)
            return;

        GameSettingsData settings = DatabaseManager.GetOrCreateInstance().GetSettingsData();
        int currentFps = settings != null ? settings.FpsLimit : 60;
        
        fpsText.text = currentFps + " FPS";
        Application.targetFrameRate = currentFps;
    }
}
