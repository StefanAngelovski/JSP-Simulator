using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // Public variable to assign the scene name in the inspector
    public string sceneToLoad;

    // Method to load the specified scene
    public void LoadScene()
    {
        // Load the scene with the name specified in the inspector
        SceneManager.LoadScene(sceneToLoad);
    }

    // Method to quit the application
    public void QuitGame()
    {
        // If we're running in the editor, stop playing
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Otherwise, quit the application
            Application.Quit();
        #endif
    }
}
