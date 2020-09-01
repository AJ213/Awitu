using TMPro;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] TMP_Text count = default;
    [SerializeField] UpgradeCoin.UpgradeType upgrade = default;
    public Player player;

    public void UpdateCount()
    {
        count.text = player.upgrades[upgrade].Count.ToString();
    }
}
