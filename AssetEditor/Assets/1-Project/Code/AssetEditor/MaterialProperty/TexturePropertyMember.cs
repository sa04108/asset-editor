using UnityEngine;

namespace Merlin
{
    public class TexturePropertyMember : MaterialPropertyMember<Texture>
    {
        public override void Initialize(Material mat, MaterialPropertyType type, string name, Texture value)
        {
            base.Initialize(mat, type, name, value);

            texProp.Initialize(mat, name, value);
            texProp.gameObject.SetActive(true);
        }
    }
}