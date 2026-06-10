using TornadoStrike.Core;
using TornadoStrike.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TornadoStrike.UI
{
    [DisallowMultipleComponent]
    public sealed class MainMenuController : MonoBehaviour
    {
        public string gameSceneName = "City_MVP";
        public Button playButton;
        public Button languageButton;
        public Button privacyButton;
        public Button acceptPrivacyButton;
        public Button rewardAdButton;
        public Button quitButton;
        public GameObject privacyPanel;
        public Text adStatusText;

        private readonly string[] languages = { "zh-Hans", "zh-Hant", "en", "de", "fr", "ja", "ar" };
        private int languageIndex;

        private void Awake()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(Play);
            }

            if (languageButton != null)
            {
                languageButton.onClick.AddListener(CycleLanguage);
            }

            if (privacyButton != null)
            {
                privacyButton.onClick.AddListener(ShowPrivacy);
            }

            if (acceptPrivacyButton != null)
            {
                acceptPrivacyButton.onClick.AddListener(AcceptPrivacy);
            }

            if (rewardAdButton != null)
            {
                rewardAdButton.onClick.AddListener(PreviewRewardedAd);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(Quit);
            }
        }

        private void Start()
        {
            var service = LocalizationService.Instance;
            if (service != null)
            {
                for (var i = 0; i < languages.Length; i++)
                {
                    if (languages[i] == service.CurrentLanguage)
                    {
                        languageIndex = i;
                        break;
                    }
                }
            }

            RefreshPrivacyPanel();
            RefreshAdStatus("ad_status_ready");
        }

        public void Play()
        {
            var privacy = PrivacyConsentService.Instance;
            if (privacy != null && !privacy.HasConsent)
            {
                ShowPrivacy();
                return;
            }

            SceneManager.LoadScene(gameSceneName);
        }

        public void CycleLanguage()
        {
            languageIndex = (languageIndex + 1) % languages.Length;
            var service = LocalizationService.Instance;
            if (service != null)
            {
                service.SetLanguage(languages[languageIndex]);
            }
        }

        public void ShowPrivacy()
        {
            if (privacyPanel != null)
            {
                privacyPanel.SetActive(true);
            }
        }

        public void AcceptPrivacy()
        {
            var privacy = PrivacyConsentService.Instance;
            if (privacy != null)
            {
                privacy.Accept();
            }

            RefreshPrivacyPanel();
        }

        public void PreviewRewardedAd()
        {
            var ads = AdService.Instance;
            if (ads == null || !ads.CanShowRewardedAd())
            {
                RefreshAdStatus("ad_status_unavailable");
                return;
            }

            ads.ShowRewardedAd("main_menu_booster_preview");
            RefreshAdStatus("ad_status_rewarded");
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void RefreshPrivacyPanel()
        {
            if (privacyPanel == null)
            {
                return;
            }

            var privacy = PrivacyConsentService.Instance;
            privacyPanel.SetActive(privacy != null && !privacy.HasConsent);
        }

        private void RefreshAdStatus(string key)
        {
            if (adStatusText == null)
            {
                return;
            }

            var service = LocalizationService.Instance;
            adStatusText.text = service != null ? service.Get(key) : key;
        }
    }
}
