using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", order = 51, menuName = "Item")]
public class Item : ScriptableObject
{
    public string displayName = "Item";
    public ItemType itemType = ItemType.Basic;
    public CraftRequirement[] craftRequirements;

    [System.Serializable]
    public class CraftRequirement
    {
        public Item item;
        public int count;
    }
}
