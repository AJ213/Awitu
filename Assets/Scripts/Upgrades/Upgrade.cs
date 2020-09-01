public class Upgrade
{
    int count = 0;
    public UpgradeBehavior behavior;

    public Upgrade(UpgradeBehavior behavior) => this.behavior = behavior;

    public void IncrementCount() => count++;
    public int Count => count;

    public void ChangeStats(Player.Stats stats)
    {
        behavior.ChangeStat(ref stats, count);
    }

    
}
