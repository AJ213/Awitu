public class AttackSpeed : UpgradeBehavior
{
    public void ChangeStat(ref Player.Stats stats, int count)
    {
        float percentIncrease = 1.6f;
        stats.attackSpeed *= percentIncrease;
    }
}
