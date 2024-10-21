using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider slider; // Reference to the UI Slider component
    public int maxCapacity = 1000; // Max capacity for the slider
    public GameObject info; // Reference to the GameObject containing the DisplayCharacteristics script

    private DisplayCharacteristics displayCharacteristicsScript;

    void Start()
    {
        // Get the DisplayCharacteristics component from the info GameObject
        if (info != null)
        {
            displayCharacteristicsScript = info.GetComponent<DisplayCharacteristics>();

            if (displayCharacteristicsScript != null)
            {
                slider.maxValue = maxCapacity; // Set the maximum value for the slider
                slider.value = maxCapacity; // Initialize slider value to maxCapacity
                Debug.Log("Initial Slider Value: " + slider.value);
            }
            else
            {
                Debug.LogError("No DisplayCharacteristics script found on the info GameObject.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check for space bar press
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Check if the DisplayCharacteristics script can update capacity
            if (displayCharacteristicsScript != null)
            {
                int currentCapacity = displayCharacteristicsScript.ObjectCapacity; // Get the current capacity

                // Ensure the slider doesn't go below zero
                if (slider.value - currentCapacity >= 0)
                {
                    slider.value -= currentCapacity; // Decrease the slider value by current capacity
                    Debug.Log("Slider Value after pressing Space: " + slider.value);
                }
                else
                {
                    Debug.LogWarning("Not enough capacity to reduce slider value."); // Warn if the slider would go negative
                }
            }
        }
    }
}
