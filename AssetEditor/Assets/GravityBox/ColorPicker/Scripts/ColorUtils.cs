using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GravityBox.ColorPicker
{
    /// <summary>
    /// Capture screen pixel color with this tool, needs monobehaviour to run coroutine
    /// Note: invokes onPixelCaptured callback in both cases color is new and color stays same
    /// so check color change manually!
    /// </summary>
    public class ScreenPixelCaptureTool
    {
        private const string screenCaptureShader = "UI/ColorPicker/Pixels";
        private static readonly int _Screen = Shader.PropertyToID(nameof(_Screen));

        private System.Action<Color> onPixelCaptured;

        private MonoBehaviour owner;
        private Texture2D capture;
        private EventSystem eventSystem;
        private Graphic image;
        private Color initialColor;
        private bool showCapture;

        public Color currentColor { get; private set; }

        public void PickColor(MonoBehaviour owner, Graphic image, Color initialColor, System.Action<Color> onPixelCaptured)
        {
            this.owner = owner;
            this.image = image;
            this.initialColor = initialColor;
            this.onPixelCaptured = onPixelCaptured;
            showCapture = CanShowCaptureScreen(image);
            owner.StartCoroutine(CaptureFrame());
        }
        
        bool CanShowCaptureScreen(Graphic image)
        {
            if (image.material != null && image.material.shader != null)
                return image.material.shader.name == screenCaptureShader;

            return false;
        }

        IEnumerator CaptureFrame()
        {
            yield return new WaitForEndOfFrame();
            eventSystem = EventSystem.current;
            eventSystem.enabled = false;

            capture = ScreenCapture.CaptureScreenshotAsTexture();
            capture.filterMode = FilterMode.Point;
            capture.wrapMode = TextureWrapMode.Clamp;

            if (showCapture)
            {
                image.material.mainTexture = capture;
                image.gameObject.SetActive(true);
            }

            while (true)
            {
                Color rgb = capture.GetPixel((int)Input.mousePosition.x, (int)Input.mousePosition.y);
                rgb.a = 1;

                currentColor = rgb;

                if (showCapture)
                    image.material.SetVector(_Screen, new Vector4(Input.mousePosition.x, Input.mousePosition.y, Screen.width, Screen.height));
                else
                    image.color = rgb;

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    EndCapture();
                    onPixelCaptured.Invoke(initialColor);
                }
                if (Input.GetMouseButtonDown(0))
                {
                    EndCapture();
                    onPixelCaptured.Invoke(currentColor);
                }
                yield return null;
            }
        }

        void EndCapture()
        {
            eventSystem.enabled = true;
            owner.StopAllCoroutines();

            if (showCapture)
                image.gameObject.SetActive(false);

            Object.Destroy(capture);
        }
    }

    /// <summary>
    /// Extension methods for Color class to make it simple converting between Color spaces, applying HDR
    /// and restoring HDR intensity back
    /// </summary>
    public static class ColorExtensions
    {
        public static Color ToHSV(this Color rgba)
        {
            Color result;
            Color.RGBToHSV(rgba, out result.r, out result.g, out result.b);
            result.a = rgba.a;
            return result;
        }

        public static Color ToRGB(this Color hsva)
        {
            Color result = Color.HSVToRGB(hsva.r, hsva.g, hsva.b);
            result.a = hsva.a;
            return result;
        }

        public static Color ToHDRColor(this Color color, float intensity)
        {
            float exposure = Mathf.Pow(2, intensity);
            color.r *= exposure;
            color.g *= exposure;
            color.b *= exposure;
            return color;
        }

        public static float GetLuminance(this Color color)
        {
            return (color.r * 0.3f) + (color.g * 0.59f) + (color.b * 0.11f);
        }

        private const int k_MaxByteForOverexposedColor = 191;

        public static void DecomposeHDR(this Color colorHDR, out Color32 baseColor, out float exposure)
        {
            baseColor = colorHDR;
            var maxColorComponent = colorHDR.maxColorComponent;
            // replicate Photoshops's decomposition behaviour
            if (maxColorComponent == 0f || maxColorComponent <= 1f && maxColorComponent > 1 / 255f)
            {
                exposure = 0f;

                baseColor.r = (byte)Mathf.RoundToInt(colorHDR.r * 255f);
                baseColor.g = (byte)Mathf.RoundToInt(colorHDR.g * 255f);
                baseColor.b = (byte)Mathf.RoundToInt(colorHDR.b * 255f);
            }
            else
            {
                // calibrate exposure to the max float color component
                var scaleFactor = k_MaxByteForOverexposedColor / maxColorComponent;
                exposure = Mathf.Log(255f / scaleFactor) / Mathf.Log(2f);

                //Debug.Log($"Color32 {Mathf.Log(255f / scaleFactor)} / {Mathf.Log(2f)} = {exposure}");

                // maintain maximal integrity of byte values to prevent off-by-one errors when scaling up a color one component at a time
                baseColor.r = (byte)Mathf.Min(k_MaxByteForOverexposedColor, Mathf.CeilToInt(scaleFactor * colorHDR.r));
                baseColor.g = (byte)Mathf.Min(k_MaxByteForOverexposedColor, Mathf.CeilToInt(scaleFactor * colorHDR.g));
                baseColor.b = (byte)Mathf.Min(k_MaxByteForOverexposedColor, Mathf.CeilToInt(scaleFactor * colorHDR.b));
            }
        }

        private const float k_MaxValueForOverexposedColor = 0.749f;

        public static void DecomposeHDR(this Color colorHDR, out Color baseColor, out float exposure)
        {
            baseColor = colorHDR;
            var maxColorComponent = colorHDR.maxColorComponent;

            if (maxColorComponent == 0f || maxColorComponent <= 1f && maxColorComponent > 1 / 255f)
            {
                exposure = 0f;
                baseColor = colorHDR;
            }
            else
            {
                var scaleFactor = k_MaxValueForOverexposedColor / maxColorComponent;
                exposure = Mathf.Log(1f / scaleFactor) / Mathf.Log(2f);

                //Debug.Log($"Color01 {Mathf.Log(1f / scaleFactor)} / {Mathf.Log(2f)} = {exposure}");
                
                baseColor.r = Mathf.Min(k_MaxValueForOverexposedColor, scaleFactor * colorHDR.r);
                baseColor.g = Mathf.Min(k_MaxValueForOverexposedColor, scaleFactor * colorHDR.g);
                baseColor.b = Mathf.Min(k_MaxValueForOverexposedColor, scaleFactor * colorHDR.b);
            }
        }
    }
}