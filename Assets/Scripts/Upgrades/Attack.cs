public class Attack : UpgradeBehavior
{
    public void ChangeStat(ref Player.Stats stats, int count)
    {
        float percentIncrease = 2;
        for (int i = 0; i < stats.attack.Length; i++)
        {
            stats.attack[i] *= percentIncrease;
        }
    }
}
