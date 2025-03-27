using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Merlin
{
    public class AssetWindow : MonoBehaviour
    {
        #region Static Fields

        private static AssetWindow source;
        private static Dictionary<System.Type, AssetWindow> instances = new();

        #endregion Static Fields

        private Transform owner;
        private UnityEvent<Object> onValueChangedEvent = new();
        private UnityEvent onUnsubscribeEvent = new();

        [SerializeField]
        private string startType;

        [SerializeField]
        private TMP_Text title;

        [SerializeField]
        private AssetWindowItem itemPreset;

        [SerializeField]
        private Transform itemParent;

        private List<AssetWindowItem> items = new();

        public static AssetWindow Get<T>(Transform subscriber, UnityAction<T> onValueChanged, UnityAction onUnsubscribe = null) where T : Object
        {
            var instance = GetInstance<T>();

            if (subscriber != null && instance.owner == subscriber)
            {
                instance.gameObject.SetActive(!instance.gameObject.activeSelf);
                return instance;
            }

            if (instance.items.Count == 0)
            {
                Debug.LogWarning("You must assign assets before open asset window.");
                return instance;
            }

            instance.Subscribe(subscriber, onValueChanged, onUnsubscribe);

            return instance;
        }

        public static AssetWindow Get<T>(Transform subscriber, T[] values, UnityAction<T> onValueChanged, UnityAction onUnsubscribe = null) where T : Object
        {
            var instance = GetInstance<T>();

            if (subscriber != null && instance.owner == subscriber)
            {
                instance.gameObject.SetActive(!instance.gameObject.activeSelf);
                return instance;
            }

            if (values.Length == 0)
            {
                Debug.LogWarning("Assets to be shown are empty.");
                return instance;
            }

            instance.Subscribe(subscriber, onValueChanged, onUnsubscribe);
            instance.SetValues(values);

            return instance;
        }

        private static AssetWindow GetInstance<T>()
        {
            if (!instances.ContainsKey(typeof(T)))
            {
                if (source == null)
                {
                    source = FindAnyObjectByType<AssetWindow>();
                    source.startType = "";
                }

                var instance = Instantiate(source, source.transform.parent);
                instance.name = $"{typeof(T).Name}Window";
                instance.title.text = typeof(T).Name;
                instance.transform.localPosition = Vector3.zero;
                instance.gameObject.SetActive(true);

                instances.Add(typeof(T), instance);
            }

            return instances[typeof(T)];
        }

        private void Awake()
        {
            if (!string.IsNullOrEmpty(startType))
            {
                System.Type thisType = System.Type.GetType($"{startType}, UnityEngine");
                instances.Add(thisType, this);
                title.text = thisType.Name;
            }
        }

        private void Subscribe<T>(Transform subscriber, UnityAction<T> onValueChanged, UnityAction onUnsubscribe) where T : Object
        {
            onUnsubscribeEvent.Invoke();
            onUnsubscribeEvent.RemoveAllListeners();
            if (onUnsubscribe != null)
            {
                onUnsubscribeEvent.AddListener(onUnsubscribe);
            }

            owner = subscriber;
            gameObject.SetActive(true);

            onValueChangedEvent.RemoveAllListeners();
            onValueChangedEvent.AddListener(obj =>
            {
                if (subscriber != null)
                    onValueChanged?.Invoke((T)obj);
            });
        }

        private void SetValues<T>(T[] values) where T : Object
        {
            if (items.Count > 0)
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    Destroy(items[i].gameObject);
                }

                items.Clear();
            }

            foreach (T value in values)
            {
                var item = Instantiate(itemPreset, itemParent);
                items.Add(item);

                if (value is Texture tex)
                {
                    item.Icon.texture = tex;
                }
                else if (value is Material mat)
                {
                    item.Icon.texture = mat.mainTexture;
                }

                item.Label.text = value.name;
                item.gameObject.SetActive(true);

                item.OnClick.AddListener(() =>
                {
                    onValueChangedEvent.Invoke(value);
                });
            }
        }
    }
}