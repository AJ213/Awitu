using UnityEngine;

public class JumpAmount : UpgradeBehavior
{

    public void ChangeStat(ref Player.Stats stats, int count)
    {
        int jumpAmount = 1;
        stats.jumpAmount += jumpAmount;

        //Die();
    }
}


// Input baseStat, Count, float,