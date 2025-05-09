using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Merlin
{
    public class AssetWindow : MonoBehaviour
    {
        #region Static Fields

        private static Transform windowParent;
        private static AssetWindow source;
        private static Dictionary<System.Type, AssetWindow> instances = new();

        #endregion Static Fields

        private Transform owner;
        private UnityEvent<Object> onItemSelectedEvent = new();

        [Header("Options")]
        [SerializeField]
        private bool fixPosition;

        [SerializeField]
        private string startType;

        [Header("Links")]
        [SerializeField]
        private TMP_Text title;

        [SerializeField]
        private AssetWindowItem itemPreset;

        [SerializeField]
        private Transform itemParent;

        private List<AssetWindowItem> items = new();

        public static AssetWindow Show<T>(Transform subscriber, UnityAction<T> onItemSelected) where T : Object
        {
            var instance = GetInstance<T>();

            if (subscriber != null && instance.owner == subscriber)
            {
                instance.gameObject.SetActive(true);
                return instance;
            }

            if (instance.items.Count == 0)
            {
                Debug.LogWarning("You must assign assets before open asset window.");
                return instance;
            }

            instance.Subscribe(subscriber, onItemSelected);
            instance.OnShow();

            return instance;
        }

        public static AssetWindow Show<T>(Transform subscriber, T[] values, UnityAction<T> onItemSelected) where T : Object
        {
            var instance = GetInstance<T>();

            if (subscriber != null && instance.owner == subscriber)
            {
                instance.gameObject.SetActive(true);
                return instance;
            }

            if (values.Length == 0)
            {
                Debug.LogWarning("Assets to be shown are empty.");
                return instance;
            }

            instance.Subscribe(subscriber, onItemSelected);
            instance.SetValues(values);
            instance.OnShow();

            return instance;
        }

        public static AssetWindow Set<T>(T[] values) where T : Object
        {
            var instance = GetInstance<T>();

            if (values.Length == 0)
            {
                Debug.LogWarning("Assets to be shown are empty.");
                return instance;
            }

            instance.SetValues(values);

            return instance;
        }

        public static void Hide<T>()
        {
            var instance = GetInstance<T>();
            instance.gameObject.SetActive(false);
        }

        private static AssetWindow GetInstance<T>()
        {
            if (!instances.ContainsKey(typeof(T)))
            {
                if (source == null)
                {
                    source = Resources.Load<AssetWindow>("AssetWindow");
                }

                var instance = Instantiate(source, windowParent);
                instance.name = $"{typeof(T).Name}Window";
                instance.title.text = typeof(T).Name;

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
            }

            if (windowParent == null)
                windowParent = transform.parent;

            gameObject.SetActive(false);
        }

        private void OnShow()
        {
            transform.SetAsLastSibling();

            if (!fixPosition)
                transform.position = Input.mousePosition;

            gameObject.SetActive(true);
        }

        private void Subscribe<T>(Transform subscriber, UnityAction<T> onItemSelected) where T : Object
        {
            owner = subscriber;

            onItemSelectedEvent.RemoveAllListeners();
            onItemSelectedEvent.AddListener(obj =>
            {
                if (subscriber != null)
                    onItemSelected?.Invoke((T)obj);
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
                    onItemSelectedEvent.Invoke(value);
                });
            }
        }

        private void OnDisable()
        {
            owner = null;
            onItemSelectedEvent.RemoveAllListeners();
        }
    }
}