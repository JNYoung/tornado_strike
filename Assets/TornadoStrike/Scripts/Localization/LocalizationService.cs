using System;
using System.Collections.Generic;
using UnityEngine;

namespace TornadoStrike.Localization
{
    [DisallowMultipleComponent]
    public sealed class LocalizationService : MonoBehaviour
    {
        private const string LanguagePlayerPrefsKey = "selected_language";

        public static LocalizationService Instance { get; private set; }

        public TextAsset stringTable;
        public string defaultLanguage = "zh-Hans";
        public string CurrentLanguage { get; private set; }

        public static event Action LanguageChanged;

        private readonly Dictionary<string, Dictionary<string, string>> rows = new Dictionary<string, Dictionary<string, string>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (stringTable == null)
            {
                stringTable = Resources.Load<TextAsset>("Localization/localization");
            }

            LoadTable();
            var savedLanguage = PlayerPrefs.GetString(LanguagePlayerPrefsKey, string.Empty);
            SetLanguage(string.IsNullOrWhiteSpace(savedLanguage) ? SystemLanguageToCode(Application.systemLanguage, defaultLanguage) : savedLanguage);
        }

        public void SetLanguage(string languageCode)
        {
            CurrentLanguage = string.IsNullOrWhiteSpace(languageCode) ? defaultLanguage : languageCode;
            PlayerPrefs.SetString(LanguagePlayerPrefsKey, CurrentLanguage);
            PlayerPrefs.Save();
            LanguageChanged?.Invoke();
        }

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (rows.TryGetValue(key, out var translations))
            {
                if (!string.IsNullOrEmpty(CurrentLanguage) && translations.TryGetValue(CurrentLanguage, out var localized))
                {
                    return localized;
                }

                if (translations.TryGetValue(defaultLanguage, out var fallback))
                {
                    return fallback;
                }

                if (translations.TryGetValue("en", out var english))
                {
                    return english;
                }
            }

            return key;
        }

        public string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }

        public static string SystemLanguageToCode(SystemLanguage language, string fallback)
        {
            switch (language)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    return "zh-Hans";
                case SystemLanguage.ChineseTraditional:
                    return "zh-Hant";
                case SystemLanguage.German:
                    return "de";
                case SystemLanguage.French:
                    return "fr";
                case SystemLanguage.Japanese:
                    return "ja";
                case SystemLanguage.Arabic:
                    return "ar";
                case SystemLanguage.English:
                    return "en";
                default:
                    return fallback;
            }
        }

        private void LoadTable()
        {
            rows.Clear();

            if (stringTable == null || string.IsNullOrEmpty(stringTable.text))
            {
                return;
            }

            var lines = stringTable.text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            if (lines.Length < 2)
            {
                return;
            }

            var headers = lines[0].Split('\t');
            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var columns = lines[i].Split('\t');
                if (columns.Length == 0 || string.IsNullOrWhiteSpace(columns[0]))
                {
                    continue;
                }

                var key = columns[0].Trim();
                var translations = new Dictionary<string, string>();
                for (var column = 1; column < headers.Length && column < columns.Length; column++)
                {
                    translations[headers[column].Trim()] = columns[column].Replace("\\n", "\n");
                }

                rows[key] = translations;
            }
        }
    }
}
