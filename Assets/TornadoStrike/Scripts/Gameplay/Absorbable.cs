using System.Collections;
using UnityEngine;

namespace TornadoStrike.Gameplay
{
    public enum AbsorbableCategory
    {
        Prop,
        Vehicle,
        Building,
        SpecialBuilding,
        Nature
    }

    [DisallowMultipleComponent]
    public sealed class Absorbable : MonoBehaviour
    {
        [Header("Identity")]
        public string absorbableId = "city_prop";
        public AbsorbableCategory category = AbsorbableCategory.Prop;
        public string localizationKey = "object_generic";

        [Header("Progression")]
        [Min(0.1f)] public float requiredRadius = 1f;
        [Min(0f)] public float growthValue = 0.03f;
        [Min(0)] public int scoreValue = 1;

        [Header("Slot")]
        public bool isSpecialSlot;
        public string slotKey;

        [Header("Feedback")]
        [SerializeField] private float consumeDuration = 0.18f;

        public bool IsConsumed { get; private set; }

        public void Consume(Transform collector)
        {
            if (IsConsumed)
            {
                return;
            }

            IsConsumed = true;
            StartCoroutine(ConsumeRoutine(collector));
        }

        private IEnumerator ConsumeRoutine(Transform collector)
        {
            var startPosition = transform.position;
            var startScale = transform.localScale;
            var elapsed = 0f;

            while (elapsed < consumeDuration)
            {
                elapsed += Time.deltaTime;
                var t = consumeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / consumeDuration);
                var eased = 1f - Mathf.Pow(1f - t, 3f);
                var target = collector != null ? collector.position + Vector3.up * 0.2f : startPosition;

                transform.position = Vector3.Lerp(startPosition, target, eased);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, eased);
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
