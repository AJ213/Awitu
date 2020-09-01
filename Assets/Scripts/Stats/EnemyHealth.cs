using UnityEngine;

public class EnemyHealth : Health
{
    [SerializeField] float itemDropChance = 0.4f;
    [SerializeField] GameObject[] upgrades = default;
    [SerializeField] GameObject impactEffect = null;
    [SerializeField] bool spawnItem = true;
    [SerializeField] float despawnDistance = 500;
    [SerializeField] Transform player = default;
    protected override void Death()
    {
        EnemySpawner.aliveEnemies--;
        GameObject go = Instantiate(deathEffect, this.transform.position, Quaternion.identity);
        Destroy(go, 2);
        if(spawnItem)
        {
            SpawnItem(itemDropChance, this.transform.position);
        }
        spawnItem = true;
        Destroy(this.gameObject);
    }

    void Awake()
    {
        isDead = false;
        player = Player.player.transform;
    }

    protected override void Update()
    {
        if (currentStat < maxStat)
        {
            CurrrentStat += regenAmount * Time.deltaTime;
        }
        if(Vector3.Distance(new Vector3(this.transform.position.x, 0, this.transform.position.z), new Vector3(player.position.x, 0, player.position.z)) > despawnDistance)
        {
            spawnItem = false;
            Death();
        }
    }


    void SpawnItem(float chance, Vector3 position)
    {
        if (Random.Range(0f, 1f) > chance)
            return;

        int itemIndex = Random.Range(0, upgrades.Length);
        Instantiate(upgrades[itemIndex], position + 2*Vector3.up, Quaternion.identity);
    }

    private void OnParticleCollision(GameObject other)
     {

         if (other.GetComponentInParent<Flamethower>() != null)
         {
             if(Random.Range(0, 3) < 1)
             {
                 GameObject goImpact = Instantiate(impactEffect, this.transform.position + Vector3.up, Quaternion.LookRotation(other.transform.position), this.transform);
                 Destroy(goImpact, 1);
             }

             CurrrentStat -= other.GetComponentInParent<Flamethower>().Damage;
         }
     }
}
