using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] enemies = default;
    public static int aliveEnemies = 0;
    [SerializeField] int maxEnemies = 100;
    [SerializeField] float enemySpawningCoeff = 0.2f;
    [SerializeField] Vector2 spawnDistanceFromPlayer = new Vector2(20, 50);
    [SerializeField] float mapMaxHeight = 50;
    [SerializeField] LayerMask ground = default;

    float nextTimeToSpawn = 0;
    Transform player;
    void Start()
    {
        player = Player.player.transform;
        PlayerHealth.playerDead = false;
        aliveEnemies = 0;
    }

    private void Update()
    {

        if (Time.time >= nextTimeToSpawn)
        {
            nextTimeToSpawn = Time.time + (1 / EnemySpawnFrequency());



            SpawnEnemy(enemies[Random.Range(0, enemies.Length)], RandomViableSpawnLocation());
        }
    }
    void SpawnEnemy(GameObject enemy, Vector3 location)
    {
        if (aliveEnemies >= maxEnemies)
            return;
        GameObject go = Instantiate(enemy, location, Quaternion.identity, this.transform);
        ScaleEnemyByDifficulty(go);
        aliveEnemies++;
    }

    void ScaleEnemyByDifficulty(GameObject enemy)
    {
        float calculation = DifficultlyHandler.difficulty * Time.timeSinceLevelLoad / 60;
        if (calculation == 0)
            calculation = 0.1f;

        Weapon[] weapons = enemy.GetComponents<Weapon>();
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].Damage *= calculation;
        }

        enemy.GetComponent<Health>().MaxStat *= calculation;
        enemy.GetComponent<Health>().RegenAmount *= calculation;
    }

    Vector3 RandomViableSpawnLocation()
    {
        if (player == null)
            return new Vector3(0, mapMaxHeight, 0);

        float x = player.position.x + ((Random.Range(0,2)*2-1) * Random.Range(spawnDistanceFromPlayer.x, spawnDistanceFromPlayer.y));
        float z = player.position.z + ((Random.Range(0, 2) * 2 - 1) * Random.Range(spawnDistanceFromPlayer.x, spawnDistanceFromPlayer.y));

        Vector3 xzPosition = new Vector3(x, mapMaxHeight, z);
        if (Physics.Raycast(xzPosition, Vector3.down, out RaycastHit hit, ground))
        {
            return new Vector3(x, hit.point.y+5, z);
        }
        return new Vector3(x, mapMaxHeight, z);
    }

    float EnemySpawnFrequency()
    {
        return (DifficultlyHandler.difficulty * enemySpawningCoeff) + -2.8f;
    }
}
