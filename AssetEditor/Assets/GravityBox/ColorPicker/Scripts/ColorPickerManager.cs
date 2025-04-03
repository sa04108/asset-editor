using UnityEngine;

namespace GravityBox.ColorPicker
{
    /// <summary>
    /// Scriptable Object to control Color Picker window
    /// and simplify interfacing with it
    /// will later move loading color presets here (and multiple libraries support)
    /// and saving window position and other settings here
    /// </summary>
    public class ColorPickerManager : ScriptableObject
    {
        [SerializeField]
        private ColorPickerWindow colorPickerPrefab;

        private ColorPickerWindow sceneColorPicker;
        private GameObject colorPickerGameObject;

        /// <summary>
        /// General method to call picker window and use it to update object's color
        /// </summary>
        /// <param name="color">Initial color of object</param>
        /// <param name="hasAlpha">Used if color picker should control color's alpha</param>
        /// <param name="isHDR">Used if color picker should control colors intensity (exposure)</param>
        /// <param name="onColorUpdated">Callback method to update object's color</param>
        public void Show(Color color, bool hasAlpha, bool isHDR, System.Action<Color> onColorUpdated)
        {
            if (colorPickerGameObject == null)
            {
                sceneColorPicker = Instantiate(colorPickerPrefab);
                colorPickerGameObject = sceneColorPicker.gameObject;
            }
            else
                colorPickerGameObject.SetActive(true);

            sceneColorPicker.onColorUpdated = null;
            sceneColorPicker.Show(color, hasAlpha, isHDR);
            sceneColorPicker.onColorUpdated += onColorUpdated;
        }

        /// <summary>
        /// Simplified method for color buttons
        /// </summary>
        /// <param name="colorButton"></param>
        public void Show(ColorButton colorButton)
        {
            Show(colorButton.color, colorButton.hasAlpha, colorButton.isHDR, (color) => colorButton.color = color);
        }
    }
}