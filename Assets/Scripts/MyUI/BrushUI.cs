using Common;
using UnityEngine;
using UnityEngine.UI;

namespace MyUI
{
    public class BrusUI : MonoBehaviour
    {
        [SerializeField] private Brush brush;
        [SerializeField] private Color selectedColor, idleColor;

        [SerializeField] private MaterialManagerUI materialManagerUI;
        [SerializeField] private Button addModeBut, delModeBut, pickModeBut, paintModeBut;

        private Image lastSelected;

        private void Start()
        {
            addModeBut.onClick.AddListener(SetAddMode);
            delModeBut.onClick.AddListener(SetDelMode);
            pickModeBut.onClick.AddListener(SetPickMode);
            paintModeBut.onClick.AddListener(SetPaintMode);

            brush.onPicked += ActivateMaterialUI;
            materialManagerUI.onSelected += ChangeBrushMat;
        }

        private void SetAddMode()
        {
            brush.brushMode = Brush.BrushMode.Add;
            ColorizeButton(addModeBut);
        }

        private void SetDelMode()
        {
            brush.brushMode = Brush.BrushMode.Del;
            ColorizeButton(delModeBut);
        }

        private void SetPickMode()
        {
            brush.brushMode = Brush.BrushMode.Pick;
            ColorizeButton(pickModeBut);
        }

        private void SetPaintMode()
        {
            brush.brushMode = Brush.BrushMode.Paint;
            ColorizeButton(paintModeBut);
        }

        private void ChangeBrushMat(int index) => brush.matIndex = (byte)index;
        private void ActivateMaterialUI(int index) => materialManagerUI.ActivateElement(index);

        private void ColorizeButton(Button button)
        {
            if (lastSelected != null)
                lastSelected.color = idleColor;

            lastSelected = button.gameObject.GetComponent<Image>();
            lastSelected.color = selectedColor;
        }
    }
}