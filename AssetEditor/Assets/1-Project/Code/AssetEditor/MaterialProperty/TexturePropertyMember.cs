using UnityEngine;

namespace Merlin
{
    public class TexturePropertyMember : MaterialPropertyMember<Texture>
    {
        public new void Initialize(Material mat, string name, Texture value)
        {
            base.Initialize(mat, name, value);

            texProp.Initialize(mat, name, value);
            texProp.gameObject.SetActive(true);
        }
    }
}