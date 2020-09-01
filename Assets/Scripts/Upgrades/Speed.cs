public class Speed : UpgradeBehavior
{
    public void ChangeStat(ref Player.Stats stats, int count)
    {
        int capIncrease = 45;
        float smoothness = 0.1f;
        int baseSpeed = 15;
        float calculation = capIncrease - (capIncrease / (smoothness * count + 1));
        stats.speed = calculation + baseSpeed;

    }
}


