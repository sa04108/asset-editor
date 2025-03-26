using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Merlin
{
    public class RuntimeAssetWindow : MonoBehaviour
    {
        #region Static Fields

        private static RuntimeAssetWindow source;
        private static Dictionary<System.Type, RuntimeAssetWindow> instances = new();

        #endregion Static Fields

        private Transform owner;
        private UnityEvent<Object> onValueChangedEvent = new();

        [SerializeField]
        private string startType;

        [SerializeField]
        private TMP_Text title;

        [SerializeField]
        private Button elementPreset;

        [SerializeField]
        private Transform elementParent;

        private List<Button> elements = new();

        public static RuntimeAssetWindow Get<T>(Transform _owner, UnityAction<T> onValueChanged) where T : Object
        {
            var instance = GetInstance<T>();

            if (_owner != null && instance.owner == _owner)
            {
                instance.gameObject.SetActive(!instance.gameObject.activeSelf);
                return instance;
            }

            if (instance.elements.Count == 0)
            {
                Debug.LogWarning("You must assign assets before open asset window.");
                return instance;
            }

            instance.Subscribe(_owner, onValueChanged);

            return instance;
        }

        public static RuntimeAssetWindow Get<T>(Transform _owner, T[] values, UnityAction<T> onValueChanged) where T : Object
        {
            var instance = GetInstance<T>();

            if (_owner != null && instance.owner == _owner)
            {
                instance.gameObject.SetActive(!instance.gameObject.activeSelf);
                return instance;
            }

            if (values.Length == 0)
            {
                Debug.LogWarning("Assets to be shown are empty.");
                return instance;
            }

            instance.Subscribe(_owner, onValueChanged);
            instance.SetValues(values);

            return instance;
        }

        private static RuntimeAssetWindow GetInstance<T>()
        {
            if (!instances.ContainsKey(typeof(T)))
            {
                if (source == null)
                {
                    source = FindAnyObjectByType<RuntimeAssetWindow>();
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

        private void Subscribe<T>(Transform _owner, UnityAction<T> onValueChanged) where T : Object
        {
            owner = _owner;
            gameObject.SetActive(true);

            onValueChangedEvent.RemoveAllListeners();
            onValueChangedEvent.AddListener(obj =>
            {
                if (_owner != null)
                    onValueChanged?.Invoke((T)obj);
            });
        }

        private void SetValues<T>(T[] values) where T : Object
        {
            if (elements.Count > 0)
            {
                for (int i = elements.Count - 1; i >= 0; i--)
                {
                    Destroy(elements[i].gameObject);
                }

                elements.Clear();
            }

            foreach (T value in values)
            {
                var button = Instantiate(elementPreset, elementParent);
                elements.Add(button);

                if (value is Texture tex)
                {
                    button.GetComponent<RawImage>().texture = tex;
                }
                else if (value is Material mat)
                {
                    button.GetComponent<RawImage>().texture = mat.mainTexture;
                }

                button.GetComponentInChildren<TMP_Text>().text = value.name;
                button.gameObject.SetActive(true);

                button.onClick.AddListener(() =>
                {
                    onValueChangedEvent.Invoke(value);
                });
            }
        }
    }
}