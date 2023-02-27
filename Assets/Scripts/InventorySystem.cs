using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemListText;
    [SerializeField] private TextMeshProUGUI eventLogText;
    [SerializeField] private TextMeshProUGUI weaponText;
    [SerializeField] private TextMeshProUGUI armourText;
    [SerializeField] private TextMeshProUGUI toolsText;
    [SerializeField] private CanvasGroup craftMenu;
    [SerializeField] private Transform craftContent;
    [SerializeField] private RectTransform craftEntryPrefab;
    [SerializeField] private CanvasGroup eventLogGroup;
    [SerializeField] private float eventLogFadeDelay = 3f;
    [SerializeField] private float eventLogFadeTime = 2f;
    [SerializeField] private Item[] craftableItems;

    public Item Weapon { get; private set; }
    public Item Armour { get; private set; }
    public List<Item> Tools { get; private set; }
    public Dictionary<Item, int> Inventory { get; private set; }

    private List<Button> craftEntries;

    private Coroutine eventLogFadeCR;

    private void Awake()
    {
        Tools = new List<Item>();
        Inventory = new Dictionary<Item, int>();
        craftEntries = new List<Button>();

        int itemCount = 0;

        foreach (Item item in craftableItems)
        {
            RectTransform craftEntry = Instantiate(craftEntryPrefab, craftContent);
            craftEntry.anchoredPosition += Vector2.down * 30 * itemCount;
            craftEntry.GetComponent<CraftEntry>().Item = item;

            string buyText = item.displayName + " (";

            for (int i = 0; i < item.craftRequirements.Length; i++)
            {
                Item.CraftRequirement req = item.craftRequirements[i];

                buyText += req.count + " " + req.item.displayName;

                if (i < item.craftRequirements.Length - 1)
                {
                    buyText += ", ";
                }
            }

            buyText += ")";

            craftEntry.GetComponentInChildren<TextMeshProUGUI>().text = buyText;

            craftEntries.Add(craftEntry.GetComponent<Button>());

            itemCount++;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            craftMenu.alpha = 1 - craftMenu.alpha;
            craftMenu.interactable = !craftMenu.interactable;
            craftMenu.blocksRaycasts = !craftMenu.blocksRaycasts;

            UpdateCraftables();
        }
    }

    public void AddItem(Item item, int count)
    {
        switch (item.itemType)
        {
            case ItemType.Basic:
                {
                    int totalCount = count;

                    if (Inventory.ContainsKey(item))
                    {
                        totalCount += Inventory[item];
                    }

                    Inventory[item] = totalCount;

                    UpdateItemList();

                    UpdateCraftables();
                }
                break;
            case ItemType.Weapon:
                {
                    Weapon = item;

                    weaponText.text = "Weapon\n" + item.displayName;
                }
                break;
            case ItemType.Armour:
                {
                    Armour = item;

                    armourText.text = "Armour\n" + item.displayName;
                }
                break;
            case ItemType.Tool:
                {
                    Tools.Add(item);

                    toolsText.text += "\n" + item.displayName;
                }
                break;
        }


        AddItemEvent(item, count);
    }

    public void BuyItem(Item item)
    {
        foreach (Item.CraftRequirement req in item.craftRequirements)
        {
            AddItem(req.item, -req.count);
        }

        AddItem(item, 1);
    }

    public bool HasTool(Item item)
    {
        return Tools.Contains(item);
    }

    private void UpdateCraftables()
    {
        int itemCount = 0;

        foreach (Item item in craftableItems)
        {
            craftEntries[itemCount].interactable = CanCraft(item);
            itemCount++;
        }
    }

    private bool CanCraft(Item item)
    {
        bool canCraft = true;

        foreach (Item.CraftRequirement req in item.craftRequirements)
        {
            bool reqPassed = false;

            foreach (Item invItem in Inventory.Keys)
            {
                if (req.item == invItem && Inventory[invItem] >= req.count)
                {
                    reqPassed = true;
                    break;
                }
            }

            if (!reqPassed)
            {
                canCraft = false;
                break;
            }
        }

        return canCraft;
    }

    private void UpdateItemList()
    {
        string itemListStr = "";

        foreach (Item item in Inventory.Keys)
        {
            itemListStr += " - " + item.displayName + " (" + Inventory[item] + ")\n";
        }

        itemListText.text = itemListStr;
    }

    private void AddItemEvent(Item item, int count)
    {
        string acquireText = count >= 0 ? "acquired" : "lost";
        AddEventLog("You " + acquireText + " " + item.displayName + " (" + count + ")");
    }

    public void AddEventLog(string message)
    {
        eventLogText.text = message + "\n" + eventLogText.text;
        eventLogGroup.alpha = 1f;

        if (eventLogFadeCR != null) StopCoroutine(eventLogFadeCR);
        eventLogFadeCR = StartCoroutine(FadeEventLog());
    }

    private IEnumerator FadeEventLog()
    {
        yield return new WaitForSeconds(eventLogFadeDelay);

        float time = 0;

        while (time < eventLogFadeTime)
        {
            eventLogGroup.alpha = Mathf.Lerp(1f, 0f, time / eventLogFadeTime);
            time += Time.deltaTime;
            yield return null;
        }

        eventLogGroup.alpha = 0f;
    }
}
