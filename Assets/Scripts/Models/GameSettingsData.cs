using SQLite;

public class GameSettingsData
{
    [PrimaryKey]
    public int Id { get; set; }

    public int FpsLimit { get; set; }

    public float MasterVolume { get; set; }

    public float MusicVolume { get; set; }

    public float SfxVolume { get; set; }

    public float CameraZoom { get; set; }

    public int Fullscreen { get; set; }

    public int ResolutionWidth { get; set; }

    public int ResolutionHeight { get; set; }

    public string Language { get; set; }
}