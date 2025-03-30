using UnityEngine;

namespace Merlin
{
    public class TexturePropertyMember : MaterialPropertyMember<Texture>
    {
        public new void Initialize(string label, Material mat, Texture value, string propName)
        {
            base.Initialize(label, mat, value, propName);

            texProp.Initialize(value);
            texProp.OnClick.AddListener(() =>
            {
                AssetWindow.Show<Texture>(transform, tex =>
                {
                    CurrentValue = tex;
                    texProp.SetTextureIcon(tex);
                    mat.SetTexture(propName, tex);
                });
            });

            texProp.gameObject.SetActive(true);
        }

        public override void UpdateUI()
        {
            base.UpdateUI();

            texProp.SetTextureIcon(CurrentValue);
        }
    }
}