using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : Health
{
    public static bool playerDead = false;
    [SerializeField] protected AudioSource immune = default;
    public float Armor { get => armor; set => armor = value; }
    [SerializeField] protected float armor = 0;
    public float PercentImmune { get => percentImmune; set => percentImmune = value; }
    [SerializeField] protected float percentImmune = 1;
    public UnityEvent PlayerDead;
    [SerializeField] Animator redBarsAnim = default;
    protected override void Start()
    {
        UpdateMaxStat();
        playerDead = false;
    }
    public override float CurrrentStat
    {
        get
        {
            return currentStat;
        }
        set
        {
            if (value - currentStat < 0)
            {
                
                
                if (value - currentStat + armor > 0 || Random.Range(1f, 2f) < percentImmune)
                {
                    if (!immune.isPlaying && immune != null)
                        immune.Play();
                    return;
                }
                else
                {
                    currentStat = value + armor;
                }
                PlayHitSound();
                redBarsAnim.SetTrigger("Fade");
            }
            else
            {
                currentStat = value;
            }
            if (currentStat <= 0)
            {
                Death();
                return;
            }
            else if (currentStat > maxStat)
            {
                currentStat = maxStat;
            }

            if (statBar != null)
            {
                statBar.SetValue(currentStat);
            }
        }
    }
    protected override void Death()
    {
        playerDead = true;
        PlayerDead.Invoke();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.GetComponentInParent<Laser>() != null)
        {
            CurrrentStat -= other.GetComponentInParent<Laser>().Damage;
        }
    }

}
