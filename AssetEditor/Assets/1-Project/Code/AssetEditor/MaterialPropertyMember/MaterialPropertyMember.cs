using TMPro;
using UnityEngine;

namespace Merlin
{
    public abstract class MaterialPropertyMember<T> : MonoBehaviour
    {
        [SerializeField] protected TexturePropertyMember texMember;
        [SerializeField] protected TMP_Text title;

        protected Material mat;
        protected MaterialPropertyType type;
        protected string propertyName;
        protected T currentValue;

        public virtual void Initialize(Material mat, MaterialPropertyType type, string name, T value)
        {
            title.text = $"{name}";

            this.mat = mat;
            this.type = type;
            propertyName = name;
            currentValue = value;

            texMember.gameObject.SetActive(false);
        }

        public void InitializeWithTexture(Material mat, MaterialPropertyType type, string name, T value, Texture tex)
        {
            Initialize(mat, type, name, value);
            texMember.Initialize(mat, MaterialPropertyType.Texture, name, tex);

            texMember.gameObject.SetActive(true);
        }
    }
}