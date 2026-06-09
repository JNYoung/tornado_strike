using TornadoStrike.Core;
using TornadoStrike.Localization;
using TornadoStrike.UI;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TornadoStrike.Editor
{
    public static class BrandingAndMenuGenerator
    {
        private const string AppIconPath = "Assets/TornadoStrike/Art/Branding/app_icon_tornado_strike.png";
        private const string SplashImagePath = "Assets/TornadoStrike/Art/Branding/splash_city_tornado.png";
        private const string LocalizationPath = "Assets/TornadoStrike/Resources/Localization/localization.txt";
        private const string SplashScenePath = "Assets/TornadoStrike/Scenes/Splash.unity";
        private const string MainMenuScenePath = "Assets/TornadoStrike/Scenes/MainMenu.unity";
        private const string CityScenePath = "Assets/TornadoStrike/Scenes/City_MVP.unity";

        [MenuItem("Tornado Strike/Generate Branding and Menu Scenes")]
        public static void GenerateBrandingAndMenuScenes()
        {
            PrepareBrandingTextures();
            ConfigureAppIcon();
            GenerateSplashScene();
            GenerateMainMenuScene();
            ConfigureBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void GenerateBrandingAndMenuScenesBatch()
        {
            GenerateBrandingAndMenuScenes();
        }

        private static void PrepareBrandingTextures()
        {
            ConfigureSpriteImport(AppIconPath, 1024, TextureImporterCompression.Uncompressed);
            ConfigureSpriteImport(SplashImagePath, 2048, TextureImporterCompression.CompressedHQ);
        }

        private static void ConfigureSpriteImport(string path, int maxSize, TextureImporterCompression compression)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = false;
            importer.maxTextureSize = maxSize;
            importer.textureCompression = compression;
            ConfigurePlatformTexture(importer, "Android", maxSize, compression);
            ConfigurePlatformTexture(importer, "iPhone", maxSize, compression);
            importer.SaveAndReimport();
        }

        private static void ConfigurePlatformTexture(TextureImporter importer, string platform, int maxSize, TextureImporterCompression compression)
        {
            var settings = importer.GetPlatformTextureSettings(platform);
            settings.overridden = true;
            settings.maxTextureSize = maxSize;
            settings.textureCompression = compression;
            importer.SetPlatformTextureSettings(settings);
        }

        private static void ConfigureAppIcon()
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconPath);
            if (icon == null)
            {
                Debug.LogWarning($"App icon missing at {AppIconPath}");
                return;
            }

            var icons = new[] { icon };
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, icons);
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, icons);

            ConfigureAndroidPlatformIcons(icon, AndroidPlatformIconKind.Legacy);
            ConfigureAndroidPlatformIcons(icon, AndroidPlatformIconKind.Round);
            ConfigureAndroidPlatformIcons(icon, AndroidPlatformIconKind.Adaptive);
        }

        private static void ConfigureAndroidPlatformIcons(Texture2D icon, PlatformIconKind kind)
        {
            var slots = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, kind);
            for (var i = 0; i < slots.Length; i++)
            {
                for (var layer = 0; layer < slots[i].maxLayerCount; layer++)
                {
                    slots[i].SetTexture(icon, layer);
                }
            }

            PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, kind, slots);
        }

        private static void GenerateSplashScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var canvas = CreateCanvas("Splash_Canvas");
            var background = CreateBackground(canvas.transform);

            var overlay = CreatePanel("FadeOverlay", canvas.transform, Color.black);
            var fade = overlay.AddComponent<CanvasGroup>();
            overlay.GetComponent<Image>().raycastTarget = false;

            CreateSystems();

            var title = CreateText("Title", canvas.transform, "game_title", 58, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.79f), new Vector2(820f, 120f));
            title.gameObject.AddComponent<LocalizedText>().key = "game_title";

            var loading = CreateText("Loading", canvas.transform, "splash_loading", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.18f), new Vector2(720f, 60f));
            loading.gameObject.AddComponent<LocalizedText>().key = "splash_loading";

            var controller = background.AddComponent<SplashController>();
            controller.fadeGroup = fade;
            controller.nextSceneName = "MainMenu";

            EditorSceneManager.SaveScene(scene, SplashScenePath);
        }

        private static void GenerateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var canvas = CreateCanvas("MainMenu_Canvas");
            CreateBackground(canvas.transform);
            CreateSystems();
            CreateEventSystem();

            var title = CreateText("Title", canvas.transform, "game_title", 64, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.77f), new Vector2(840f, 110f));
            title.gameObject.AddComponent<LocalizedText>().key = "game_title";

            var subtitle = CreateText("Subtitle", canvas.transform, "game_subtitle", 27, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.69f), new Vector2(820f, 70f));
            subtitle.gameObject.AddComponent<LocalizedText>().key = "game_subtitle";

            var badge = CreateText("Badge", canvas.transform, "menu_best", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.61f), new Vector2(620f, 52f));
            badge.gameObject.AddComponent<LocalizedText>().key = "menu_best";
            var badgeImage = badge.gameObject.AddComponent<Outline>();
            badgeImage.effectColor = new Color(0f, 0.12f, 0.18f, 0.55f);
            badgeImage.effectDistance = new Vector2(2f, -2f);

            var controllerObject = new GameObject("MainMenuController");
            var controller = controllerObject.AddComponent<MainMenuController>();

            controller.playButton = CreateButton("PlayButton", canvas.transform, "menu_play", new Vector2(0.5f, 0.43f), new Vector2(520f, 92f), new Color(0.98f, 0.71f, 0.16f));
            controller.languageButton = CreateButton("LanguageButton", canvas.transform, "menu_language", new Vector2(0.5f, 0.33f), new Vector2(430f, 72f), new Color(0.18f, 0.58f, 0.86f));
            controller.rewardAdButton = CreateButton("RewardAdButton", canvas.transform, "menu_reward_ad", new Vector2(0.5f, 0.245f), new Vector2(460f, 66f), new Color(0.24f, 0.65f, 0.34f));
            controller.privacyButton = CreateButton("PrivacyButton", canvas.transform, "menu_privacy", new Vector2(0.5f, 0.168f), new Vector2(340f, 60f), new Color(0.14f, 0.22f, 0.28f, 0.9f));
            controller.quitButton = CreateButton("QuitButton", canvas.transform, "menu_quit", new Vector2(0.5f, 0.095f), new Vector2(320f, 56f), new Color(0.1f, 0.18f, 0.25f, 0.88f));

            var adStatus = CreateText("AdStatus", canvas.transform, "ad_status_ready", 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.205f), new Vector2(760f, 36f));
            adStatus.gameObject.AddComponent<LocalizedText>().key = "ad_status_ready";
            controller.adStatusText = adStatus;

            CreatePrivacyPanel(canvas.transform, controller);

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static Canvas CreateCanvas(string name)
        {
            var canvasObject = new GameObject(name);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreateBackground(Transform parent)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SplashImagePath);
            var background = new GameObject("BrandingBackground");
            background.transform.SetParent(parent, false);

            var rect = background.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = background.AddComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.raycastTarget = false;
            background.AddComponent<AspectCoverImage>();

            return background;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static void CreatePrivacyPanel(Transform parent, MainMenuController controller)
        {
            var overlay = CreatePanel("PrivacyOverlay", parent, new Color(0f, 0f, 0f, 0.72f));

            var sheet = new GameObject("PrivacySheet");
            sheet.transform.SetParent(overlay.transform, false);
            var sheetRect = sheet.AddComponent<RectTransform>();
            sheetRect.anchorMin = new Vector2(0.5f, 0.5f);
            sheetRect.anchorMax = new Vector2(0.5f, 0.5f);
            sheetRect.pivot = new Vector2(0.5f, 0.5f);
            sheetRect.anchoredPosition = Vector2.zero;
            sheetRect.sizeDelta = new Vector2(850f, 620f);

            var sheetImage = sheet.AddComponent<Image>();
            sheetImage.color = new Color(0.05f, 0.1f, 0.12f, 0.96f);

            var title = CreateText("PrivacyTitle", sheet.transform, "privacy_title", 38, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.78f), new Vector2(760f, 70f));
            title.gameObject.AddComponent<LocalizedText>().key = "privacy_title";

            var body = CreateText("PrivacyBody", sheet.transform, "privacy_body", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.52f), new Vector2(720f, 230f));
            body.gameObject.AddComponent<LocalizedText>().key = "privacy_body";
            body.fontStyle = FontStyle.Normal;
            body.lineSpacing = 1.2f;

            controller.acceptPrivacyButton = CreateButton("AcceptPrivacyButton", sheet.transform, "menu_accept_privacy", new Vector2(0.5f, 0.18f), new Vector2(520f, 76f), new Color(0.98f, 0.71f, 0.16f));
            controller.privacyPanel = overlay;
            overlay.SetActive(false);
        }

        private static Text CreateText(string name, Transform parent, string fallback, int size, TextAnchor alignment, Vector2 anchor, Vector2 sizeDelta)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = sizeDelta;

            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = fallback;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;

            var shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0.08f, 0.12f, 0.65f);
            shadow.effectDistance = new Vector2(3f, -3f);

            return text;
        }

        private static Button CreateButton(string name, Transform parent, string localizationKey, Vector2 anchor, Vector2 sizeDelta, Color color)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = sizeDelta;

            var image = buttonObject.AddComponent<Image>();
            image.color = color;

            var button = buttonObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            button.colors = colors;

            var label = CreateText("Label", buttonObject.transform, localizationKey, Mathf.RoundToInt(sizeDelta.y * 0.36f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), sizeDelta);
            label.gameObject.AddComponent<LocalizedText>().key = localizationKey;

            return button;
        }

        private static void CreateSystems()
        {
            var systems = new GameObject("GameSystems");
            systems.AddComponent<GameBootstrap>();
            var localization = systems.AddComponent<LocalizationService>();
            localization.stringTable = AssetDatabase.LoadAssetAtPath<TextAsset>(LocalizationPath);
            localization.defaultLanguage = "zh-Hans";
        }

        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static void ConfigureBuildScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SplashScenePath, true),
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(CityScenePath, true)
            };
        }
    }
}
