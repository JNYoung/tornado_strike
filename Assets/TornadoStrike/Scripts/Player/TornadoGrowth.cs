using System;
using TornadoStrike.Gameplay;
using UnityEngine;

namespace TornadoStrike.Player
{
    [DisallowMultipleComponent]
    public sealed class TornadoGrowth : MonoBehaviour
    {
        [Header("Growth")]
        [Min(0.5f)] public float currentRadius = 1.25f;
        [Min(1f)] public float maxRadius = 8f;
        [Min(0f)] public float radiusTolerance = 0.05f;

        [Header("References")]
        public Transform visualRoot;
        public SphereCollider absorptionTrigger;
        public ParticleSystem swirlFx;

        public int Score { get; private set; }
        public float CurrentRadius => currentRadius;

        public event Action<int> ScoreChanged;
        public event Action<float> RadiusChanged;
        public event Action<Absorbable> Absorbed;

        private void Reset()
        {
            absorptionTrigger = GetComponent<SphereCollider>();
        }

        private void Awake()
        {
            if (absorptionTrigger == null)
            {
                absorptionTrigger = GetComponent<SphereCollider>();
            }

            ApplyScale();
        }

        private void OnValidate()
        {
            currentRadius = Mathf.Clamp(currentRadius, 0.5f, maxRadius);
            if (Application.isPlaying)
            {
                ApplyScale();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            var absorbable = other.GetComponentInParent<Absorbable>();
            if (absorbable == null || absorbable.IsConsumed)
            {
                return;
            }

            if (!CanAbsorb(absorbable))
            {
                return;
            }

            Absorb(absorbable);
        }

        public bool CanAbsorb(Absorbable absorbable)
        {
            return absorbable != null && currentRadius + radiusTolerance >= absorbable.requiredRadius;
        }

        public void Absorb(Absorbable absorbable)
        {
            if (absorbable == null || absorbable.IsConsumed)
            {
                return;
            }

            absorbable.Consume(transform);
            Score += absorbable.scoreValue;
            currentRadius = Mathf.Min(maxRadius, currentRadius + absorbable.growthValue);

            ApplyScale();
            ScoreChanged?.Invoke(Score);
            RadiusChanged?.Invoke(currentRadius);
            Absorbed?.Invoke(absorbable);
        }

        private void ApplyScale()
        {
            if (absorptionTrigger != null)
            {
                absorptionTrigger.isTrigger = true;
                absorptionTrigger.radius = currentRadius;
            }

            if (visualRoot != null)
            {
                var height = Mathf.Lerp(1.1f, 2.2f, Mathf.InverseLerp(1f, maxRadius, currentRadius));
                visualRoot.localScale = new Vector3(currentRadius, height, currentRadius);
            }

            if (swirlFx != null)
            {
                var main = swirlFx.main;
                main.startSize = Mathf.Lerp(0.25f, 1.2f, Mathf.InverseLerp(1f, maxRadius, currentRadius));

                var shape = swirlFx.shape;
                shape.radius = currentRadius * 0.45f;
            }
        }
    }
}
