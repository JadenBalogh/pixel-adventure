using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftEntry : MonoBehaviour
{
    public Item Item { get; set; }

    public void Buy()
    {
        GameManager.InventorySystem.BuyItem(Item);
    }
}

