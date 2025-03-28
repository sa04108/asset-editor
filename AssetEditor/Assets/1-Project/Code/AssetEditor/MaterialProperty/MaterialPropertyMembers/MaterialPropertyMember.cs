using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Merlin
{
    public abstract class MaterialPropertyMember<T> : MonoBehaviour
    {
        [SerializeField] protected TextureProperty texProp;
        [SerializeField] protected TMP_Text title;

        protected Material mat;
        protected string propertyName;

        private T currentValue;
        protected T CurrentValue
        {
            get => currentValue;
            set
            {
                currentValue = value;
                onValueChanged?.Invoke(currentValue);
            }
        }
        private UnityEvent<T> onValueChanged;

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

        public void OnValueChanged(UnityAction<T> onValueChanged)
        {
            this.onValueChanged = new();
            this.onValueChanged.AddListener(onValueChanged);
        }
    }
}