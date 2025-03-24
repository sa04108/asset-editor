using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.ColorPicker
{
    public class ColorImage : MaskableGraphic
    {
        [ColorUsage(true, true)]
        public Color hdrColor;

        [SerializeField]
        private bool _hasAlpha;
        [SerializeField]
        private bool _isHDR;
        [SerializeField]
        private int alphaIndicatorHeight = 4;

        private bool _isCapturing;
        private GameObject _hdrText;

        public GameObject hdrText
        {
            get
            {
                if (_hdrText == null)
                    _hdrText = transform.GetChild(0).gameObject;

                return _hdrText;
            }
        }

        public Color colorValue { get => hdrColor; set { hdrColor = value; SetVerticesDirty(); } }

        public override Color color
        {
            get => _isCapturing ? hdrColor : base.color;
            set
            {
                if (_isCapturing)
                {
                    hdrColor = value; SetVerticesDirty();
                }
                else
                    base.color = value;
            }
        }

        public bool hasAlpha { get => _hasAlpha; set { _hasAlpha = value; SetVerticesDirty(); } }
        public bool isHDR { get => _isHDR; set { _isHDR = value; SetVerticesDirty(); } }
        public float intensity { get { hdrColor.DecomposeHDR(out Color _color, out float _intensity); return _intensity; } }

        protected ColorImage()
        {
            useLegacyMeshGeneration = false;
        }

        public void SetColor(Color color, float intensity, bool showAlpha, bool showHDR)
        {
            colorValue = showHDR && intensity > 0 ? color.ToHDRColor(intensity) : color;
            hasAlpha = showAlpha;
            isHDR = showHDR;
        }

        public void SetColor(Color color, bool showAlpha)
        {
            SetColor(color, 0, showAlpha, isHDR);
        }

        public void SetColor(Color color)
        {
            SetColor(color, 0, hasAlpha, isHDR);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = GetPixelAdjustedRect();

            //Color32 c1 = color;
            //c1.a = 255 * color.a;
            //Color32 c2 = color.ToHDRColor(hdrIntensity);
            //c2.a = 255 * color.a;

            hdrColor.DecomposeHDR(out Color c1, out float intensity);
            c1.a = color.a;
            Color c2 = _isHDR ? hdrColor : c1;
            c2.a = color.a;

            float alpha = _hasAlpha ? alphaIndicatorHeight : 0;

            var p0 = new Vector3(r.x, r.y + alpha);
            var p1 = new Vector3(r.x, r.y + r.height);
            var p2 = new Vector3(r.x + r.width / 2f, r.y + r.height);
            var p3 = new Vector3(r.x + r.width / 2f, r.y + alpha);
            var p4 = new Vector3(r.x + r.width, r.y + r.height);
            var p5 = new Vector3(r.x + r.width, r.y + alpha);
            {
                vh.AddVert(p0, c1, new Vector2(0, 0)); //0
                vh.AddVert(p1, c1, new Vector2(0, 0)); //1
                vh.AddVert(p2, c2, new Vector2(0, 0)); //2
                vh.AddVert(p3, c2, new Vector2(0, 0)); //3            
                vh.AddVert(p4, c1, new Vector2(0, 0)); //4
                vh.AddVert(p5, c1, new Vector2(0, 0)); //5

                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
                vh.AddTriangle(3, 2, 4);
                vh.AddTriangle(4, 5, 3);
            }

            if (_hasAlpha)
            {
                Color white = Color.white;
                white.a = color.a;
                Color black = Color.black;
                black.a = color.a;

                var p6 = new Vector3(r.x, r.y);
                var p7 = new Vector3(r.x, r.y + alpha);
                var p8 = new Vector3(r.x + r.width * hdrColor.a, r.y + alpha);
                var p9 = new Vector3(r.x + r.width * hdrColor.a, r.y);
                var p10 = p9;
                var p11 = p8;
                var p12 = new Vector3(r.x + r.width, r.y + alpha);
                var p13 = new Vector3(r.x + r.width, r.y);
                {
                    vh.AddVert(p6, Color.white, new Vector2(0, 0)); //0
                    vh.AddVert(p7, Color.white, new Vector2(0, 0)); //1
                    vh.AddVert(p8, Color.white, new Vector2(0, 0)); //2
                    vh.AddVert(p9, Color.white, new Vector2(0, 0)); //3

                    vh.AddTriangle(6, 7, 8);
                    vh.AddTriangle(8, 9, 6);

                    vh.AddVert(p10, Color.black, new Vector2(0, 0)); //2
                    vh.AddVert(p11, Color.black, new Vector2(0, 0)); //3
                    vh.AddVert(p12, Color.black, new Vector2(0, 0)); //2
                    vh.AddVert(p13, Color.black, new Vector2(0, 0)); //3

                    vh.AddTriangle(10, 11, 12);
                    vh.AddTriangle(12, 13, 10);
                }
            }
        }

        public void StartCapture() => _isCapturing = true;

        public void StopCapture() => _isCapturing = false;

        protected override void OnValidate()
        {
            if (hdrText != null)
                hdrText.SetActive(isHDR);

            base.OnValidate();
        }
    }
}