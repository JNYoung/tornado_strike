using System.Collections.Generic;
using TornadoStrike.CameraRig;
using TornadoStrike.Core;
using TornadoStrike.Gameplay;
using TornadoStrike.Localization;
using TornadoStrike.Player;
using TornadoStrike.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TornadoStrike.Editor
{
    public static class CityMvpSceneGenerator
    {
        private const string ScenePath = "Assets/TornadoStrike/Scenes/City_MVP.unity";
        private const string SplashScenePath = "Assets/TornadoStrike/Scenes/Splash.unity";
        private const string MainMenuScenePath = "Assets/TornadoStrike/Scenes/MainMenu.unity";
        private const string LocalizationPath = "Assets/TornadoStrike/Resources/Localization/localization.txt";

        private static readonly Color Asphalt = new Color(0.11f, 0.18f, 0.24f);
        private static readonly Color RoadLine = new Color(1f, 0.87f, 0.22f);
        private static readonly Color Grass = new Color(0.42f, 0.73f, 0.22f);
        private static readonly Color Sidewalk = new Color(0.82f, 0.86f, 0.84f);
        private static readonly Color TornadoBlue = new Color(0.08f, 0.56f, 1f, 0.58f);
        private static readonly Color CarRed = new Color(0.96f, 0.16f, 0.1f);
        private static readonly Color BusYellow = new Color(1f, 0.74f, 0.06f);
        private static readonly Color HouseA = new Color(1f, 0.72f, 0.28f);
        private static readonly Color HouseB = new Color(0.32f, 0.67f, 0.94f);
        private static readonly Color PowerPlant = new Color(0.42f, 0.47f, 0.5f);
        private static readonly Color Police = new Color(0.13f, 0.28f, 0.72f);
        private static readonly Color Fire = new Color(0.78f, 0.12f, 0.08f);
        private static readonly Color Concrete = new Color(0.78f, 0.8f, 0.76f);
        private static readonly Color Brick = new Color(0.82f, 0.3f, 0.18f);
        private static readonly Color Metal = new Color(0.34f, 0.36f, 0.36f);
        private static readonly Color CarGlass = new Color(0.12f, 0.24f, 0.32f);

        [MenuItem("Tornado Strike/Generate City MVP Scene")]
        public static void GenerateCityScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var materials = CreateMaterials();
            ConfigureProjectSettings();

            var root = new GameObject("City_MVP_Root");
            CreateLighting();

            var tornado = CreateTornado(materials);
            CreateCamera(tornado.transform);
            CreateSystems();
            CreateHud(tornado);
            CreateInfiniteCityWorld(root.transform, tornado.transform, materials);

            EditorSceneManager.SaveScene(scene, ScenePath);
            ConfigureBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Generated Tornado Strike city MVP scene at {ScenePath}");
        }

        public static void GenerateCitySceneBatch()
        {
            GenerateCityScene();
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            return new Dictionary<string, Material>
            {
                ["grass"] = Material("City/Grass", Grass, 0f, 0.18f),
                ["asphalt"] = Material("City/Asphalt", Asphalt, 0f, 0.22f),
                ["roadLine"] = Material("City/RoadLine", RoadLine, 0f, 0.36f),
                ["sidewalk"] = Material("City/Sidewalk", Sidewalk, 0f, 0.28f),
                ["curb"] = Material("City/Curb", new Color(0.9f, 0.92f, 0.88f), 0f, 0.34f),
                ["tornado"] = TransparentMaterial("FX/TornadoBlue", TornadoBlue),
                ["car"] = Material("Vehicles/CarRed", CarRed, 0f, 0.58f),
                ["bus"] = Material("Vehicles/BusYellow", BusYellow, 0f, 0.48f),
                ["houseA"] = Material("Buildings/HouseA", HouseA, 0f, 0.33f),
                ["houseB"] = Material("Buildings/HouseB", HouseB, 0f, 0.37f),
                ["roof"] = Material("Buildings/Roof", new Color(0.95f, 0.22f, 0.16f), 0f, 0.32f),
                ["powerPlant"] = Material("Special/PowerPlant", PowerPlant, 0.08f, 0.42f),
                ["police"] = Material("Special/Police", Police, 0f, 0.5f),
                ["fire"] = Material("Special/Fire", Fire, 0f, 0.5f),
                ["glass"] = Material("Buildings/Glass", new Color(0.2f, 0.58f, 0.9f), 0f, 0.78f),
                ["treeTrunk"] = Material("Props/TreeTrunk", new Color(0.48f, 0.25f, 0.09f), 0f, 0.2f),
                ["treeCanopy"] = Material("Props/TreeCanopy", new Color(0.34f, 0.78f, 0.18f), 0f, 0.24f),
                ["lampPole"] = Material("Props/LampPole", new Color(0.2f, 0.22f, 0.24f), 0.25f, 0.5f),
                ["lampLight"] = EmissionMaterial("Props/LampLight", new Color(1f, 0.86f, 0.38f), new Color(1.2f, 0.86f, 0.28f)),
                ["black"] = Material("Utility/Black", Color.black, 0f, 0.2f),
                ["white"] = Material("Utility/White", Color.white, 0f, 0.38f),
                ["concrete"] = Material("Detail/Concrete", Concrete, 0f, 0.31f),
                ["brick"] = Material("Detail/Brick", Brick, 0f, 0.25f),
                ["metal"] = Material("Detail/Metal", Metal, 0.45f, 0.55f),
                ["roadWear"] = Material("Detail/RoadWear", new Color(0.18f, 0.25f, 0.3f), 0f, 0.18f),
                ["sidewalkLine"] = Material("Detail/SidewalkLine", new Color(0.64f, 0.69f, 0.67f), 0f, 0.24f),
                ["windowFrame"] = Material("Detail/WindowFrame", new Color(0.82f, 0.84f, 0.82f), 0f, 0.45f),
                ["carGlass"] = Material("Vehicles/CarGlass", CarGlass, 0f, 0.82f),
                ["tire"] = Material("Vehicles/TireRubber", new Color(0.025f, 0.025f, 0.026f), 0f, 0.28f),
                ["headlight"] = EmissionMaterial("Vehicles/Headlight", new Color(1f, 0.93f, 0.7f), new Color(1.3f, 1.05f, 0.62f)),
                ["tailLight"] = EmissionMaterial("Vehicles/TailLight", new Color(0.95f, 0.08f, 0.04f), new Color(1.1f, 0.08f, 0.04f)),
                ["warningStripe"] = Material("Detail/WarningStripe", new Color(0.95f, 0.78f, 0.05f), 0f, 0.42f),
                ["sign"] = Material("Detail/SignPaint", new Color(0.04f, 0.38f, 0.95f), 0f, 0.5f),
                ["pedestrianSkin"] = Material("Pedestrian/Skin", new Color(0.86f, 0.58f, 0.38f), 0f, 0.3f),
                ["pedestrianShirt"] = Material("Pedestrian/Shirt", new Color(0.14f, 0.44f, 0.82f), 0f, 0.42f),
                ["pedestrianPants"] = Material("Pedestrian/Pants", new Color(0.12f, 0.15f, 0.2f), 0f, 0.34f),
                ["pedestrianHair"] = Material("Pedestrian/Hair", new Color(0.08f, 0.045f, 0.025f), 0f, 0.22f),
                ["leafDark"] = Material("Props/TreeCanopyDark", new Color(0.24f, 0.62f, 0.12f), 0f, 0.22f)
            };
        }

        private static Material Material(string name, Color color, float metallic = 0f, float smoothness = 0.35f)
        {
            var material = new Material(Shader.Find("Standard"))
            {
                name = name,
                color = color
            };
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", smoothness);
            return material;
        }

        private static Material EmissionMaterial(string name, Color color, Color emission)
        {
            var material = Material(name, color, 0f, 0.65f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            return material;
        }

        private static Material TransparentMaterial(string name, Color color)
        {
            var material = Material(name, color);
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            return material;
        }

        private static void ConfigureProjectSettings()
        {
            PlayerSettings.companyName = "JNYoung";
            PlayerSettings.productName = "Tornado Strike";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.jnyoung.tornadostrike");
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.jnyoung.tornadostrike");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.bundleVersion = "0.1.0";
        }

        private static void ConfigureBuildScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            AddBuildSceneIfExists(scenes, SplashScenePath);
            AddBuildSceneIfExists(scenes, MainMenuScenePath);
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AddBuildSceneIfExists(ICollection<EditorBuildSettingsScene> scenes, string path)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }
        }

        private static void CreateGround(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            var ground = Primitive("PlayableGround", PrimitiveType.Cube, parent, materials["grass"]);
            ground.transform.position = new Vector3(0f, -0.05f, 0f);
            ground.transform.localScale = new Vector3(72f, 0.1f, 72f);

            for (var x = -2; x <= 2; x++)
            {
                for (var z = -2; z <= 2; z++)
                {
                    var block = Primitive($"Sidewalk_{x}_{z}", PrimitiveType.Cube, parent, materials["sidewalk"]);
                    block.transform.position = new Vector3(x * 12f, 0.01f, z * 12f);
                    block.transform.localScale = new Vector3(7.5f, 0.08f, 7.5f);
                }
            }
        }

        private static void CreateRoadGrid(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            for (var i = -2; i <= 2; i++)
            {
                var horizontal = Primitive($"Road_H_{i}", PrimitiveType.Cube, parent, materials["asphalt"]);
                horizontal.transform.position = new Vector3(0f, 0.04f, i * 12f);
                horizontal.transform.localScale = new Vector3(70f, 0.08f, 3.2f);

                var vertical = Primitive($"Road_V_{i}", PrimitiveType.Cube, parent, materials["asphalt"]);
                vertical.transform.position = new Vector3(i * 12f, 0.05f, 0f);
                vertical.transform.localScale = new Vector3(3.2f, 0.08f, 70f);

                var hLine = Primitive($"RoadLine_H_{i}", PrimitiveType.Cube, parent, materials["roadLine"]);
                hLine.transform.position = new Vector3(0f, 0.1f, i * 12f);
                hLine.transform.localScale = new Vector3(65f, 0.03f, 0.12f);

                var vLine = Primitive($"RoadLine_V_{i}", PrimitiveType.Cube, parent, materials["roadLine"]);
                vLine.transform.position = new Vector3(i * 12f, 0.11f, 0f);
                vLine.transform.localScale = new Vector3(0.12f, 0.03f, 65f);
            }
        }

        private static TornadoGrowth CreateTornado(IReadOnlyDictionary<string, Material> materials)
        {
            var tornado = new GameObject("Player_Tornado");
            tornado.transform.position = Vector3.zero;
            tornado.tag = "Player";

            var body = tornado.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;

            var trigger = tornado.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.25f;

            var controller = tornado.AddComponent<TornadoController>();
            controller.useWorldBounds = false;
            controller.worldBounds = new Vector2(32f, 32f);

            var growth = tornado.AddComponent<TornadoGrowth>();
            growth.absorptionTrigger = trigger;
            growth.currentRadius = 1.25f;
            growth.maxRadius = 8f;

            var visualRoot = new GameObject("Visual");
            visualRoot.transform.SetParent(tornado.transform);
            visualRoot.transform.localPosition = Vector3.zero;
            growth.visualRoot = visualRoot.transform;
            visualRoot.AddComponent<TornadoVortexVisual>();

            for (var i = 0; i < 8; i++)
            {
                var ring = Primitive($"SwirlRing_{i + 1}", PrimitiveType.Cylinder, visualRoot.transform, materials["tornado"]);
                ring.transform.localPosition = new Vector3(0f, 0.18f + i * 0.3f, 0f);
                ring.transform.localScale = new Vector3(0.42f + i * 0.08f, 0.05f, 0.42f + i * 0.08f);
                ring.transform.localRotation = Quaternion.Euler(0f, i * 27f, 0f);
                Object.DestroyImmediate(ring.GetComponent<Collider>());
            }

            for (var i = 0; i < 12; i++)
            {
                var debris = Primitive($"Debris_{i + 1}", PrimitiveType.Cube, visualRoot.transform, i % 2 == 0 ? materials["roof"] : materials["black"]);
                var angle = i * 30f * Mathf.Deg2Rad;
                var radius = 0.55f + (i % 4) * 0.12f;
                debris.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, 0.28f + i * 0.11f, Mathf.Sin(angle) * radius);
                debris.transform.localScale = Vector3.one * (0.08f + (i % 3) * 0.025f);
                debris.transform.localRotation = Quaternion.Euler(i * 17f, i * 31f, i * 11f);
                Object.DestroyImmediate(debris.GetComponent<Collider>());
            }

            for (var i = 0; i < 9; i++)
            {
                var ribbon = Primitive($"WhiteWindRibbon_{i + 1}", PrimitiveType.Cube, visualRoot.transform, materials["white"]);
                var angle = (i * 40f + 15f) * Mathf.Deg2Rad;
                var radius = 0.58f + i * 0.055f;
                ribbon.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, 0.42f + i * 0.25f, Mathf.Sin(angle) * radius);
                ribbon.transform.localScale = new Vector3(0.52f + i * 0.05f, 0.045f, 0.08f);
                ribbon.transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg + 18f, 8f);
                Object.DestroyImmediate(ribbon.GetComponent<Collider>());
            }

            var particleObject = new GameObject("SwirlParticles");
            particleObject.transform.SetParent(tornado.transform);
            particleObject.transform.localPosition = Vector3.up * 0.3f;
            var particles = particleObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.startLifetime = 0.8f;
            main.startSpeed = 1.5f;
            main.startSize = 0.35f;
            main.maxParticles = 120;

            var emission = particles.emission;
            emission.rateOverTime = 90f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.8f;
            shape.length = 1.4f;

            var particleRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sharedMaterial = materials["tornado"];
            particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;

            growth.swirlFx = particles;
            return growth;
        }

        private static void CreateInfiniteCityWorld(Transform parent, Transform target, IReadOnlyDictionary<string, Material> materials)
        {
            var worldObject = new GameObject("InfiniteCityWorld");
            worldObject.transform.SetParent(parent);

            var world = worldObject.AddComponent<InfiniteCityWorld>();
            world.target = target;
            world.chunkSize = 24f;
            world.activeRadius = 2;
            world.seed = 314159;
            world.buildingDensity = 1.15f;
            world.vehicleDensity = 1.25f;
            world.streetPropDensity = 1.45f;
            world.pedestrianDensity = 1.05f;
            world.surfaceDetailDensity = 1f;
            world.minBuildingsPerChunk = 8;
            world.maxBuildingsPerChunk = 13;
            world.streetPropsPerChunk = 12;
            world.pedestriansPerChunk = 5;
            world.grassMaterial = materials["grass"];
            world.asphaltMaterial = materials["asphalt"];
            world.roadLineMaterial = materials["roadLine"];
            world.sidewalkMaterial = materials["sidewalk"];
            world.curbMaterial = materials["curb"];
            world.carMaterial = materials["car"];
            world.busMaterial = materials["bus"];
            world.houseMaterialA = materials["houseA"];
            world.houseMaterialB = materials["houseB"];
            world.roofMaterial = materials["roof"];
            world.glassMaterial = materials["glass"];
            world.whiteMaterial = materials["white"];
            world.blackMaterial = materials["black"];
            world.treeTrunkMaterial = materials["treeTrunk"];
            world.treeCanopyMaterial = materials["treeCanopy"];
            world.lampPoleMaterial = materials["lampPole"];
            world.lampLightMaterial = materials["lampLight"];
            world.powerPlantMaterial = materials["powerPlant"];
            world.policeMaterial = materials["police"];
            world.fireStationMaterial = materials["fire"];
            world.markerMaterial = materials["white"];
            world.concreteMaterial = materials["concrete"];
            world.brickMaterial = materials["brick"];
            world.metalMaterial = materials["metal"];
            world.roadWearMaterial = materials["roadWear"];
            world.sidewalkLineMaterial = materials["sidewalkLine"];
            world.windowFrameMaterial = materials["windowFrame"];
            world.carGlassMaterial = materials["carGlass"];
            world.tireMaterial = materials["tire"];
            world.headlightMaterial = materials["headlight"];
            world.tailLightMaterial = materials["tailLight"];
            world.warningStripeMaterial = materials["warningStripe"];
            world.signMaterial = materials["sign"];
            world.pedestrianSkinMaterial = materials["pedestrianSkin"];
            world.pedestrianShirtMaterial = materials["pedestrianShirt"];
            world.pedestrianPantsMaterial = materials["pedestrianPants"];
            world.pedestrianHairMaterial = materials["pedestrianHair"];
            world.leafDarkMaterial = materials["leafDark"];
        }

        private static void CreateCamera(Transform target)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 17f, -16f);
            cameraObject.transform.rotation = Quaternion.Euler(50f, 0f, 0f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.62f, 0.72f, 0.82f);
            camera.fieldOfView = 48f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 220f;
            camera.allowHDR = false;
            camera.allowMSAA = true;

            var follow = cameraObject.AddComponent<FollowCamera>();
            follow.target = target;
            follow.offset = new Vector3(0f, 17f, -16f);
        }

        private static void CreateLighting()
        {
            var sun = new GameObject("Directional Light");
            sun.transform.rotation = Quaternion.Euler(48f, -34f, 0f);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.08f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.68f;
            light.shadowBias = 0.035f;
            light.shadowNormalBias = 0.32f;

            var fill = new GameObject("Soft Fill Light");
            fill.transform.rotation = Quaternion.Euler(28f, 136f, 0f);
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = 0.22f;
            fillLight.shadows = LightShadows.None;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.72f, 0.83f);
            RenderSettings.ambientEquatorColor = new Color(0.48f, 0.52f, 0.54f);
            RenderSettings.ambientGroundColor = new Color(0.28f, 0.3f, 0.28f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.66f, 0.74f, 0.79f);
            RenderSettings.fogStartDistance = 54f;
            RenderSettings.fogEndDistance = 165f;
            RenderSettings.reflectionIntensity = 0.35f;

            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.shadowDistance = 70f;
            QualitySettings.antiAliasing = 2;
        }

        private static void CreateSystems()
        {
            var systems = new GameObject("GameSystems");
            systems.AddComponent<GameBootstrap>();
            var localization = systems.AddComponent<LocalizationService>();
            localization.stringTable = AssetDatabase.LoadAssetAtPath<TextAsset>(LocalizationPath);
            localization.defaultLanguage = "zh-Hans";
        }

        private static void CreateHud(TornadoGrowth tornado)
        {
            var canvasObject = new GameObject("HUD_Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080f, 1920f);
            canvasObject.AddComponent<GraphicRaycaster>();

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            var panel = UiPanel("TopHud", canvasObject.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 170f), new Color(0f, 0f, 0f, 0.35f));
            var title = UiText("TitleText", panel.transform, "game_title", 34, TextAnchor.MiddleLeft, new Vector2(36f, -18f), new Vector2(480f, 48f));
            title.gameObject.AddComponent<LocalizedText>().key = "game_title";

            var hint = UiText("HintText", panel.transform, "hint_drag", 22, TextAnchor.MiddleLeft, new Vector2(36f, -74f), new Vector2(650f, 36f));
            hint.gameObject.AddComponent<LocalizedText>().key = "hint_drag";

            var score = UiText("ScoreText", panel.transform, "Score 0/4200", 24, TextAnchor.MiddleRight, new Vector2(-36f, -20f), new Vector2(360f, 36f));
            var radius = UiText("RadiusText", panel.transform, "Radius 1.3", 22, TextAnchor.MiddleRight, new Vector2(-36f, -62f), new Vector2(360f, 36f));
            var timer = UiText("TimerText", panel.transform, "Time 360s", 22, TextAnchor.MiddleRight, new Vector2(-36f, -104f), new Vector2(360f, 36f));

            var completion = UiPanel("CompletionPanel", canvasObject.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 180f), new Color(0.05f, 0.09f, 0.1f, 0.85f));
            var completionText = UiText("CompletionText", completion.transform, "level_complete", 34, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(460f, 120f));
            completionText.gameObject.AddComponent<LocalizedText>().key = "level_complete";
            completion.SetActive(false);

            var hud = canvasObject.AddComponent<LevelProgressHud>();
            hud.tornado = tornado;
            hud.targetScore = TornadoBalanceRules.DefaultTargetScore;
            hud.levelDurationSeconds = TornadoBalanceRules.DefaultRoundSeconds;
            hud.scoreText = score;
            hud.radiusText = radius;
            hud.timerText = timer;
            hud.completionPanel = completion;
        }

        private static GameObject UiPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            var image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Text UiText(string name, Transform parent, string text, int size, TextAnchor alignment, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = alignment.ToString().Contains("Right") ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = alignment.ToString().Contains("Right") ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var label = textObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text;
            label.fontSize = size;
            label.color = Color.white;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private static void CreateCityBlocks(Transform absorbablesRoot, Transform slotsRoot, IReadOnlyDictionary<string, Material> materials)
        {
            var houseIndex = 0;
            for (var x = -2; x <= 2; x++)
            {
                for (var z = -2; z <= 2; z++)
                {
                    if (x == 0 && z == 0)
                    {
                        continue;
                    }

                    var blockCenter = new Vector3(x * 12f, 0f, z * 12f);
                    if (x == -2 && z == 1)
                    {
                        CreateSpecialBuilding("PowerPlant", blockCenter, slotsRoot, absorbablesRoot, materials["powerPlant"], SceneSlotKind.PowerPlant, "slot_power_plant", 3.5f, 85, 0.32f);
                    }
                    else if (x == 2 && z == 0)
                    {
                        CreateSpecialBuilding("PoliceStation", blockCenter, slotsRoot, absorbablesRoot, materials["police"], SceneSlotKind.PoliceStation, "slot_police_station", 2.8f, 70, 0.26f);
                    }
                    else if (x == 1 && z == -2)
                    {
                        CreateSpecialBuilding("FireStation", blockCenter, slotsRoot, absorbablesRoot, materials["fire"], SceneSlotKind.FireStation, "slot_fire_station", 2.9f, 75, 0.28f);
                    }
                    else
                    {
                        CreateHouseCluster(blockCenter, absorbablesRoot, materials, ref houseIndex);
                    }
                }
            }

            CreateVehicles(absorbablesRoot, materials);
            CreateFutureRainforestSlot(slotsRoot);
        }

        private static void CreateHouseCluster(Vector3 blockCenter, Transform parent, IReadOnlyDictionary<string, Material> materials, ref int houseIndex)
        {
            var offsets = new[]
            {
                new Vector3(-2.2f, 0f, -2f),
                new Vector3(2.1f, 0f, -1.7f),
                new Vector3(-1.7f, 0f, 2.1f),
                new Vector3(2.3f, 0f, 2f)
            };

            foreach (var offset in offsets)
            {
                houseIndex++;
                var height = 1.2f + (houseIndex % 3) * 0.45f;
                var house = Primitive($"House_{houseIndex:00}", PrimitiveType.Cube, parent, houseIndex % 2 == 0 ? materials["houseA"] : materials["houseB"]);
                house.transform.position = blockCenter + offset + Vector3.up * height * 0.5f;
                house.transform.localScale = new Vector3(1.8f, height, 1.7f);

                var absorbable = house.AddComponent<Absorbable>();
                absorbable.absorbableId = "city_house";
                absorbable.category = AbsorbableCategory.Building;
                absorbable.localizationKey = "object_house";
                absorbable.requiredRadius = 1.3f + height * 0.28f;
                absorbable.growthValue = 0.045f + height * 0.01f;
                absorbable.scoreValue = 12 + Mathf.RoundToInt(height * 4f);

                var roof = Primitive($"House_{houseIndex:00}_Roof", PrimitiveType.Cube, house.transform, materials["roof"]);
                roof.transform.localPosition = new Vector3(0f, 0.58f, 0f);
                roof.transform.localScale = new Vector3(1.12f, 0.18f, 1.12f);
                Object.DestroyImmediate(roof.GetComponent<Collider>());
            }
        }

        private static void CreateSpecialBuilding(string id, Vector3 blockCenter, Transform slotsRoot, Transform absorbablesRoot, Material material, SceneSlotKind kind, string localizationKey, float requiredRadius, int score, float growth)
        {
            var slot = new GameObject($"{id}_Slot");
            slot.transform.SetParent(slotsRoot);
            slot.transform.position = blockCenter;
            var sceneSlot = slot.AddComponent<SceneSlot>();
            sceneSlot.slotId = id;
            sceneSlot.kind = kind;
            sceneSlot.displayNameKey = localizationKey;
            sceneSlot.recommendedTier = Mathf.RoundToInt(requiredRadius);

            var building = Primitive(id, PrimitiveType.Cube, absorbablesRoot, material);
            building.transform.position = blockCenter + Vector3.up * 1.4f;
            building.transform.localScale = new Vector3(5.6f, 2.8f, 4.8f);
            building.transform.SetParent(slot.transform, true);

            var absorbable = building.AddComponent<Absorbable>();
            absorbable.absorbableId = id;
            absorbable.category = AbsorbableCategory.SpecialBuilding;
            absorbable.localizationKey = localizationKey;
            absorbable.requiredRadius = requiredRadius;
            absorbable.growthValue = growth;
            absorbable.scoreValue = score;
            absorbable.isSpecialSlot = true;
            absorbable.slotKey = id;

            var marker = Primitive($"{id}_Marker", PrimitiveType.Cylinder, slot.transform, Material($"{id}/Marker", Color.white));
            marker.transform.position = blockCenter + Vector3.up * 3.05f;
            marker.transform.localScale = new Vector3(1.2f, 0.05f, 1.2f);
            Object.DestroyImmediate(marker.GetComponent<Collider>());
        }

        private static void CreateVehicles(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            var carPositions = new[]
            {
                new Vector3(-18f, 0f, -12f),
                new Vector3(-6f, 0f, 12f),
                new Vector3(12f, 0f, 6f),
                new Vector3(24f, 0f, -12f),
                new Vector3(-24f, 0f, 24f),
                new Vector3(6f, 0f, -24f)
            };

            for (var i = 0; i < carPositions.Length; i++)
            {
                CreateVehicle($"Car_{i + 1:00}", carPositions[i], parent, materials["car"], false);
            }

            CreateVehicle("Bus_01", new Vector3(-12f, 0f, 0f), parent, materials["bus"], true);
            CreateVehicle("Bus_02", new Vector3(0f, 0f, 24f), parent, materials["bus"], true);
        }

        private static void CreateVehicle(string id, Vector3 position, Transform parent, Material material, bool isBus)
        {
            var vehicle = new GameObject(id);
            vehicle.transform.SetParent(parent);
            vehicle.transform.position = position;

            var body = Primitive($"{id}_Body", PrimitiveType.Cube, vehicle.transform, material);
            body.transform.localPosition = Vector3.up * (isBus ? 0.42f : 0.3f);
            body.transform.localScale = isBus ? new Vector3(3.2f, 0.8f, 1.2f) : new Vector3(1.5f, 0.55f, 0.9f);

            var collider = vehicle.AddComponent<BoxCollider>();
            collider.center = body.transform.localPosition;
            collider.size = body.transform.localScale;

            var absorbable = vehicle.AddComponent<Absorbable>();
            absorbable.absorbableId = isBus ? "city_bus" : "city_car";
            absorbable.category = AbsorbableCategory.Vehicle;
            absorbable.localizationKey = isBus ? "object_bus" : "object_car";
            absorbable.requiredRadius = isBus ? 2.15f : 1.15f;
            absorbable.growthValue = isBus ? 0.08f : 0.03f;
            absorbable.scoreValue = isBus ? 24 : 8;

            Object.DestroyImmediate(body.GetComponent<Collider>());
        }

        private static void CreateFutureRainforestSlot(Transform slotsRoot)
        {
            var slot = new GameObject("RainforestBiome_FutureSlot");
            slot.transform.SetParent(slotsRoot);
            slot.transform.position = new Vector3(34f, 0f, 34f);
            var sceneSlot = slot.AddComponent<SceneSlot>();
            sceneSlot.slotId = "rainforest_biome";
            sceneSlot.kind = SceneSlotKind.RainforestFuture;
            sceneSlot.displayNameKey = "slot_generic";
            sceneSlot.recommendedTier = 4;
            sceneSlot.participatesInMvp = false;
        }

        private static GameObject Primitive(string name, PrimitiveType type, Transform parent, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent);
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            return go;
        }
    }
}
