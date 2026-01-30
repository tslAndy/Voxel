using UnityEngine;

namespace ChunksRendering
{
    public class MatProp
    {
        private MatType _matType;
        private Color _color;
        private float _glossiness, _metallic, _emission;

        private readonly MaterialPropertyBlock _propBlock;

        private static readonly int EmissionColorHash = Shader.PropertyToID("_EmissionColor");
        private static readonly int ColorHash = Shader.PropertyToID("_Color");
        private static readonly int GlossinessHash = Shader.PropertyToID("_Glossiness");
        private static readonly int MetallicHash = Shader.PropertyToID("_Metallic");

        public MatProp()
        {
            _propBlock = new MaterialPropertyBlock();

            ToDefaultValues();
        }

        public void ToDefaultValues()
        {
            color = Color.white;
            emission = 0.0f;
            glossiness = 0.5f;
            metallic = 0.0f;
        }

        public MaterialPropertyBlock propBlock => _propBlock;
        public MatType matType => _matType;

        public Color color
        {
            get => _color;
            set
            {
                _color = value;
                _propBlock.SetColor(ColorHash, _color);

                if (value.a > 0.99f)
                {
                    _matType &= ~MatType.Transparent;
                    _matType |= MatType.Opaque;
                }
                else
                {
                    _matType &= ~MatType.Opaque;
                    _matType |= MatType.Transparent;
                }
            }
        }

        public float emission
        {
            get => _emission;
            set
            {
                _emission = value;
                _propBlock.SetColor(EmissionColorHash, _color * emission);

                if (_emission < 0.00001f)
                    _matType &= ~MatType.Emissive;
                else
                    _matType |= MatType.Emissive;
            }
        }

        public float glossiness
        {
            get => _glossiness;
            set
            {
                _glossiness = value;
                _propBlock.SetFloat(GlossinessHash, _glossiness);
            }
        }

        public float metallic
        {
            get => _metallic;
            set
            {
                _metallic = value;
                _propBlock.SetFloat(MetallicHash, _metallic);
            }
        }
        
        public string name { get; set; }

        public enum MatType
        {
            Opaque = 1,
            Transparent = 2,
            Emissive = 4,
            OpaqueEmissive = Opaque | Emissive,
            TransparentEmissive = Transparent | Emissive
        }
    }
}