using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Serializable]
    public struct Stats
    {
        public float[] attack;
        public float attackSpeed;
        public float flamethrowerSpeed;
        public float flamethrowerSize;
        public int jumpAmount;
        public float jumpHeight;
        public PlayerHealth health;
        public Mana mana;
        public float speed;
    }
    
    public Dictionary<UpgradeCoin.UpgradeType, Upgrade> upgrades = new Dictionary<UpgradeCoin.UpgradeType, Upgrade>();
    [SerializeField] Stats baseStats = default;
    [SerializeField] ItemHUD itemHUD = default;
    [SerializeField] Weapon[] weapons = default;
    public static GameObject player = default;
    PlayerMovement playerMovement;
    private void Awake()
    {
        player = this.gameObject;
    }
    void Start()
    {
        upgrades.Clear();
        AddToDictionary();
        playerMovement = GetComponent<PlayerMovement>();
        ApplyStats(this.baseStats);
        itemHUD.UpdateAllText();
    }

    void ApplyStats(Stats stats)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].Damage = stats.attack[i];
        }

        weapons[0].AttackRate = stats.attackSpeed;

        playerMovement.MaxJumpCount = stats.jumpAmount;
        playerMovement.JumpHeight = stats.jumpHeight;
        playerMovement.Speed = stats.speed;

        ParticleSystem.MainModule particleMain = weapons[1].GetComponentInChildren<ParticleSystem>().main;
        particleMain.startSpeedMultiplier = stats.flamethrowerSpeed;
        particleMain.startSizeMultiplier = stats.flamethrowerSize;
    }

    void AddToDictionary()
    {
        upgrades.Add(UpgradeCoin.UpgradeType.Armor, new Upgrade(new Armor()));
        upgrades.Add(UpgradeCoin.UpgradeType.Attack, new Upgrade(new Attack())); //
        upgrades.Add(UpgradeCoin.UpgradeType.AttackSpeed, new Upgrade(new AttackSpeed()));
        upgrades.Add(UpgradeCoin.UpgradeType.FlamethrowerRange, new Upgrade(new FlamethrowerRange()));
        upgrades.Add(UpgradeCoin.UpgradeType.JumpAmount, new Upgrade(new JumpAmount()));
        upgrades.Add(UpgradeCoin.UpgradeType.JumpHeight, new Upgrade(new JumpHeight()));
        upgrades.Add(UpgradeCoin.UpgradeType.ManaRegeneration, new Upgrade(new ManaRegeneration()));
        upgrades.Add(UpgradeCoin.UpgradeType.MaxHealth, new Upgrade(new MaxHealth()));
        upgrades.Add(UpgradeCoin.UpgradeType.MaxMana, new Upgrade(new MaxMana()));
        upgrades.Add(UpgradeCoin.UpgradeType.PercentImmune, new Upgrade(new PercentImmune()));
        upgrades.Add(UpgradeCoin.UpgradeType.Regeneration, new Upgrade(new Regeneration()));
        upgrades.Add(UpgradeCoin.UpgradeType.Speed, new Upgrade(new Speed()));
    }
    
    public void IncrementUpgrade(UpgradeCoin.UpgradeType type)
    {
        upgrades[type].IncrementCount();
        upgrades[type].behavior.ChangeStat(ref baseStats, upgrades[type].Count);

        ApplyStats(baseStats);
        itemHUD.UpdateAllText();
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.gameObject.GetComponent<UpgradeCoin>() != null)
        {
            hit.gameObject.GetComponent<UpgradeCoin>().OnPlayerGrab();
        }
    }
}
