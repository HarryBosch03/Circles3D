using Circles3D.Runtime.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Circles3D.Runtime.UI
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UIBar : MonoBehaviour
    {
        [Space]
        [Range(0f, 1f)]
        public float normalizedValue;
        public float maxValue = 100;
        public bool unbounded;
        public string displayName;
        [Range(0f, 1f)]
        public float titleHeight;

        [Space]
        public float minWidth = 300f;
        public float maxWidth = 800f;
        public float nominalMaxValue = 100f;
        public float nominalWidth = 400f;
        public float widthScaling = 1f;

        [Space]
        public string formatString = "{0:N0}/{1:N0}";
        public int textPad = 12;
        public Color hue = Color.red;

        [Space]
        public Color fillReference = new Color(0.69f, 0.13f, 0.13f);
        public Color backgroundReference = new Color(0.36f, 0.03f, 0.03f);

        [Space]
        public Image background;
        public Image deltaFill;
        public Image partialFill;
        public Image fill;
        public TMP_Text text;
        public TMP_Text title;

        public RectTransform rectTransform => transform as RectTransform;

        private void Awake()
        {
            Find(ref background, "Background");
            Find(ref deltaFill, "Delta Fill");
            Find(ref partialFill, "Partial Fill");
            Find(ref fill, "Fill");
            Find(ref text, "Text");
            Find(ref title, "Title");
            
            ForceUpdate();
        }

        private void Find<T>(ref T var, string path) where T : Component
        {
            if (!var) var = transform.Find<T>(path);
            if (var) var.transform.SetAsLastSibling();
        }

        public void SetValue(float absoluteValue, float maxValue)
        {
            this.maxValue = maxValue;
            SetValue(absoluteValue);
        }

        public void SetValue(float absoluteValue) => SetNormalizedValue(maxValue > float.Epsilon ? absoluteValue / maxValue : 0f);

        public void SetNormalizedValue(float normalizedValue)
        {
            this.normalizedValue = unbounded ? normalizedValue : Mathf.Clamp01(normalizedValue);
            ForceUpdate();
        }

        public void ForceUpdate()
        {
            var width = Mathf.Clamp(nominalWidth + (maxValue - nominalMaxValue) * widthScaling, minWidth, maxWidth);
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);

            var value = normalizedValue * maxValue;
            if (partialFill) value = Mathf.Floor(value);

            if (background)
            {
                SetChildSize(background.rectTransform);
                background.color = GetColor(backgroundReference);
            }

            if (deltaFill)
            {
                SetChildSize(deltaFill.rectTransform);
                deltaFill.color = Color.white;
            }

            if (partialFill) SetFill(partialFill, normalizedValue, Color.Lerp(backgroundReference, fillReference, 0.5f));
            if (fill) SetFill(fill, value, maxValue, fillReference);
            if (text) SetupText(text, string.Format(formatString, value, maxValue));
            if (title) SetTitleSize(title, (string.IsNullOrEmpty(displayName) ? name : displayName).ToUpper());
        }

        public void SetFill(Image fill, float percent, Color refColor)
        {
            SetChildSize(fill.rectTransform);
            fill.color = GetColor(refColor);
            fill.fillAmount = percent;
        }
        
        public void SetFill(Image fill, float value, float max, Color refColor) => SetFill(fill, max > float.Epsilon ? value / max : 0f, refColor);

        private void SetTitleSize(TMP_Text text, string content)
        {
            var rectTransform = text.rectTransform;
            
            rectTransform.pivot = Vector2.zero;
            rectTransform.anchorMin = new Vector2(0f, 1f - titleHeight);
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            text.text = content;
        }

        public Color GetColor(Color refColor)
        {
            Color.RGBToHSV(refColor, out _, out var rs, out var rv);
            Color.RGBToHSV(hue, out var h, out _, out _);
            return Color.HSVToRGB(h, rs, rv);
        }

        public void SetupText(TMP_Text text, string content)
        {
            SetChildSize(text.rectTransform, textPad);

            text.enableAutoSizing = true;
            text.fontSizeMin = 1;
            text.fontSizeMax = 128;
            text.alignment = TextAlignmentOptions.Left;
            text.text = content;
        }

        private void SetChildSize(RectTransform rectTransform, int pad = 0)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = new Vector2(1f, 1f - titleHeight);

            rectTransform.sizeDelta = -Vector2.one * pad * 2;
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}