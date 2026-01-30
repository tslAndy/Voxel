using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyUI.Props
{
    public class MaterialPropUI : MonoBehaviour
    {
        [SerializeField] private Mode mode;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_InputField inputField;

        private string _format;

        public event Action<float> onValueChanged;

        private float _val;
        public float val
        {
            get => _val;
            set
            {
                _val = value;
                slider.SetValueWithoutNotify(val);
                inputField.SetTextWithoutNotify(val.ToString(_format));
            }
        }

        private void Start()
        {
            slider.onValueChanged.AddListener(UpdateInputField);
            inputField.onValueChanged.AddListener(UpdateSlider);

            _format = mode == Mode.Float ? "0.00" : "0";
        }

        // called from ui
        public void UpdateSlider(string uiVal)
        {
            val = Convert.ToSingle(uiVal);
            onValueChanged?.Invoke(val);
        }

        public void UpdateInputField(float uiVal)
        {
            val = uiVal;
            onValueChanged?.Invoke(val);
        }

        public enum Mode
        {
            Float,
            Integer
        }
    }
}