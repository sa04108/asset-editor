using UnityEngine;
using UnityEngine.Events;

namespace Merlin
{
    public abstract class RuntimeWindow<T> : MonoBehaviour
    {
        protected static RuntimeWindow<T> instance;
        protected Transform owner;
        protected UnityEvent<T> onValueChanged = new();

        protected virtual void Start()
        {
            instance = this;
            gameObject.SetActive(false);
        }

        public static void SetOwner(Transform _owner, UnityAction<T> onValueChanged)
        {
            if (instance == null)
            {
                Debug.LogWarning("instance is not created");
                return;
            }

            if (instance.owner == _owner)
            {
                instance.gameObject.SetActive(!instance.gameObject.activeSelf);
            }
            else
            {
                instance.owner = _owner;
                instance.gameObject.SetActive(true);

                instance.onValueChanged.RemoveAllListeners();
                instance.onValueChanged.AddListener(onValueChanged);
            }
        }

        //public static void SetValue(T value)
        //{
        //    instance.SetInstanceValue(value);
        //}

        //public abstract void SetInstanceValue(T value);
    }
}