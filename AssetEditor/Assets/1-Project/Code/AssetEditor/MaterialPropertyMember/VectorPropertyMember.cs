using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class VectorPropertyMember : MaterialPropertyMember<Vector4>
    {
        [SerializeField] private TMP_Text[] inputFieldLabels;
        [SerializeField] private TMP_InputField[] inputFields;

        private void Start()
        {
            for (int i = 0; i < 4; i++)
            {
                int c_i = i;
                inputFields[i].onEndEdit.AddListener(value =>
                {
                    OnInputValueChanged(inputFields[c_i], value, c_i);
                });
            }
        }

        public void Initialize(Material mat, string name, Vector4 value)
        {
            base.Initialize(mat, MaterialPropertyType.Vector, name, value);

            string[] vectorChanels = { "X", "Y", "Z", "W" };
            for (int i = 0; i < 4; i++)
            {
                inputFieldLabels[i].text = vectorChanels[i];
                inputFields[i].SetTextWithoutNotify(value[i].ToString());
            }
        }

        private void OnInputValueChanged(TMP_InputField inputField, string value, int idx)
        {
            if (float.TryParse(value, out float fResult))
            {
                currentValue[idx] = fResult;
                inputField.SetTextWithoutNotify(fResult.ToString());

                mat.SetVector(title.text, currentValue);
            }
            else // 빈 값 입력 포함
            {
                inputField.SetTextWithoutNotify(currentValue[idx].ToString());
            }
        }
    }
}