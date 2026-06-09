using UnityEngine;
using UnityEngine.UI;

namespace TornadoStrike.UI
{
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public sealed class AspectCoverImage : MonoBehaviour
    {
        private Image image;
        private RectTransform rectTransform;
        private RectTransform parentRect;

        private void Awake()
        {
            image = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            parentRect = transform.parent as RectTransform;
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        private void Apply()
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (parentRect == null)
            {
                parentRect = transform.parent as RectTransform;
            }

            if (image == null || image.sprite == null || parentRect == null)
            {
                return;
            }

            var parentSize = parentRect.rect.size;
            if (parentSize.x <= 0f || parentSize.y <= 0f)
            {
                return;
            }

            var spriteRect = image.sprite.rect;
            var imageAspect = spriteRect.width / spriteRect.height;
            var parentAspect = parentSize.x / parentSize.y;

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            if (parentAspect > imageAspect)
            {
                rectTransform.sizeDelta = new Vector2(parentSize.x, parentSize.x / imageAspect);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(parentSize.y * imageAspect, parentSize.y);
            }
        }
    }
}
