using UnityEngine;

namespace TornadoStrike.Core
{
    [DisallowMultipleComponent]
    public sealed class PrivacyConsentService : MonoBehaviour
    {
        public static PrivacyConsentService Instance { get; private set; }

        public const string ConsentPlayerPrefsKey = "privacy_consent_v1";

        [Header("Policy")]
        public string privacyPolicyUrl = "https://github.com/JNYoung/tornado_strike";

        public bool HasConsent => PlayerPrefs.GetInt(ConsentPlayerPrefsKey, 0) == 1;

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

        public void Accept()
        {
            PlayerPrefs.SetInt(ConsentPlayerPrefsKey, 1);
            PlayerPrefs.Save();
        }

        public void OpenPrivacyPolicy()
        {
            if (!string.IsNullOrWhiteSpace(privacyPolicyUrl))
            {
                Application.OpenURL(privacyPolicyUrl);
            }
        }
    }
}
