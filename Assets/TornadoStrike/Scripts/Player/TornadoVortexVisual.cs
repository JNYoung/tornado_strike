using UnityEngine;

namespace TornadoStrike.Player
{
    [DisallowMultipleComponent]
    public sealed class TornadoVortexVisual : MonoBehaviour
    {
        [Header("Motion")]
        public float baseSpinSpeed = 170f;
        public float spinGradient = 34f;
        public float pulseAmount = 0.08f;
        public float pulseSpeed = 2.8f;

        private Transform[] rings = new Transform[0];
        private Vector3[] baseScales = new Vector3[0];

        private void Awake()
        {
            CacheRings();
        }

        private void OnTransformChildrenChanged()
        {
            CacheRings();
        }

        private void Update()
        {
            if (rings.Length == 0)
            {
                return;
            }

            var time = Time.time;
            for (var i = 0; i < rings.Length; i++)
            {
                var ring = rings[i];
                if (ring == null)
                {
                    continue;
                }

                var direction = i % 2 == 0 ? 1f : -1f;
                var spin = (baseSpinSpeed + spinGradient * i) * direction * Time.deltaTime;
                ring.Rotate(0f, spin, 0f, Space.Self);

                var pulse = 1f + Mathf.Sin(time * pulseSpeed + i * 0.7f) * pulseAmount;
                ring.localScale = new Vector3(baseScales[i].x * pulse, baseScales[i].y, baseScales[i].z * pulse);
            }
        }

        private void CacheRings()
        {
            rings = new Transform[transform.childCount];
            baseScales = new Vector3[transform.childCount];

            for (var i = 0; i < transform.childCount; i++)
            {
                rings[i] = transform.GetChild(i);
                baseScales[i] = rings[i].localScale;
            }
        }
    }
}
