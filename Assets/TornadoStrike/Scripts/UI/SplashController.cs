using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TornadoStrike.UI
{
    [DisallowMultipleComponent]
    public sealed class SplashController : MonoBehaviour
    {
        public string nextSceneName = "MainMenu";
        [Min(0f)] public float minimumDuration = 2.2f;
        public CanvasGroup fadeGroup;
        [Min(0.01f)] public float fadeDuration = 0.45f;

        private IEnumerator Start()
        {
            if (fadeGroup != null)
            {
                fadeGroup.alpha = 1f;
                yield return Fade(1f, 0f);
            }

            yield return new WaitForSeconds(minimumDuration);

            if (fadeGroup != null)
            {
                yield return Fade(0f, 1f);
            }

            SceneManager.LoadScene(nextSceneName);
        }

        private IEnumerator Fade(float from, float to)
        {
            var elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }

            fadeGroup.alpha = to;
        }
    }
}
