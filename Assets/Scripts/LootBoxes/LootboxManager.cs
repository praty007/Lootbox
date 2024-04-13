using System;
using System.Collections.Generic;
using UnityEngine;
using Inventory;
using utils;
using Newtonsoft.Json;

namespace LootBoxes
{
    /// <summary>
    /// All data based operations for lootbox are to be done here.
    /// 
    /// </summary>
    internal static class LootboxManager
    {
        /// <summary>
        /// Whether the LootboxManager has been initialized with data.
        /// </summary>
        public static bool IsInitialized
        {
            get => _isInitialized;
            private set
            {
                OnInitialize?.Invoke();
                _isInitialized = value;
            }
        }

        /// <summary>
        /// Do not modify directly. Use <see cref="IsInitialized"/> instead.
        /// </summary>
        private static bool _isInitialized = false;

        /// <summary>
        /// Subscribe to this event to be notified when the lootbox manager is initialized with data.
        /// </summary>
        public static Action OnInitialize;

        public static Action<LootPool> OnLootboxOpened;

        /// <summary>
        /// Everytime a box is bought, this event is fired.
        /// </summary>
        public static Action<LootboxTier> OnBoxBought;

        static Dictionary<LootboxTier, TierData> TierDatas;

        /// <summary>
        /// Use it to initialize using user's data from the server.
        /// </summary>
        /// <param name="jsonData"></param>
        internal static void InitializeWithJson(string jsonData)
        {
            try
            {
                TierDatas = JsonConvert.DeserializeObject<Dictionary<LootboxTier, TierData>>(jsonData);
                foreach (var td in TierDatas)
                    td.Value.SetDefaultDateTimeIfNull();
                IsInitialized = true;
                Debug.Log("Initialized Lootbox Data for existing user using JSON");
            }
            catch
            {
                Debug.LogError("Cound not initialize Tier Data for the received json document.");
            }
        }

        /// <summary>
        /// Use it to initialize data for a new user.
        /// </summary>
        internal static void InitializeWithData(Dictionary<LootboxTier, TierData> data)
        {
            TierDatas = data;
            foreach (var td in TierDatas)
            {
                td.Value.SetDefaultDateTimeIfNull();
            }
            Debug.Log("Initialized Lootbox Data using TierData Dictionary");
            IsInitialized = true;
        }

        /// <summary>
        /// Get TierData pertaining to a particular tier
        /// </summary>
        /// <param name="tier">tier to get the TierData for</param>
        /// <returns></returns>
        public static TierData GetTierData(LootboxTier tier)
        {
            return TierDatas[tier];
        }

        public static bool BuyLootBox(LootboxTier lootboxTier, Action<string> onFailure = null)
        {
            var val = GetTierData(lootboxTier).Buy(onFailure);
            if(val) OnBoxBought?.Invoke(lootboxTier);
            return val;
        }

        public static bool SpeedUpLootBoxTimer(LootboxTier lootboxTier)
        {
            var obj = GetTierData(lootboxTier).SpeedUp();
            return obj;
        }

        public static LootPool OpenLootBox(LootboxTier tier)
        {
            TierData tierData = GetTierData(tier);
            return tierData.Open();
        }
    }

    /// <summary>
    /// Struct to hold all data for a lootbox tier.
    /// The static functions can be used to get tier specific data.
    /// Each instance holds data for a lootbox level such as count,
    /// last purchased, last spawned, etc.
    /// </summary>
    [Serializable]
    public sealed class TierData
    {
        public LootboxTier Type;
        public int count;
        public string lastSpawnedUTC;
        public string lastBoughtUTC;


        public TierData(LootboxTier mTier, int mCount = 0, string mLastSpawnedUTC = null, string mLastBoughtUTC = null)
        {
            Type = mTier;
            count = mCount;
            if(string.IsNullOrEmpty(mLastSpawnedUTC))
                lastSpawnedUTC = Const.GetDateTimeString(DateTime.MinValue);
            else
                lastSpawnedUTC = mLastSpawnedUTC;
            if (string.IsNullOrEmpty(mLastBoughtUTC))
                lastBoughtUTC = Const.GetDateTimeString(DateTime.MinValue);
            else
                lastBoughtUTC = mLastBoughtUTC;
        }

        public Cost GetPurchaseCost()
        {
            switch (Type)
            {
                case LootboxTier.Bronze: return new Cost(CurrencyType.Advertisement, 1);
                case LootboxTier.Silver: return new Cost(CurrencyType.Coin, 5000);
                case LootboxTier.Gold: return new Cost(CurrencyType.Maidan, 15);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public uint GetStorageLimit()
        {
            switch (Type)
            {
                case LootboxTier.Bronze: return uint.MaxValue;
                case LootboxTier.Silver: return 3;
                case LootboxTier.Gold: return uint.MaxValue;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public Cost GetCooldownSpeedupCost()
        {
            switch (Type)
            {
                case LootboxTier.Bronze: return new Cost(CurrencyType.Coin, 0);
                case LootboxTier.Silver: return new Cost(CurrencyType.Coin, 25);
                case LootboxTier.Gold: return new Cost(CurrencyType.Coin, 0);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public TimeSpan GetCoolDownTime()
        {
            switch (Type)
            {
                case LootboxTier.Bronze: return new TimeSpan(6, 0, 0);
                case LootboxTier.Silver: return new TimeSpan(3, 0, 0);
                case LootboxTier.Gold: return new TimeSpan(0, 0, 0);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public bool CanBuy(Action<string> onError=null, Action<TimeSpan> onTimeSpan = null)
        {
            var timeDiff = DateTime.UtcNow - Const.GetDateTime(lastBoughtUTC);
            if (timeDiff < GetCoolDownTime())
            {
                TimeSpan timeSpan = GetCoolDownTime() - timeDiff;
                onError?.Invoke("Cannot buy " + Type + " lootbox yet. Please try again after " + timeSpan.Hours + " hours " + timeSpan.Minutes + " minutes " + timeSpan.Seconds + " seconds");
                onTimeSpan?.Invoke(timeSpan);
                return false;
            } else if(GetStorageLimit()<=count)
            {
                onError?.Invoke("Inventory full. Please open a few boxes first");
                return false;
            }
            return true;
        }

        public bool Buy(Action<string> onError = null)
        {
            if (CanBuy(onError))
            {
                lastBoughtUTC = Const.GetDateTimeString(DateTime.UtcNow);
                Debug.Log("Bought a lootbox "+ Type);
                count += 1;
                return true;
            } return false;
        }

        public bool SpeedUp()
        {
            if (DateTime.UtcNow - Const.GetDateTime(lastBoughtUTC) > GetCoolDownTime())
                return false;
            else
            {
                lastBoughtUTC = Const.GetDateTimeString(DateTime.MinValue.ToUniversalTime());
                return true;
            }
        }

        public LootPool Open()
        {
            if (count > 0)
            {
                count -= 1;
                return LootPool.Generate(Type);
            }
            throw new Exception("Not Enough Lootboxes in your inventory");
        }

        public void SetDefaultDateTimeIfNull()
        {
            if (string.IsNullOrEmpty(lastSpawnedUTC))
                lastSpawnedUTC = Const.GetDateTimeString(DateTime.MinValue);
            if (string.IsNullOrEmpty(lastBoughtUTC))
                lastBoughtUTC = Const.GetDateTimeString(DateTime.MinValue);
        }
    }

    public struct LootPool
    {
        public Dictionary<HeadstartType, uint> headstarts;
        public Dictionary<PowerupType, uint> powerups;
        public Dictionary<CurrencyType, uint> currencies;

        internal static LootPool Generate(LootboxTier tier)                           //TODO: Create readonly vars to fetch probabilities from API
        {
            LootPool lp = new LootPool();
            lp.headstarts = new Dictionary<HeadstartType, uint>();
            lp.powerups = new Dictionary<PowerupType, uint>();
            lp.currencies = new Dictionary<CurrencyType, uint>();
            switch (tier)                                                           //TODO: Make it Generic
            {
                case LootboxTier.Bronze:
                    {
                        float prob = Rand.Float();
                        if (prob < .4f)
                            lp.headstarts.Add(HeadstartType.ThousandMeterDash, Rand.Int(1, 2));
                        else if (prob < .7f)
                            lp.headstarts.Add(HeadstartType.TwoXMultiplier, Rand.Int(1, 2));
                        else
                            lp.powerups.Add((PowerupType)(Rand.Int(0, 48) % 3), Rand.Int(1, 3));

                        lp.currencies.Add(CurrencyType.Coin, Rand.Int(100, 1000));

                        return lp;
                    }
                case LootboxTier.Silver:
                    {
                        if (Rand.Float() < 50)
                        {
                            lp.currencies.Add(CurrencyType.Maidan, Rand.Int(5, 10));
                            float prob = Rand.Float(0, .5f);
                            if (prob < .25f)
                                lp.headstarts.Add(HeadstartType.ThousandMeterDash, Rand.Int(2, 5));
                            else if (prob < .35f)
                                lp.headstarts.Add(HeadstartType.TwoXMultiplier, Rand.Int(2, 5));
                            else
                                lp.powerups.Add((PowerupType)(Rand.Int(0, 48) % 3), Rand.Int(3, 5));
                        }
                        else
                        {
                            if (Rand.Float(0f, .35f) < .25f)
                                lp.headstarts.Add(HeadstartType.ThousandMeterDash, Rand.Int(2, 5));
                            else
                                lp.headstarts.Add(HeadstartType.TwoXMultiplier, Rand.Int(2, 5));

                            lp.powerups.Add((PowerupType)(Rand.Int(0, 48) % 3), Rand.Int(3, 5));
                        }

                        lp.currencies.Add(CurrencyType.Coin, Rand.Int(1000, 5000));

                        return lp;
                    }
                case LootboxTier.Gold:
                    {
                        lp.headstarts.Add((HeadstartType)(Rand.Int(0, 50) % 2), Rand.Int(5, 7));
                        lp.powerups.Add((PowerupType)(Rand.Int(0, 48) % 3), Rand.Int(5, 8));
                        lp.currencies.Add(CurrencyType.Coin, Rand.Int(5000, 10000));
                        lp.currencies.Add(CurrencyType.Maidan, Rand.Int(15, 20));
                        return lp;
                    }
                default: throw new ArgumentOutOfRangeException();
            }
        }


    }

    public enum LootboxTier
    {
        Bronze = 0,
        Silver = 1,
        Gold = 2
    }

   
   
}