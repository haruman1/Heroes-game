using SQLite;

public class PlayerData
{
    [PrimaryKey]
    public int Id { get; set; }

    public int Coin { get; set; }

    public int Level { get; set; }

    public int HP { get; set; }

    public int Heart { get; set; }
}
