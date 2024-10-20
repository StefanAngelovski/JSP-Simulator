using UnityEngine;
using TMPro;

public class DisplayCharacteristics : MonoBehaviour
{
    public TextMeshProUGUI infoText; 

    public void UpdateText(string name, string info)
    {
        if (infoText != null)
        {
            infoText.text = $"Name: {name}\nInfo: {info}";
        }
    }
}
