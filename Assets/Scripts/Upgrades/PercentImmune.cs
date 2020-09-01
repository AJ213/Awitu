public class PercentImmune : UpgradeBehavior
{
    public void ChangeStat(ref Player.Stats stats, int count)
    {
        float calculation = 1 - (1 / (0.15f * count + 1)) + 1;
        stats.health.PercentImmune = calculation;
    }

}
