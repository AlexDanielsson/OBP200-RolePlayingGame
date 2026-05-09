namespace OBP200_RolePlayingGame;

public class BossEnemy : Enemy
{
    public override int CalculateAttackDamage()
    {
        return AttackStat + 2;
    }
}