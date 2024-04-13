using System;
using System.Collections.Generic;
using utils;
using LootBoxes;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
namespace Inventory
{
    /// <summary>
    /// Manages all Inventory operations.
    /// </summary>
    public static class InventoryManager
    {
        /// <summary>
        /// Whether the InventoryManager has been initialized
        /// </summary>
        public static bool IsInitialized
        {
            get => _isInitialized;
            private set
            {
                if (value)
                {
                    LootboxManager.InitializeWithData(Inventory.Lootboxes);
                    OnInitialize?.Invoke();
                }
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
        public static Action OnInventoryUpdated;

        /// <summary>
        /// User's current inventoy
        /// </summary>
        public static Inventory Inventory { get; private set; }

        /// <summary>
        /// Use it to initialize using user's Inventory json from the server.
        /// </summary>
        /// <param name="jsonData"></param>
        public static void InitializeWithJson(string inventoryJson)
        {
            try
            {
                Inventory = JsonConvert.DeserializeObject<Inventory>(inventoryJson);

                _ = new Coin(Inventory.Currencies[CurrencyType.Coin]);
                _ = new Maidan(Inventory.Currencies[CurrencyType.Maidan]);

                Debug.Log("Initialized Inventory Data for existing user");
                IsInitialized = true;
                OnInventoryUpdated?.Invoke();
            }
            catch
            {
                Debug.LogError("Cound not initialize Tier Data for the received json document.");
            }
        }

        /// <summary>
        /// Initializes new Inventory with default values, for a new user.
        /// </summary>
        public static void InitializeNewUser(string userID = null)
        {
            if (string.IsNullOrEmpty(userID))
                Inventory = Inventory.InitializeNewInventory(Rand.String(10));
            else 
                Inventory = Inventory.InitializeNewInventory(userID);

            _ = new Coin(Inventory.Currencies[CurrencyType.Coin]);
            _ = new Maidan(Inventory.Currencies[CurrencyType.Maidan]);
            Debug.Log("GetJSON\n" + GetInventoryJson());
            IsInitialized = true;

            OnInventoryUpdated?.Invoke();
        }

        public static string GetInventoryJson()
        {
            return JsonConvert.SerializeObject(Inventory);
        }


        public static bool AddCurrency(CurrencyType type, uint units)
        {
            if (type == CurrencyType.Coin)
            {
                if (uint.MaxValue - Coin.Wallet.Count > units)
                {
                    Coin.Wallet.Add(units);
                    Inventory.Currencies[CurrencyType.Coin] = Coin.Wallet.Count;
                    OnInventoryUpdated?.Invoke();
                    return true;
                }
                return false;

            }
            else if (type == CurrencyType.Maidan)
            {
                if (uint.MaxValue - Maidan.Wallet.Count > units)
                {
                    Maidan.Wallet.Add(units);
                    Inventory.Currencies[CurrencyType.Maidan] = Maidan.Wallet.Count;
                    OnInventoryUpdated?.Invoke();
                    return true;
                }
                return false;
            }
            throw new NotImplementedException("Advertisements have not yet been implemented.");
        }


        public static bool WithdrawCurrency(CurrencyType type, uint units)
        {
            if (type == CurrencyType.Coin)
            {
                if (Coin.Wallet.Count >= units)
                {
                    Coin.Wallet.Spend(units);
                    Inventory.Currencies[CurrencyType.Coin] = Coin.Wallet.Count;
                    OnInventoryUpdated?.Invoke();
                    return true;
                }
                return false;

            }
            else if (type == CurrencyType.Maidan)
            {
                if (Maidan.Wallet.Count >= units)
                {
                    Maidan.Wallet.Spend(units);
                    Inventory.Currencies[CurrencyType.Maidan] = Maidan.Wallet.Count;
                    OnInventoryUpdated?.Invoke();
                    return true;
                }
                return false;
            }
            throw new InvalidOperationException();
        }

        public static bool Use(Consumable consumable)
        {
            if (Inventory.Use(consumable)){
                OnInventoryUpdated?.Invoke();

                return true;
            }
            return false;
        }
    }


    [Serializable]
    public class Inventory
    {
        public readonly string UserID;
        public Dictionary<HeadstartType, uint> Headstarts;
        public Dictionary<PowerupType, uint> Powerups;
        public Dictionary<CurrencyType, uint> Currencies;
        public Dictionary<LootboxTier, TierData> Lootboxes;

        public Inventory(string userID)
        {
            UserID = userID;
            Headstarts = null;
            Powerups = null;
            Currencies = null;
            Lootboxes = null;
        }

        /// <summary>
        /// Initializes a new Inventory for a new user
        /// </summary>
        /// <param name="userID">Use <see cref="GenerateRandomString(uint, string)"/> to generate a random string for UesrID</param>
        /// <returns></returns>
        internal static Inventory InitializeNewInventory(string userID)
        {
            var newInv = new Inventory(userID);
            newInv.Headstarts = new Dictionary<HeadstartType, uint>();
            foreach(var typeString in Enum.GetNames(typeof(HeadstartType)))
            {
                if (Enum.TryParse(typeString, out HeadstartType type))
                    newInv.Headstarts.Add(type, 0);
            }
            newInv.Powerups = new Dictionary<PowerupType, uint>();
            foreach (var typeString in Enum.GetNames(typeof(PowerupType)))
            {
                if (Enum.TryParse(typeString, out PowerupType type))
                    newInv.Powerups.Add(type, 0);
            }
            newInv.Currencies = new Dictionary<CurrencyType, uint>
            {
                { CurrencyType.Coin, 10000 },
                { CurrencyType.Maidan, 45 }
            };

            newInv.Lootboxes = new Dictionary<LootboxTier, TierData>();
            foreach (var typeString in Enum.GetNames(typeof(LootboxTier)))
            {
                if (Enum.TryParse(typeString, out LootboxTier type))
                    newInv.Lootboxes.Add(type, new TierData(type));
            }
            return newInv;
        }

        public void Add(LootPool lootPool)
        {
            foreach(var c in lootPool.currencies)
            {
                InventoryManager.AddCurrency(c.Key, c.Value);   
            }
            foreach(var c in lootPool.headstarts)
            {
                Headstarts[c.Key] += c.Value;
            }
            foreach(var c in lootPool.powerups)
            {
                Powerups[c.Key] += c.Value;
            }
        }

        public bool Use(Consumable consumable)
        {
            switch (consumable.ConsumableType)
            {
                case ConsumableType.Headstarts:
                    {
                        Headstart headstart = (Headstart)consumable;
                        if (Headstarts[headstart.Type] > 0)
                        {
                            Headstarts[headstart.Type] -= 1;
                            consumable.Activate();
                            return true;
                        }
                        return false;
                    }
                case ConsumableType.Powerups:
                    {
                        Powerup powerup = (Powerup)consumable;
                        if (Powerups[powerup.Type] > 0)
                        {
                            Powerups[powerup.Type] -= 1;
                            consumable.Activate();
                            return true;
                        }
                        return false;
                    }
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }

    public interface Consumable
    {
        public ConsumableType ConsumableType { get; }
        public void Activate();
    }

    public class Headstart : Consumable
    {
        ConsumableType Consumable.ConsumableType => ConsumableType.Headstarts;

        private HeadstartType _type;
        public HeadstartType Type { get => _type; private set => _type = value; }

        public Headstart(HeadstartType mType)
        {
            this.Type = mType;
        }
        public void Activate()
        {
            Debug.Log("Used consumable headstart " + _type);
        }
    }

    public class Powerup : Consumable
    {
        ConsumableType Consumable.ConsumableType => _consumableType;
        private readonly ConsumableType _consumableType;

        private PowerupType _type;
        public PowerupType Type { get => _type; private set => _type = value; }

        public Powerup(PowerupType mType)
        {
            _consumableType = ConsumableType.Powerups;
            this.Type = mType;
        }
        public void Activate()
        {
            Debug.Log("Used consumable powerup " + _type);
        }
    }

    public enum ConsumableType
    {
        Headstarts,
        Powerups
    }

    public enum HeadstartType
    {
        ThousandMeterDash,
        TwoXMultiplier
    }

    public enum PowerupType
    {
        Cycle,
        Skates,
        Motorcycle
    }

    [Serializable]
    public struct Cost
    {
        public CurrencyType currencyType { get; private set; }
        public uint units { get; private set; }

        public Cost(CurrencyType mCurrencyType, uint mUnits)
        {
            this.currencyType = mCurrencyType;
            this.units = mUnits;
        }
    }

    public interface ICurrency           //TODO: Refactor for better usability
    {
        public CurrencyType Type { get; }

        public uint Count { get; }

        public void Add(uint units);
        public void Spend(uint units);
    }

    public class Coin : ICurrency
    {
        public static Coin Wallet;                          //Static instance

        public CurrencyType Type => CurrencyType.Coin;
        public uint Count { get => count; }
        private static uint count;

        public Coin(uint units)
        {
            if (Wallet == null)
            {
                Wallet = this;
                count = units;
            }
            else throw new Exception("Cannot instantiate new Coin class as there is already instance of Coin.Wallet.");
        }

        public void Add(uint units)
        {
            count += units;
        }

        public void Spend(uint units)
        {
            count -= units;
        }

        ~Coin()
        {
            count = 0;
            Wallet = null;
        }
    }

    public class Maidan : ICurrency
    {
        public static Maidan Wallet;

        public CurrencyType Type => CurrencyType.Maidan;
        public uint Count { get => count; }
        private static uint count;

        public Maidan(uint units)
        {
            if (Wallet == null)
            {
                Wallet = this;
                count = units;
            }
            else throw new Exception("Cannot instantiate new Maidan class as there is already instance of Maidan.Wallet.");
        }

        public void Add(uint units)
        {
            count += units;
        }

        public void Spend(uint units)
        {
            count -= units;
        }

        ~Maidan()
        {
            count = 0;
            Wallet = null;
        }
    }

    public class Advertisement : ICurrency       //TODO: Derive from monobehaviour?
    {
        public CurrencyType Type => CurrencyType.Advertisement;
        public uint Count { get => 0; }
        public static uint count { get; private set; }

        public void Add(uint units)
        {
            //TODO: Implement Async method to watch an ad.
            throw new NotImplementedException();
        }

        public void Spend(uint units)
        {
            //TODO: Right after the ad is watched, spend it to get the lootbox.
            throw new NotImplementedException();

        }

        public static async Task<bool> Watch()
        {
            await Task.Delay(1000); //Run an Ad here instead
            Debug.Log("Watched an ad");
            return true;
        }
    }

    public enum CurrencyType
    {
        Advertisement,
        Coin,
        Maidan
    }

}