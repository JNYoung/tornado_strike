using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TornadoStrike.Tests.EditMode
{
    public sealed class TornadoStrikeMvpEditModeTests
    {
        private static readonly string[] RequiredLanguages =
        {
            "zh-Hans",
            "zh-Hant",
            "en",
            "de",
            "fr",
            "ja",
            "ar"
        };

        private static readonly string[] RequiredLocalizationKeys =
        {
            "game_title",
            "menu_play",
            "menu_language",
            "menu_privacy",
            "menu_accept_privacy",
            "menu_reward_ad",
            "privacy_body",
            "ad_status_ready",
            "hud_score",
            "hud_radius",
            "hud_timer",
            "object_car",
            "object_bus",
            "object_house",
            "object_tree",
            "object_lamp_post",
            "object_pedestrian",
            "slot_power_plant",
            "slot_police_station",
            "slot_fire_station"
        };

        [Test]
        public void MvpRoundDurationStaysWithinThreeToTenMinutes()
        {
            var balanceRules = Type.GetType("TornadoStrike.Gameplay.TornadoBalanceRules, Assembly-CSharp");
            Assert.That(balanceRules, Is.Not.Null);

            var defaultRound = (float)balanceRules.GetField("DefaultRoundSeconds").GetRawConstantValue();
            var minRound = (float)balanceRules.GetField("MinMvpRoundSeconds").GetRawConstantValue();
            var maxRound = (float)balanceRules.GetField("MaxMvpRoundSeconds").GetRawConstantValue();
            Assert.That(defaultRound, Is.InRange(minRound, maxRound));

            var estimateMethod = balanceRules.GetMethod("EstimateCompletionSeconds");
            var estimatedCompletion = (float)estimateMethod.Invoke(null, new object[] { 4200, 700f });
            Assert.That(estimatedCompletion, Is.InRange(minRound, maxRound));
        }

        [Test]
        public void LocalizationTableContainsAllRequiredLanguagesAndKeys()
        {
            var tablePath = "Assets/TornadoStrike/Resources/Localization/localization.tsv";
            var runtimeTablePath = "Assets/TornadoStrike/Resources/Localization/localization.txt";
            Assert.That(File.Exists(tablePath), Is.True, $"Missing localization table at {tablePath}.");
            Assert.That(File.Exists(runtimeTablePath), Is.True, $"Missing Unity runtime localization text asset at {runtimeTablePath}.");
            Assert.That(File.ReadAllText(runtimeTablePath), Is.EqualTo(File.ReadAllText(tablePath)), "Runtime localization.txt must match source localization.tsv.");
            Assert.That(AssetDatabase.LoadAssetAtPath<TextAsset>(runtimeTablePath), Is.Not.Null, "Unity must import localization.txt as a TextAsset.");

            var lines = File.ReadAllLines(tablePath);
            Assert.That(lines.Length, Is.GreaterThan(1));

            var headers = lines[0].Split('\t');
            CollectionAssert.IsSubsetOf(RequiredLanguages, headers);

            var rows = new Dictionary<string, string[]>();
            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var columns = lines[i].Split('\t');
                Assert.That(columns.Length, Is.EqualTo(headers.Length), $"Column mismatch on line {i + 1}: {columns[0]}");
                rows.Add(columns[0], columns);

                for (var column = 1; column < columns.Length; column++)
                {
                    Assert.That(columns[column], Is.Not.Empty, $"Missing {headers[column]} value for {columns[0]}.");
                }
            }

            CollectionAssert.IsSubsetOf(RequiredLocalizationKeys, rows.Keys.ToArray());
        }

        [Test]
        public void BuildSettingsContainSplashMenuAndCityScenes()
        {
            var requiredScenes = new[]
            {
                "Assets/TornadoStrike/Scenes/Splash.unity",
                "Assets/TornadoStrike/Scenes/MainMenu.unity",
                "Assets/TornadoStrike/Scenes/City_MVP.unity"
            };

            var enabledScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            CollectionAssert.IsSubsetOf(requiredScenes, enabledScenes);

            foreach (var scenePath in requiredScenes)
            {
                Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath), Is.Not.Null, $"Missing scene asset: {scenePath}");
            }
        }

        [Test]
        public void InfiniteWorldExposesRealisticCityArtControls()
        {
            var worldType = Type.GetType("TornadoStrike.Gameplay.InfiniteCityWorld, Assembly-CSharp");
            Assert.That(worldType, Is.Not.Null);

            var requiredFields = new[]
            {
                "pedestrianDensity",
                "surfaceDetailDensity",
                "concreteMaterial",
                "roadWearMaterial",
                "carGlassMaterial",
                "pedestrianSkinMaterial",
                "leafDarkMaterial"
            };

            foreach (var field in requiredFields)
            {
                Assert.That(worldType.GetField(field), Is.Not.Null, $"Missing city art control: {field}");
            }
        }

        [Test]
        public void AndroidBuildPreservesPrimitiveColliderTypes()
        {
            var linkXml = "Assets/TornadoStrike/link.xml";
            Assert.That(File.Exists(linkXml), Is.True, "Android IL2CPP stripping must preserve colliders used by GameObject.CreatePrimitive.");

            var contents = File.ReadAllText(linkXml);
            Assert.That(contents, Does.Contain("UnityEngine.CapsuleCollider"));
            Assert.That(contents, Does.Contain("UnityEngine.SphereCollider"));
            Assert.That(contents, Does.Contain("UnityEngine.BoxCollider"));
        }

        [Test]
        public void CitySceneContainsInfiniteWorldAndMenuSceneContainsComplianceHooks()
        {
            Assert.That(File.ReadAllText("Assets/TornadoStrike/Scenes/City_MVP.unity"), Does.Contain("InfiniteCityWorld"));

            var menuScene = File.ReadAllText("Assets/TornadoStrike/Scenes/MainMenu.unity");
            Assert.That(menuScene, Does.Contain("PrivacyOverlay"));
            Assert.That(menuScene, Does.Contain("RewardAdButton"));
        }
    }
}
