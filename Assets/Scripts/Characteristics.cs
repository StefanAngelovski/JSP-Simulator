using UnityEngine;

public class Characteristics : MonoBehaviour
{
    public enum Gender { Male, Female }
    public int capacity;  // Integer for capacity
    public Gender gender;

    public enum Type { Adult, Elderly, Student, Tourist, Child }
    public Type type;

    private string[] maleNames = { "John", "Alex", "Michael", "David", "Chris" };
    private string[] femaleNames = { "Sarah", "Emily", "Anna", "Jessica", "Sophia" };
    private string[] capacityInfo = {
        "Adult",
        "Elderly",
        "Student",
        "Tourist",
        "Child"
    };

    private string generatedName;
    private string generatedInfo;

    private DisplayCharacteristics displayCharacteristics;

    void Start()
    {
        displayCharacteristics = FindObjectOfType<DisplayCharacteristics>();

        GenerateRandomName();
        GenerateRandomInfo();
        UpdateDisplay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateRandomName();
            GenerateRandomInfo();
            UpdateDisplay();

            Debug.Log("Generated Name: " + generatedName);
            Debug.Log("Generated Info: " + generatedInfo);
        }
    }

    void UpdateDisplay()
    {
        if (displayCharacteristics != null)
        {
            // Update the display with the generated name, info, and capacity (int)
            displayCharacteristics.UpdateText(generatedName, generatedInfo,capacity);
        }
    }

    void GenerateRandomName()
    {
        if (gender == Gender.Male)
        {
            int randomIndex = Random.Range(0, maleNames.Length);
            generatedName = maleNames[randomIndex];
        }
        else if (gender == Gender.Female)
        {
            int randomIndex = Random.Range(0, femaleNames.Length);
            generatedName = femaleNames[randomIndex];
        }
    }

    void GenerateRandomInfo()
    {
        // Generated info based on type (string)
        switch (type)
        {
            case Type.Adult:
                generatedInfo = capacityInfo[0];
                break;
            case Type.Elderly:
                generatedInfo = capacityInfo[1];
                break;
            case Type.Student:
                generatedInfo = capacityInfo[2];
                break;
            case Type.Tourist:
                generatedInfo = capacityInfo[3];
                break;
            case Type.Child:
                generatedInfo = capacityInfo[4];
                break;
        }
    }
}
