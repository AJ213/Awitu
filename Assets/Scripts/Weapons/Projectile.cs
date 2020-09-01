using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float despawnDistance = 100;
    

    [SerializeField] float damage = 1;
    [SerializeField] GameObject impactEffect = null;

    public void SetStats(float damage)
    {
        this.damage = damage;
    }
    Transform player = default;
    private void Start()
    {
        player = Player.player.transform;
    }

    private void Update()
    {
        if (player == null)
        {
            DeathEffect();
            return;
        }

        if (Vector3.Distance(this.transform.position, player.position) > despawnDistance)
        {
            DeathEffect();
            Destroy(this.gameObject);
        }
    }

    public static Vector3 CalculateDirection(Vector3 currentLocation, Vector3 target)
    {
        Vector3 targetDirection = new Vector3(target.x - currentLocation.x, target.y - currentLocation.y, target.z - currentLocation.z);
        targetDirection.Normalize();
        return targetDirection;
    }

    void DeathEffect()
    {
        GameObject go = Instantiate(impactEffect, this.transform.position, Quaternion.identity);
        Destroy(go, 2);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collisionGO = collision.gameObject;
        Rigidbody rb = collisionGO.GetComponent<Rigidbody>();

        if (!collisionGO.CompareTag(this.gameObject.tag))
        {
            Health health = collisionGO.GetComponent<Health>();
            if (health != null)
            {
                health.CurrrentStat += -damage;
            }
        }
        DeathEffect();
        Destroy(this.gameObject);
    }
}
