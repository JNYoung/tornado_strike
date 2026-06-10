using System;
using UnityEngine;

namespace TornadoStrike.Core
{
    [DisallowMultipleComponent]
    public sealed class AdService : MonoBehaviour
    {
        public static AdService Instance { get; private set; }

        [Header("MVP Simulation")]
        public bool simulateAds = true;
        public bool rewardedAdsAvailable = true;
        public bool interstitialAdsAvailable = true;

        public event Action<string> RewardedAdCompleted;
        public event Action<string> InterstitialAdClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool CanShowRewardedAd()
        {
            return simulateAds && rewardedAdsAvailable;
        }

        public bool CanShowInterstitialAd()
        {
            return simulateAds && interstitialAdsAvailable;
        }

        public void ShowRewardedAd(string placement)
        {
            if (!CanShowRewardedAd())
            {
                Debug.LogWarning($"Rewarded ad unavailable for placement '{placement}'.");
                return;
            }

            Debug.Log($"[Ads MVP] Rewarded ad completed: {placement}");
            RewardedAdCompleted?.Invoke(placement);
        }

        public void ShowInterstitialAd(string placement)
        {
            if (!CanShowInterstitialAd())
            {
                Debug.LogWarning($"Interstitial ad unavailable for placement '{placement}'.");
                return;
            }

            Debug.Log($"[Ads MVP] Interstitial ad closed: {placement}");
            InterstitialAdClosed?.Invoke(placement);
        }
    }
}
