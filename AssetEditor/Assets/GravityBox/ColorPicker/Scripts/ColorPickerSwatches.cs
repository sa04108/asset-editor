using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.ColorPicker
{
    /// <summary>
    /// Behaviour responsible for loading, creating and storing color presets. 
    /// For now single file only supported
    /// </summary>
    public class ColorPickerSwatches : MonoBehaviour
    {
        public enum DataPath { Default, Persistent }

        [SerializeField]
        private DataPath dataPath = DataPath.Default;
        [SerializeField]
        private string folderPath = "Configs";
        [SerializeField]
        private ColorObject colorObject;
        [SerializeField]
        private ColorPickerSwatchesItem template;
        [SerializeField]
        private Button createButton;
        [SerializeField]
        private Transform content;
        [SerializeField]
        private Transform contextMenu;

        private string filename;
        private ColorPickerSwatchesItem contextTarget;
        private List<ColorPickerSwatchesItem> items = new List<ColorPickerSwatchesItem>();

        private void Awake()
        {
            filename = dataPath == DataPath.Default ? Application.dataPath : Application.persistentDataPath;
            filename = string.Join(Path.DirectorySeparatorChar.ToString(), new object[] { filename, folderPath, "ColorPresets.xml" });

            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    string content = reader.ReadToEnd();
                    ColorPresets presets = (ColorPresets)JsonUtility.FromJson(content, typeof(ColorPresets));
                    foreach (var p in presets.presets)
                        items.Add(CreatePresetInternal(p.color, p.intensity));
                }

                createButton.transform.SetAsLastSibling();
            }
            catch { }

            createButton.onClick.AddListener(OnCreateClicked);
        }

        private void OnDestroy()
        {
            if (items.Count > 0)
            {
                string directory = Path.GetDirectoryName(filename);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using (StreamWriter writer = new StreamWriter(filename))
                {
                    ColorPresets presets = new ColorPresets();
                    for (int i = 0; i < items.Count; i++)
                        presets.AddPreset(items[i].color, items[i].intensity);

                    string content = JsonUtility.ToJson(presets);

                    writer.Write(content);
                    writer.Close();
                }
            }
        }

        public void OnCreateClicked()
        {
            items.Add(CreatePresetInternal(colorObject.GetRGBA32(), colorObject.GetIntensity()));
            createButton.transform.SetAsLastSibling();
        }

        private ColorPickerSwatchesItem CreatePresetInternal(Color color, float intensity)
        {
            ColorPickerSwatchesItem item = Instantiate(template, content);
            item.gameObject.SetActive(true);
            item.color = color;
            item.intensity = intensity;
            item.onClick += SetCurrentColor;
            item.onContext += ShowContextMenu;
            return item;
        }

        void SetCurrentColor(ColorPickerSwatchesItem item)
        {
            colorObject.SetRGBA(item.color);
            colorObject.SetIntensity(item.intensity);
        }

        void ShowContextMenu(ColorPickerSwatchesItem index)
        {
            contextTarget = index;
            contextMenu.parent.SetAsLastSibling();
            contextMenu.parent.gameObject.SetActive(true);
            contextMenu.position = index.transform.position;
        }

        /// <summary>
        /// context menu commands
        /// </summary>
        public void RemoveCurrentPreset()
        {
            items.Remove(contextTarget);
            Destroy(contextTarget.gameObject);
            contextMenu.parent.gameObject.SetActive(false);
        }

        public void ReplaceCurrentPreset()
        {
            contextTarget.color = colorObject.GetRGBA32();
            contextTarget.intensity = colorObject.GetIntensity();
            contextMenu.parent.gameObject.SetActive(false);
        }

        public void MakeFirstCurrentPreset()
        {
            items.Remove(contextTarget);
            items.Insert(0, contextTarget);
            contextTarget.transform.SetAsFirstSibling();
            contextMenu.parent.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Color presets library object. Used for serializing current list of presets
    /// </summary>
    [System.Serializable]
    public class ColorPresets
    {
        [System.Serializable]
        public class Preset
        {
            public Color color;
            public float intensity;
        }

        public List<Preset> presets = new List<Preset>();

        public void AddPreset(Color color, float intensity)
        {
            presets.Add(new Preset() { color = color, intensity = intensity });
        }
    }
}