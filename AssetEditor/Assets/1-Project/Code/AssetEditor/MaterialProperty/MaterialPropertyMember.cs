using TMPro;
using UnityEngine;

namespace Merlin
{
    public abstract class MaterialPropertyMember<T> : MonoBehaviour
    {
        [SerializeField] protected TextureProperty texProp;
        [SerializeField] protected TMP_Text title;

        protected Material mat;
        protected string propertyName;
        protected T currentValue;

        protected void Initialize(Material mat, string name, T value)
        {
            title.text = $"{name}";

            this.mat = mat;
            propertyName = name;
            currentValue = value;

            texProp.gameObject.SetActive(false);
        }

        public void SetTextureProperty(Material mat, string name, Texture tex)
        {
            texProp.Initialize(mat, name, tex);

            texProp.gameObject.SetActive(true);
        }
    }
}