using UnityEngine;

public class ItemHUD : MonoBehaviour
{

    [SerializeField] public Item[] items = new Item[12];


    public void UpdateAllText()
    {
        Player player = Player.player.GetComponent<Player>();
        foreach (Item item in items)
        {
            item.player = player;
            item.UpdateCount();
        }
    }
}
