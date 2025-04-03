using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Merlin
{
    public class ShaderSelectionMember : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropDown;

        private List<Shader> shaders = new();
        private Material mat;

        [HideInInspector]
        public UnityEvent<Material> OnShaderChanged;

        private void Start()
        {
            dropDown.onValueChanged.AddListener(OnSelected);
            gameObject.SetActive(false);
        }

        public void Initialize(string[] options)
        {
            shaders.Clear();
            dropDown.options.Clear();

            foreach (var option in options)
            {
                dropDown.options.Add(new TMP_Dropdown.OptionData(option));

                var shader = Shader.Find(option);
                if (shader == null)
                {
                    Debug.LogWarning($"{option} is not a valid shader");
                }

                shaders.Add(shader);
            }
        }

        public void SetMaterial(Material mat)
        {
            gameObject.SetActive(true);
            this.mat = mat;

            for (int i = 0; i < shaders.Count; i++)
            {
                if (mat.shader == shaders[i])
                {
                    dropDown.SetValueWithoutNotify(i);
                }
            }
        }

        private void OnSelected(int value)
        {
            mat.shader = shaders[value];
            OnShaderChanged.Invoke(mat);
        }
    }
}