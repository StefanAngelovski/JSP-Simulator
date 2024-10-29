using UnityEngine;
using TMPro;

public class PeopleCounter : MonoBehaviour
{
    public TextMeshProUGUI peopleCountDisplay; // Reference to the UI Text component
    private int peopleCount = 0; // Tracks the current number of people

    private void Start()
    {
        UpdateDisplay(); // Initialize display on start
    }

    // Method to increment the count
    public void IncrementCount()
    {
        peopleCount++;
        UpdateDisplay(); // Update the UI after incrementing
    }

    // Method to decrement the count
    public void DecrementCount()
    {
        if (peopleCount > 0)
        {
            peopleCount--;
            UpdateDisplay(); // Update the UI after decrementing
        }
    }

    // Method to update the UI display
    private void UpdateDisplay()
    {
        if (peopleCountDisplay != null)
        {
            peopleCountDisplay.text = "People in Bus: " + peopleCount;
        }
    }
}
