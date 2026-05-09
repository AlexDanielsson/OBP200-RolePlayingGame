namespace OBP200_RolePlayingGame;

public class Player
{
    public string PlayerName { get; set; }
    public string PlayerClass { get; set; }
    
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    
    public int AttackStat { get; set; }
    public int DefenceStat { get; set; }
    
    public int PlayerExp { get; set; }
    public int PlayerLevel { get; set; }
    
    public int GoldInventory { get; set; }
    public int PotionsInventory { get; set; }

    public List<string> Inventory { get; set; } = new();

    public Player()
    {
        
    }
}