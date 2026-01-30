using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyUI
{
    public class MaterialElementUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Color selectedColor, idleColor;
        [SerializeField] private Image image;

        public int index { get; set; }

        private string _text;
        public string text
        {
            get => _text;
            set
            {
                _text = value;
                inputField.SetTextWithoutNotify(_text);
            }
        }

        public event Action<int> onSelected;
        public event Action<string> onTextChanged;

        private static MaterialElementUI lastSelected;

        private void Start()
        {
            inputField.onValueChanged.AddListener(OnValueChanged);
            inputField.onSelect.AddListener(InvokeOnSelected);
        }

        public void SelectWithoutNotify()
        {
            if (lastSelected)
                lastSelected.image.color = idleColor;
            image.color = selectedColor;
            lastSelected = this;
        }

        // called from ui

        private void InvokeOnSelected(string _)
        {
            SelectWithoutNotify();
            onSelected?.Invoke(index);
        }

        private void OnValueChanged(string txt)
        {
            _text = txt;
            onTextChanged?.Invoke(txt);
        }
    }
}