using UnityEngine;

namespace TornadoStrike.Gameplay
{
    public static class TornadoBalanceRules
    {
        public const float MinMvpRoundSeconds = 180f;
        public const float MaxMvpRoundSeconds = 600f;
        public const float DefaultRoundSeconds = 360f;
        public const int DefaultTargetScore = 4200;

        public static bool IsRoundDurationInMvpRange(float seconds)
        {
            return seconds >= MinMvpRoundSeconds && seconds <= MaxMvpRoundSeconds;
        }

        public static float EstimateCompletionSeconds(int targetScore, float scorePerMinute)
        {
            if (targetScore <= 0 || scorePerMinute <= 0f)
            {
                return 0f;
            }

            return targetScore / (scorePerMinute / 60f);
        }

        public static int EstimateChunkScore(int props, int cars, int buses, int houses, int specialBuildings)
        {
            return Mathf.Max(0, props) * 5
                + Mathf.Max(0, cars) * 9
                + Mathf.Max(0, buses) * 28
                + Mathf.Max(0, houses) * 24
                + Mathf.Max(0, specialBuildings) * 90;
        }
    }
}
