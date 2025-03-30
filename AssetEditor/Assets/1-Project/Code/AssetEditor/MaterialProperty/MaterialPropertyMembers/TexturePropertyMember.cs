using UnityEngine;
using UnityEngine.Events;

namespace Merlin
{
    public class TexturePropertyMember : MaterialPropertyMember<Texture>
    {
        public new void Initialize(string label, Material mat, Texture value, string propName, UnityAction<Texture> onValueChanged)
        {
            base.Initialize(label, mat, value, propName, onValueChanged);

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

        public override void ResetProperty()
        {
            base.ResetProperty();

            texProp.SetTextureIcon(CurrentValue);
        }
    }
}