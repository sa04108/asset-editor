using UnityEngine;

namespace Merlin
{
    public class TexturePropertyMember : MaterialPropertyMember<Texture>
    {
        public new void Initialize(Material mat, string name, Texture value)
        {
            base.Initialize(mat, name, value);

            texProp.Initialize(value);
            texProp.OnClick.AddListener(() =>
            {
                AssetWindow.Show<Texture>(transform, tex =>
                {
                    CurrentValue = tex;
                    texProp.SetTextureIcon(tex);
                    mat.SetTexture(name, tex);
                });
            });

            texProp.gameObject.SetActive(true);
        }

        public override void ResetProperty()
        {
            base.ResetProperty();

            texProp.SetTextureIcon(CurrentValue);
        }
    }
}