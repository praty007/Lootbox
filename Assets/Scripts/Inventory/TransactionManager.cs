using System;
using LootBoxes;


namespace Inventory
{
    public static class TransactionManager
    {
        public static async void BuyLootbox(LootboxTier tier, Action OnSuccess, Action<string> OnError = null)
        {
            TierData d = LootboxManager.GetTierData(tier);
            var cost = d.GetPurchaseCost();

            if (cost.currencyType == CurrencyType.Advertisement && d.CanBuy())
            {
                bool wasSuccessful = false;
                wasSuccessful = await Advertisement.Watch();
                if (!wasSuccessful)
                {
                    OnError?.Invoke("Could not run the ad");
                    return;
                }
                if (LootboxManager.BuyLootBox(tier, OnError))
                {
                    InventoryManager.OnInventoryUpdated?.Invoke();
                    OnSuccess?.Invoke();
                }
                return;
            }

            if (InventoryManager.Inventory.Currencies[cost.currencyType] >= cost.units)
            {
                if (LootboxManager.BuyLootBox(tier, OnError))
                {
                    if (InventoryManager.WithdrawCurrency(cost.currencyType, cost.units))
                    {
                        OnSuccess?.Invoke();
                        return;
                    }
                    else
                    {
                        OnError?.Invoke("Error withdrawing " + cost.currencyType.ToString());
                        return;
                    }
                }
                OnError?.Invoke("Error buying " + tier.ToString());
                return;
            }
            else
            {
                OnError?.Invoke("Not enough " + tier);
                return;
            }
        }

        public static LootPool OpenLootBox(LootboxTier tier)
        {
            LootPool d = LootboxManager.OpenLootBox(tier);
            InventoryManager.Inventory.Add(d);
            LootboxManager.OnLootboxOpened?.Invoke(d);
            InventoryManager.OnInventoryUpdated?.Invoke();

            return d;
        }
    }


}