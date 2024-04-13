using UnityEngine;
using Inventory;
using LootBoxes;
using UnityEngine.UI;

public class LootboxDemo : MonoBehaviour
{
    [SerializeField] Button BuyBronzeLootBox;
    [SerializeField] Button BuySilverLootBox;
    [SerializeField] Button BuyGoldLootBox;
    [Space, SerializeField] Button BronzeLootBox;
    [SerializeField] Button SilverLootBox;
    [SerializeField] Button GoldLootBox;
    [Space, SerializeField] Button HeadStart1;
    [SerializeField] Button HeadStart2;
    [Space, SerializeField] Button Powerup1;
    [SerializeField] Button Powerup2;
    [SerializeField] Button Powerup3;
    [Space, SerializeField] Button Coins;
    [SerializeField] Button Maidans;

    bool isLootboxManagerInitialized = false;
    bool isInventoryManagerInitialized = false;

    Inventory.Inventory inventory => InventoryHandler.Instance.GetInventory();

    // Start is called before the first frame update
    void Start()
    {
        InventoryHandler.Instance.OnInitialized += onInventoryHandlerInit;
        InventoryHandler.Instance.OnInventoryUpdated += OnInventoryUpdated;
        LootboxHandler.instance.OnInitialized += OnLootBoxManagerInitialized;
        LootboxHandler.instance.OnLootBoxBecameAvailable += OnLootboxBecameAvailable;
    }

    private void OnLootBoxManagerInitialized()
    {
        Debug.Log("OnLootBoxManagerInitialized");
        isLootboxManagerInitialized = true;
    }

    private void OnLootboxBecameAvailable(LootboxTier obj)
    {
        string btnText = obj.ToString() + " Available";
        Debug.Log(btnText);
        Button b = null;
        switch (obj)
        {
            case LootboxTier.Bronze:
                b = BuyBronzeLootBox;
                break;
            case LootboxTier.Silver:
                b = BuySilverLootBox;
                break;
            case LootboxTier.Gold:
                b = BuyGoldLootBox;
                break;
        }
        updateButton(b, btnText, true);
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => InventoryHandler.Instance.BuyLootbox(obj));
    }

    private void OnInventoryUpdated()                   //TODO: Make it more SOLID somehow.
    {
        Button b;
        string text;
        bool shouldBeInteractable = false;
        //Coins
        b = Coins;
        text = "Coins " + inventory.Currencies[CurrencyType.Coin];
        if (inventory.Currencies[CurrencyType.Coin] > 0)
        {
            b.onClick.RemoveAllListeners();
        }
        updateButton(b, text, shouldBeInteractable);

        //Maidans
        b = Maidans;
        text = "Maidans " + inventory.Currencies[CurrencyType.Maidan];
        if (inventory.Currencies[CurrencyType.Maidan] > 0)
        {
            b.onClick.RemoveAllListeners();
        }
        updateButton(b, text, shouldBeInteractable);

        //BronzeLB
        b = BronzeLootBox;
        text = "BronzeLootBoxes " + InventoryHandler.Instance.GetLootBoxCount(LootboxTier.Bronze);
        if (InventoryHandler.Instance.GetLootBoxCount(LootboxTier.Bronze) > 0)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => OpenLootBox(LootboxTier.Bronze));
            shouldBeInteractable = true;
        }
        updateButton(b, text, shouldBeInteractable);
        shouldBeInteractable = false;

        //SilverLB
        b = SilverLootBox;
        text = "SilverLootBoxes " + InventoryHandler.Instance.GetLootBoxCount(LootboxTier.Silver);
        if (InventoryHandler.Instance.GetLootBoxCount(LootboxTier.Silver) > 0)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => OpenLootBox(LootboxTier.Silver));
            shouldBeInteractable = true;
        }
        updateButton(b, text, shouldBeInteractable);
        shouldBeInteractable = false;

        //GoldLB
        b = GoldLootBox;
        text = "GoldLootBoxes " + InventoryHandler.Instance.GetLootBoxCount(LootboxTier.Gold);
        if (InventoryHandler.Instance.GetLootBoxCount(LootboxTier.Gold) > 0)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => OpenLootBox(LootboxTier.Gold));
            shouldBeInteractable = true;
        }
        updateButton(b, text, shouldBeInteractable);
        shouldBeInteractable = false;

        //HeadStart1
        b = HeadStart1;
        text = "1000mDash " + inventory.Headstarts[HeadstartType.ThousandMeterDash];
        if (inventory.Headstarts[HeadstartType.ThousandMeterDash] > 0)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => InventoryHandler.Instance.UseConsumable(new Headstart(HeadstartType.ThousandMeterDash)));
            shouldBeInteractable = true;

        }
        updateButton(b, text, shouldBeInteractable);
        shouldBeInteractable = false;

        //HeadStart2
        b = HeadStart2;
        text = "TwoXMultiplier " + inventory.Headstarts[HeadstartType.TwoXMultiplier];
        if (inventory.Headstarts[HeadstartType.TwoXMultiplier] > 0)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => InventoryHandler.Instance.UseConsumable(new Headstart(HeadstartType.TwoXMultiplier)));
            shouldBeInteractable = true;

        }
        updateButton(b, text, shouldBeInteractable);
        shouldBeInteractable = false;

        //Powerup1
        b = Powerup1;
        text = "Cycle " + inventory.Powerups[PowerupType.Cycle];
        if (inventory.Powerups[PowerupType.Cycle] > 0)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => InventoryHandler.Instance.UseConsumable(new Powerup(PowerupType.Cycle)));
            shouldBeInteractable = true;

        }
        updateButton(b, text, shouldBeInteractable);
        shouldBeInteractable = false;

        //Powerup2
        b = Powerup2;
        text = "Motorcycle " + inventory.Powerups[PowerupType.Motorcycle];
        if (inventory.Powerups[PowerupType.Motorcycle] > 0)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => InventoryHandler.Instance.UseConsumable(new Powerup(PowerupType.Motorcycle)));
            shouldBeInteractable = true;

        }
        updateButton(b, text, shouldBeInteractable);
        shouldBeInteractable = false;

        //Powerup3
        b = Powerup3;
        text = "Skates " + inventory.Powerups[PowerupType.Skates];
        if (inventory.Powerups[PowerupType.Skates] > 0)
        {
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => InventoryHandler.Instance.UseConsumable(new Powerup(PowerupType.Skates)));
            shouldBeInteractable = true;

        }
        updateButton(b, text, shouldBeInteractable);
        shouldBeInteractable = false;
    }

    void OpenLootBox(LootboxTier tier)
    {
        var rewards = InventoryHandler.Instance.OpenLootbox(tier);
        Debug.Log("Received rewards");
        //Display some UI elements here.
    }

    private void onInventoryHandlerInit()
    {
        Debug.Log("onInventoryHandlerInit");
        isInventoryManagerInitialized = true;
    }

    private void Update()
    {
        if (!isLootboxManagerInitialized || !isInventoryManagerInitialized) return;
        foreach (var kvp in LootboxHandler.instance.LootBoxAvailabilityTimes)
        {
            if (kvp.Value == 0) continue;

            string btnText = kvp.Key.ToString() + "\n" + (int)(kvp.Value/60) + "minutes "  + (int)kvp.Value%60 + "seconds";
            Button b = null;
            switch (kvp.Key)
            {
                case LootboxTier.Bronze:
                    b = BuyBronzeLootBox;
                    break;
                case LootboxTier.Silver:
                    b = BuySilverLootBox;
                    break;
                case LootboxTier.Gold:
                    b = BuyGoldLootBox;
                    break;
            }
            updateButton(b, btnText, false);
        }

    }

    void updateButton(Button button, string text)
    {
        button.transform.GetChild(0).GetComponent<Text>().text = text;
    }

    void updateButton(Button button, string text, bool shouldBeInteractable)
    {
        button.transform.GetChild(0).GetComponent<Text>().text = text;
        button.interactable = shouldBeInteractable;
    }

}
