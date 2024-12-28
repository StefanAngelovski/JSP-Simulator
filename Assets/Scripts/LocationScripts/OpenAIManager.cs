using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;

public class OpenAIManager : MonoBehaviour
{
    // Public references for RawImage and Textures (drag images directly from the Project files)
    public RawImage rawImage; // RawImage component for displaying texture
    public Texture defaultImageUI;
    public Texture processingImageUI;
    public Texture successImageUI;
    public Texture failureImageUI;

    public TMP_InputField inputField;
    public Button submitButton;

    private string openAIKey = "OPENAI-KEY";
    private string openAIEndpoint = "https://api.openai.com/v1/chat/completions";  // Correct endpoint for GPT-3.5-turbo

    void Start()
    {
        // Check if necessary components are assigned
        if (rawImage == null)
        {
            Debug.LogError("RawImage is not assigned in the Inspector.");
            return; // Exit if RawImage is not assigned
        }
        if (defaultImageUI == null)
        {
            Debug.LogError("defaultImageUI is not assigned in the Inspector.");
            return; // Exit if default image isn't assigned
        }
        if (submitButton == null)
        {
            Debug.LogError("submitButton is not assigned in the Inspector.");
            return; // Exit if button isn't assigned
        }

        // Set the default image
        SetTexture(defaultImageUI);

        // Add listener to the submit button
        submitButton.onClick.AddListener(OnSubmit);
    }

    public void OnSubmit()
    {
        string userInput = inputField.text;
        if (string.IsNullOrEmpty(userInput)) return;

        // Show the processing image while waiting for the response
        SetTexture(processingImageUI);

        // Call OpenAI API
        StartCoroutine(SendToOpenAI(userInput));
    }

    private IEnumerator SendToOpenAI(string userInput)
    {
        string jsonData = "{\"model\": \"gpt-3.5-turbo\", \"messages\": [" +
                          "{\"role\": \"system\", \"content\": \"You reply only with municipality names and ',' between them.\"}," +
                          "{\"role\": \"user\", \"content\": \"i am in centar in skopje, im going to give you a municipality, reply to me in one line telling me all municipalities that i need to go through to get to the one i want, only one line with delimiter ',', dont ignore instruction. Here is where i want to go to: " + userInput + "\"}]," +
                          "\"max_tokens\": 50}";

        // Only one declaration of bodyRaw
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(openAIEndpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + openAIKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;

            // Log the raw response from OpenAI for debugging
            Debug.Log("Raw response: " + response);

            // Parse the response to extract the message content
            var jsonResponse = JsonUtility.FromJson<OpenAIResponse>(response);

            // Check if the response contains valid data
            if (jsonResponse != null && jsonResponse.choices != null && jsonResponse.choices.Length > 0)
            {
                string message = jsonResponse.choices[0].message.content;
                Debug.Log("OpenAI message: " + message); // Log the message content from OpenAI

                int locationCount = message.Split(',').Length;
                Debug.Log("Total number of locations: " + locationCount);

                // Show appropriate image based on result
                SetTexture(successImageUI);
            }
            else
            {
                Debug.LogError("Error: No valid message found in the response.");
                SetTexture(failureImageUI);
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            SetTexture(failureImageUI);
        }

        yield break; // End the coroutine
    }

    private void SetTexture(Texture texture)
    {
        if (rawImage != null && texture != null)
        {
            rawImage.texture = texture;
        }
        else
        {
            Debug.LogError("RawImage or Texture not assigned correctly.");
        }
    }

    // JSON response structure
    [System.Serializable]
    public class OpenAIResponse
    {
        public OpenAIChoice[] choices;
    }

    [System.Serializable]
    public class OpenAIChoice
    {
        public OpenAIMessage message;
    }

    [System.Serializable]
    public class OpenAIMessage
    {
        public string content;
    }
}
