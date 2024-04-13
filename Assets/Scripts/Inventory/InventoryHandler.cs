using UnityEngine;
using ApiHelper;
using LootBoxes;
using System;

namespace Inventory
{
    /// <summary>
    /// Monobehaviour for all Unity operation on Inventory
    /// </summary>
    public class InventoryHandler : MonoBehaviour
    {
        private InventoryHandler() { } //prevent creation of class outside this class

        private static readonly Lazy<InventoryHandler> _lazyInstance = new Lazy<InventoryHandler>(() =>
        {
            var z = new GameObject("InventoryHandler").AddComponent<InventoryHandler>();
            DontDestroyOnLoad(z);
            return z;
        });

        public static InventoryHandler Instance => _lazyInstance.Value;

        public Action OnInitialized;
        public Action OnInventoryUpdated;

        readonly string API_Inventory_Endpoint = string.Empty;
        const string playerInventoryPlayerPrefs = "PlayerInventory";

        private void Awake()
        {
            InventoryManager.OnInitialize += () =>
            {
                {
                    OnInitialized?.Invoke();
                    OnInventoryUpdated?.Invoke();
                };
            };
            InventoryManager.OnInventoryUpdated += () =>
            {
                if (LootboxManager.IsInitialized && InventoryManager.IsInitialized)
                {
                    OnInventoryUpdated?.Invoke();

                    string playerData = InventoryManager.GetInventoryJson();
                    Debug.Log("Saving to playerPrefs\n" + playerData);

                    PlayerPrefs.SetString(playerInventoryPlayerPrefs, playerData);
                }
            };
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(API_Inventory_Endpoint))
               _fetchUsingAPI();
            else if (PlayerPrefs.HasKey(playerInventoryPlayerPrefs))
            {
                Debug.Log("Initializing using PlayerPrefs data\n" + PlayerPrefs.GetString(playerInventoryPlayerPrefs));
                InventoryManager.InitializeWithJson(PlayerPrefs.GetString(playerInventoryPlayerPrefs));
            }
            else
                InventoryManager.InitializeNewUser();
        }

        void _fetchUsingAPI()
        {
            Loader.instance.Get(
                API_Inventory_Endpoint,
                s => InventoryManager.InitializeWithJson(s),
                (e) =>
                {
                    Debug.LogError("Error occurred while fetching inventory data\n" + e);
                    InventoryManager.InitializeNewUser();
                });
        }

        public bool BuyLootbox(LootboxTier tier)
        {
            TierData d = LootboxManager.GetTierData(tier);
            var cost = d.GetPurchaseCost();

            if (cost.currencyType == CurrencyType.Advertisement || InventoryManager.Inventory.Currencies[cost.currencyType] >= cost.units)
                try
                {
                    bool wasSuccessful = false;
                    TransactionManager.BuyLootbox(tier,
                        () => wasSuccessful = true,
                        (e) => Debug.LogError(e));
                    return wasSuccessful;
                }
                catch (Exception e)
                {
                    Debug.LogError("Could not buy lootbox/n" + e);
                    return false;
                }
            return false;

        }

        public LootPool OpenLootbox(LootboxTier tier)
        {
            return TransactionManager.OpenLootBox(tier);
        }

        public uint GetLootBoxCount(LootboxTier tier)
        {
            return (uint)LootboxManager.GetTierData(tier).count;
        }

        public Inventory GetInventory()
        {
            return InventoryManager.Inventory;
        }

        public void UseConsumable(Consumable c)
        {
            InventoryManager.Use(c);
        }
    }
}