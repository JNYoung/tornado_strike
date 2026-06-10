using UnityEngine;
using UnityEngine.UI;

namespace TornadoStrike.Localization
{
    [RequireComponent(typeof(Text))]
    [DisallowMultipleComponent]
    public sealed class LocalizedText : MonoBehaviour
    {
        public string key;

        private Text label;

        private void Awake()
        {
            label = GetComponent<Text>();
        }

        private void OnEnable()
        {
            LocalizationService.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            LocalizationService.LanguageChanged -= Refresh;
        }

        public void Refresh()
        {
            if (label == null)
            {
                label = GetComponent<Text>();
            }

            var service = LocalizationService.Instance;
            label.text = service != null ? service.Get(key) : key;
        }
    }
}
