using UnityEngine;

public class RocketLauncher : Weapon
{
    [SerializeField] float projectileForce = default;
    private void Start()
    {
        target = Player.player.transform;
    }

    protected override void Attack()
    {
        if (target == null || Vector3.Distance(this.transform.position, target.position) > 100)
        {
            return;
        }

        shotSound.Play();

        GameObject go = Instantiate(projectile, this.transform.position + offset, Quaternion.LookRotation(target.position));
        projectile.GetComponent<Projectile>().SetStats(damage);
        go.GetComponent<Rigidbody>().AddForce(Projectile.CalculateDirection(this.transform.position, target.transform.position) * projectileForce, ForceMode.Impulse);
        go.transform.LookAt(target.transform);
    }

    
}
