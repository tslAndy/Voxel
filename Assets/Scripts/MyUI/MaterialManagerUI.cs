using System;
using System.Collections.Generic;
using ChunksRendering;
using MyUI.Props;
using UnityEngine;
using UnityEngine.UI;

namespace MyUI
{
    public class MaterialManagerUI : MonoBehaviour
    {
        [SerializeField] private Button addButton;

        [SerializeField] private MaterialPropBlockUI propBlockUI;
        [SerializeField] private GameObject materialsList;
        [SerializeField] private MaterialElementUI materialPrefab;
        [SerializeField] private MeshManager meshManager;

        private List<MaterialElementUI> elements = new List<MaterialElementUI>();

        public event Action<int> onSelected;

        private void Start()
        {
            addButton.onClick.AddListener(OnAddButtonClicked);
        }

        public void ActivateElement(int index)
        {
            elements[index - 1].SelectWithoutNotify();
            propBlockUI.matProp = meshManager.GetMatProp(index);
        }

        private void OnAddButtonClicked() => CreateElement();

        private int CreateElement()
        {
            int index = meshManager.CreateMatProp();
            if (index < elements.Count)
                return index;

            MaterialElementUI elem = Instantiate(materialPrefab, materialsList.transform);
            elem.text = meshManager.GetMatProp(index).name;
            elem.index = index;
            elem.onSelected += InvokeOnSelected;
            elem.onTextChanged += OnTextChanged;
            elements.Add(elem);

            return index;
        }

        // called from ui
        private void InvokeOnSelected(int index)
        {
            propBlockUI.matProp = meshManager.GetMatProp(index);
            onSelected?.Invoke(index);
        }
        
        private void OnTextChanged(string txt) => propBlockUI.matProp.name = txt;
    }
}