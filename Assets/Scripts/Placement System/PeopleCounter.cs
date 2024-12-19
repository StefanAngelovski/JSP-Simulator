using UnityEngine;
using TMPro;

public class PeopleCounter : MonoBehaviour
{
    public TextMeshProUGUI peopleCountDisplay; 
    private int peopleCount = 0; 

    private void Start()
    {
        UpdateDisplay(); 
    }

    public void IncrementCount()
    {
        peopleCount++;
        UpdateDisplay(); 
    }

    public void DecrementCount()
    {
        if (peopleCount > 0)
        {
            peopleCount--;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (peopleCountDisplay != null)
        {
            peopleCountDisplay.text = peopleCount.ToString();
        }
    }
}
