using ChunksRendering;
using UnityEngine;

namespace MyUI.Props
{
    public class MaterialPropBlockUI : MonoBehaviour
    {
        [SerializeField] private MaterialPropUI metallic, smooth, emission;
        [SerializeField] private ColorPropUI color;

        private MatProp _matProp;
        public MatProp matProp
        {
            get => _matProp;
            set
            {
                _matProp = value;

                metallic.val = _matProp.metallic;
                smooth.val = _matProp.glossiness;
                emission.val = _matProp.emission;
                color.val = _matProp.color;
            }
        }

        private void Start()
        {
            metallic.onValueChanged += UpdateMetallic;
            smooth.onValueChanged += UpdateSmooth;
            emission.onValueChanged += UpdateEmission;
            color.onValueChanged += UpdateColor;
        }

        // called from ui

        private void UpdateMetallic(float val) => matProp.metallic = val;
        private void UpdateSmooth(float val) => matProp.glossiness = val;
        private void UpdateEmission(float val) => matProp.emission = val;
        private void UpdateColor(Color val) => matProp.color = val;
    }
}
