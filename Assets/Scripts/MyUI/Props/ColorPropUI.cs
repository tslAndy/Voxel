using System;
using UnityEngine;
using UnityEngine.UI;

namespace MyUI.Props
{
    public class ColorPropUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject colorSelector;

        [SerializeField]
        private Image mainColorImage, colorPreviewImage;

        [SerializeField]
        private MaterialPropUI red, green, blue, alpha;

        public event Action<Color> onValueChanged;

        private Color _val;
        public Color val
        {
            get => _val;
            set
            {
                _val = value;

                red.val = _val.r * 256.0f;
                green.val = _val.g * 256.0f;
                blue.val = _val.b * 256.0f;
                alpha.val = _val.a * 256.0f;

                colorPreviewImage.color = _val;
                mainColorImage.color = _val;
            }
        }

        private void Start()
        {
            button.onClick.AddListener(OnButtonClicked);

            red.onValueChanged += OnValueChanged;
            green.onValueChanged += OnValueChanged;
            blue.onValueChanged += OnValueChanged;
            alpha.onValueChanged += OnValueChanged;
        }

        // called from ui

        private void OnButtonClicked() => colorSelector.SetActive(!colorSelector.activeSelf);

        private void OnValueChanged(float _)
        {
            _val.r = red.val * 0.00390625f;
            _val.g = green.val * 0.00390625f;
            _val.b = blue.val * 0.00390625f;
            _val.a = alpha.val * 0.00390625f;

            // TODO IMPORTANT check if transparency by 256 is over 0.99

            colorPreviewImage.color = _val;
            mainColorImage.color = _val;

            onValueChanged?.Invoke(_val);
        }
    }
}
