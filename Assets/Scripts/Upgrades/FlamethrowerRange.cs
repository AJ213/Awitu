using JetBrains.Annotations;

public class FlamethrowerRange : UpgradeBehavior
{
    public void ChangeStat(ref Player.Stats stats, int count)
    {
        int capIncrease = 500;
        float smoothness = 0.03f;
        int baseSpeed = 20;
        int baseSize = 5;

        float speedCalculation = capIncrease - (capIncrease / (smoothness * count + 1)) + baseSpeed;
        float sizeCalculation = capIncrease/20 - (capIncrease/20 / (smoothness * count + 1)) + baseSize;
        stats.flamethrowerSpeed = speedCalculation;
        stats.flamethrowerSize = sizeCalculation;
    }
}
