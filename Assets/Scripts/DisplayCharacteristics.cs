using UnityEngine;
using TMPro;

public class DisplayCharacteristics : MonoBehaviour
{
    public TextMeshProUGUI infoText;
    private int objectCapacity;

    public int ObjectCapacity => objectCapacity;

    public void UpdateText(string name, string info, int capacity)
    {
        if (infoText != null)
        {
            infoText.text = $"Name: {name}\nInfo: {info}";
            objectCapacity = capacity;
            Debug.Log("Capacity: " + ObjectCapacity);
        }
    }
}
