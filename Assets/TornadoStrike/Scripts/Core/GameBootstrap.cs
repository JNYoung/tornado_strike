using TornadoStrike.Localization;
using UnityEngine;

namespace TornadoStrike.Core
{
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        public int targetFrameRate = 60;
        public string fallbackLanguage = "zh-Hans";

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;

            EnsureService<AdService>("AdService");
            EnsureService<PrivacyConsentService>("PrivacyConsentService");

            var localization = LocalizationService.Instance;
            if (localization != null && string.IsNullOrEmpty(localization.CurrentLanguage))
            {
                localization.SetLanguage(LocalizationService.SystemLanguageToCode(Application.systemLanguage, fallbackLanguage));
            }
        }

        private static void EnsureService<T>(string name) where T : Component
        {
            if (FindObjectOfType<T>() != null)
            {
                return;
            }

            var serviceObject = new GameObject(name);
            serviceObject.AddComponent<T>();
        }
    }
}
