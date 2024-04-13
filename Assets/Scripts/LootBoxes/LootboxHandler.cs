using UnityEngine;
using ApiHelper;
using System.Collections.Generic;
using System;
using System.Collections;

namespace LootBoxes
{
    /// <summary>
    /// Monobehaviour for handling Unity operations for Lootboxes.
    /// </summary>
    public sealed class LootboxHandler : MonoBehaviour
    {
        private LootboxHandler() { } //prevent creation of class outside this class

        private static readonly Lazy<LootboxHandler> _lazyInstance = new Lazy<LootboxHandler>(() =>
        {
            var z = new GameObject("LootboxHandler").AddComponent<LootboxHandler>();
            DontDestroyOnLoad(z);
            return z;
        });

        public static LootboxHandler instance => _lazyInstance.Value;

        readonly string API_Lootbox_Endpoint;

        public Action OnInitialized;
        public Action<LootboxTier> OnLootBoxBecameAvailable;

        public Dictionary<LootboxTier, float> LootBoxAvailabilityTimes = new Dictionary<LootboxTier, float>();

        private void Awake()
        {

            LootboxManager.OnInitialize += () =>
            {
                LootBoxAvailabilityTimes = new Dictionary<LootboxTier, float>();
                OnInitialized?.Invoke();
                foreach (var s in Enum.GetNames(typeof(LootboxTier)))
                {
                    var ss = (LootboxTier)Enum.Parse(typeof(LootboxTier), s);
                    StartCoroutine(CheckIfCanBuy(ss));
                }
            };
            LootboxManager.OnBoxBought += tier => StartCoroutine(CheckIfCanBuy(tier));

        }

        public IEnumerator CheckIfCanBuy(LootboxTier tier, float timeToWait = 0)
        {
            float t = timeToWait;
            while (t > 0)
            {
                LootBoxAvailabilityTimes[tier] = t--;
                yield return new WaitForSeconds(1);
            }

            var TierData = LootboxManager.GetTierData(tier);
            bool canBuy = TierData.CanBuy((e) => Debug.LogError(e), (ts) =>
            {
                LootBoxAvailabilityTimes[tier] = (float)ts.TotalSeconds;
                StartCoroutine(CheckIfCanBuy(tier, (float)ts.TotalSeconds));
            });

            if (canBuy)
            {
                OnLootBoxBecameAvailable?.Invoke(tier);
            }
        }

        public void InitializeUsingData(Dictionary<LootboxTier, TierData> data) => LootboxManager.InitializeWithData(data);


    }



}