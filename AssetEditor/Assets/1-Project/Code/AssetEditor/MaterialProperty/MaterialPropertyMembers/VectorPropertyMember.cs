using TMPro;
using UnityEngine;

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

        public new void Initialize(string label, Material mat, Vector4 value, string propName)
        {
            base.Initialize(label, mat, value, propName);

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
                Vector4 currVec = CurrentValue;
                currVec[idx] = fResult;
                CurrentValue = currVec;
                inputField.SetTextWithoutNotify(fResult.ToString());

                mat.SetVector(propertyName, CurrentValue);
            }
            else // 빈 값 입력 포함
            {
                inputField.SetTextWithoutNotify(CurrentValue[idx].ToString());
            }
        }

        public override void UpdateUI()
        {
            base.UpdateUI();

            for (int i = 0; i < 4; i++)
            {
                inputFields[i].SetTextWithoutNotify(CurrentValue[i].ToString());
            }
        }
    }
}