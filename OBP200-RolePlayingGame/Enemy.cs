namespace OBP200_RolePlayingGame;

public class Enemy
{
    public string Type { get; set; }
    public string Name { get; set; }
    
    public int HP { get; set; }
    public int AttackStat {get; set;}
    public int DefenceStat {get; set;}
    
    public int ExpReward {get; set;}
    public int GoldReward {get; set;}

    public virtual int CalculateAttackDamage()
    {
        return AttackStat;
    }
}