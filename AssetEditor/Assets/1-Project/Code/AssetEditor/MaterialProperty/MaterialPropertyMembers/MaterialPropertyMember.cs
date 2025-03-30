using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Merlin
{
    public abstract class MaterialPropertyMember<T> : MonoBehaviour, IMaterialPropertyMember
    {
        [SerializeField] protected TexturePropertyIcon texProp;
        [SerializeField] protected TMP_Text label;

        protected Material mat;
        protected string propertyName;
        private UnityEvent<T> onValueChanged;

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

        protected void Initialize(string label, Material mat, T value, string propName, UnityAction<T> onValueChanged)
        {
            this.label.text = $"{label}";
            this.mat = mat;
            originalValue = value;
            currentValue = value;
            propertyName = propName;

            if (onValueChanged != null)
            {
                this.onValueChanged = new();
                this.onValueChanged.AddListener(onValueChanged);
            }

            texProp.gameObject.SetActive(false);
        }

        public virtual void ResetProperty()
        {
            currentValue = originalValue;
        }
    }
}