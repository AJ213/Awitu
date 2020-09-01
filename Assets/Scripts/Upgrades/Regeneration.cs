using UnityEngine;

public class Regeneration : UpgradeBehavior
{
    public void ChangeStat(ref Player.Stats stats, int count)
    {
        float percentIncrease = 2;
        stats.health.RegenAmount *= percentIncrease;
    }
}


