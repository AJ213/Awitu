using UnityEngine;

public class MaxHealth : UpgradeBehavior
{
    public void ChangeStat(ref Player.Stats stats, int count)
    {
        float percentIncrease = 2;
        stats.health.MaxStat *= percentIncrease;
    }
}


