using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Merlin
{
    public abstract class MaterialPropertyMember<T> : MonoBehaviour, IMaterialPropertyMember
    {
        [SerializeField] protected TexturePropertyIcon texProp;
        [SerializeField] protected TMP_Text title;

        protected Material mat;
        protected string propertyName;

        private T originalValue;
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
            originalValue = value;
            currentValue = value;

            texProp.gameObject.SetActive(false);
        }

        public void OnValueChanged(UnityAction<T> onValueChanged)
        {
            this.onValueChanged = new();
            this.onValueChanged.AddListener(onValueChanged);
        }

        public virtual void ResetProperty()
        {
            currentValue = originalValue;
        }
    }
}