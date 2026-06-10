using TornadoStrike.Localization;
using TornadoStrike.Gameplay;
using TornadoStrike.Player;
using UnityEngine;
using UnityEngine.UI;

namespace TornadoStrike.UI
{
    [DisallowMultipleComponent]
    public sealed class LevelProgressHud : MonoBehaviour
    {
        public TornadoGrowth tornado;
        public int targetScore = TornadoBalanceRules.DefaultTargetScore;
        public float levelDurationSeconds = TornadoBalanceRules.DefaultRoundSeconds;

        [Header("Labels")]
        public Text scoreText;
        public Text radiusText;
        public Text timerText;
        public GameObject completionPanel;

        private float remainingSeconds;
        private bool completed;

        private void Awake()
        {
            remainingSeconds = levelDurationSeconds;
            if (tornado == null)
            {
                tornado = FindObjectOfType<TornadoGrowth>();
            }
        }

        private void OnEnable()
        {
            if (tornado != null)
            {
                tornado.ScoreChanged += OnScoreChanged;
                tornado.RadiusChanged += OnRadiusChanged;
            }

            LocalizationService.LanguageChanged += RefreshAll;
            RefreshAll();
        }

        private void OnDisable()
        {
            if (tornado != null)
            {
                tornado.ScoreChanged -= OnScoreChanged;
                tornado.RadiusChanged -= OnRadiusChanged;
            }

            LocalizationService.LanguageChanged -= RefreshAll;
        }

        private void Update()
        {
            if (completed)
            {
                return;
            }

            remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);
            RefreshTimer();

            if (remainingSeconds <= 0f || (tornado != null && tornado.Score >= targetScore))
            {
                Complete();
            }
        }

        private void OnScoreChanged(int score)
        {
            RefreshScore();
            if (score >= targetScore)
            {
                Complete();
            }
        }

        private void OnRadiusChanged(float radius)
        {
            RefreshRadius();
        }

        private void RefreshAll()
        {
            RefreshScore();
            RefreshRadius();
            RefreshTimer();

            if (completionPanel != null)
            {
                completionPanel.SetActive(completed);
            }
        }

        private void RefreshScore()
        {
            if (scoreText == null)
            {
                return;
            }

            var service = LocalizationService.Instance;
            var score = tornado != null ? tornado.Score : 0;
            scoreText.text = service != null ? service.Format("hud_score", score, targetScore) : $"Score {score}/{targetScore}";
        }

        private void RefreshRadius()
        {
            if (radiusText == null)
            {
                return;
            }

            var service = LocalizationService.Instance;
            var radius = tornado != null ? tornado.CurrentRadius : 0f;
            radiusText.text = service != null ? service.Format("hud_radius", radius.ToString("0.0")) : $"Radius {radius:0.0}";
        }

        private void RefreshTimer()
        {
            if (timerText == null)
            {
                return;
            }

            var service = LocalizationService.Instance;
            var seconds = Mathf.CeilToInt(remainingSeconds);
            timerText.text = service != null ? service.Format("hud_timer", seconds) : $"Time {seconds}s";
        }

        private void Complete()
        {
            if (completed)
            {
                return;
            }

            completed = true;
            if (completionPanel != null)
            {
                completionPanel.SetActive(true);
            }
        }
    }
}
