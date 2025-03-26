using TMPro;
using UnityEngine;

namespace Merlin
{
    public abstract class MaterialPropertyMember<T> : MonoBehaviour
    {
        [SerializeField] protected TextureProperty texProp;
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

            texProp.gameObject.SetActive(false);
        }

        public void InitializeWithTexture(Material mat, MaterialPropertyType type, string name, T value, Texture tex)
        {
            Initialize(mat, type, name, value);
            texProp.Initialize(mat, name, tex);

            texProp.gameObject.SetActive(true);
        }
    }
}