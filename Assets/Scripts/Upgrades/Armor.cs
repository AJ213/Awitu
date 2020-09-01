public class Armor : UpgradeBehavior
{
    public void ChangeStat(ref Player.Stats stats, int count)
    {
        if(count == 1)
        {
            stats.health.Armor = 6;
            return;
        }
        float armorIncrease = 1.6f;
        stats.health.Armor *= armorIncrease;
    }
}
