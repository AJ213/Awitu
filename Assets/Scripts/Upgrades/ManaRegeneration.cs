public class ManaRegeneration : UpgradeBehavior
{
    public void ChangeStat(ref Player.Stats stats, int count)
    {
        float percentIncrease = 2;
        stats.mana.RegenAmount *= percentIncrease;
    }
}


