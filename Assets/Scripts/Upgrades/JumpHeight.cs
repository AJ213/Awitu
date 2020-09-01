public class JumpHeight : UpgradeBehavior
{
    public void ChangeStat(ref Player.Stats stats, int count)
    {
        int capIncrease = 22;
        float smoothness = 0.1f;
        int baseJumpHeight = 5;
        float calculation = capIncrease - (capIncrease / (smoothness * count + 1)) + baseJumpHeight;
        stats.jumpHeight = calculation;
    }
}





