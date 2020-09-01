using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
public class UpgradeCoin : MonoBehaviour // On coin prefab
{
    [SerializeField] UpgradeType type = default;
    [SerializeField] protected string message = default;
    [SerializeField] GameObject text = default;
    bool touchedOnce = false;
    
    GameObject player;
    GameObject canvas;
    public enum UpgradeType
    {
        Armor,
        Attack,
        AttackSpeed,
        FlamethrowerRange,
        JumpAmount,
        JumpHeight,
        ManaRegeneration,
        MaxHealth,
        MaxMana,
        PercentImmune,
        Regeneration,
        Speed
    }
    
    void Start()
    {
        player = Player.player;
        canvas = GameObject.FindGameObjectWithTag("Message");
        touchedOnce = false;
    }

    public void OnPlayerGrab()
    {
        player = Player.player;
        if (touchedOnce)
        {
            return;
        }

        player.GetComponent<Player>().IncrementUpgrade(type);
        touchedOnce = true;
        Die();
    }

    protected void Die()
    {
        GameObject go = Instantiate(text, Vector3.zero , Quaternion.identity, canvas.transform.GetChild(0).transform);
        go.GetComponent<MessageController>().ShowMessage(message);
        go.transform.localPosition = text.GetComponent<MessageController>().location * UnityEngine.Random.Range(0.9f, 1.1f);
        Destroy(go, 2);
        Destroy(this.gameObject);
    }
}
