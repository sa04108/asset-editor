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

        [HideInInspector]
        public UnityEvent<T> OnValueChanged;

        private T originalValue;
        private T currentValue;

        protected T CurrentValue
        {
            get => currentValue;
            set
            {
                currentValue = value;
                OnValueChanged.Invoke(currentValue);
            }
        }

        protected void Initialize(string label, Material mat, T value, string propName)
        {
            this.label.text = $"{label}";
            this.mat = mat;
            originalValue = value;
            currentValue = value;
            propertyName = propName;

            OnValueChanged.RemoveAllListeners();
            texProp.gameObject.SetActive(false);
        }

        public virtual void UpdateUI()
        {
            currentValue = originalValue;
        }
    }
}