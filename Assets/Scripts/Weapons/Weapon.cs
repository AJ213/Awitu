using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public virtual float Damage { get => damage; set => damage = value; }
    [SerializeField] protected float damage = 1;
    [SerializeField] protected float range = 1;
    public float AttackRate { get => attackRate; set => attackRate = value; }
    [SerializeField] protected float attackRate = 10;

    [SerializeField] protected GameObject projectile;
    [SerializeField] protected Vector3 offset = default;
    [SerializeField] protected Transform target = null;
    [SerializeField] protected AudioSource shotSound = null;

    float nextTimeToAttack = 0;

    protected virtual void Update()
    {
        TryToAttack();
    }

    protected void TryToAttack()
    {
        
        if (Time.time >= nextTimeToAttack)
        {
            nextTimeToAttack = Time.time + (1 / attackRate);
            Attack();
        }
    }

    protected abstract void Attack();
}
