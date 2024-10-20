using UnityEngine;

public class Characteristics : MonoBehaviour
{
    public enum Gender { Male, Female }
    public int capacity;
    public Gender gender;

    private string[] maleNames = { "John", "Alex", "Michael", "David", "Chris" };
    private string[] femaleNames = { "Sarah", "Emily", "Anna", "Jessica", "Sophia" };
    private string[] capacityInfo = {
        "Building a belly to become santa",
        "Thin and Slim",
        "Hi I'm a cat!",
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
            displayCharacteristics.UpdateText(generatedName, generatedInfo);
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
        if (capacity == 3)
        {
            generatedInfo = capacityInfo[0];
        }
        else if (capacity == 1)
        {
            generatedInfo = capacityInfo[1];
        }
        else if (capacity == 2)
        {
            generatedInfo = capacityInfo[2];
        }
    }
}
