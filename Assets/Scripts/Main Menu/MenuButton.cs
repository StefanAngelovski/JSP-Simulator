using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    [SerializeField] MenuButtonController menuButtonController;
    [SerializeField] Animator animator;
    [SerializeField] AnimatorFunctions animatorFunctions;
    [SerializeField] int thisIndex;
    [SerializeField] string sceneToLoad;
    [SerializeField] bool isQuitButton;

    void Update()
    {
        if (menuButtonController.index == thisIndex)
        {
            animator.SetBool("selected", true);

            if (Input.GetAxis("Submit") == 1)
            {
                animator.SetBool("pressed", true);
                
                if (isQuitButton)
                {
                    QuitGame(); // Call QuitGame if it’s the quit button
                }
                else if (!string.IsNullOrEmpty(sceneToLoad))
                {
                    LoadScene(); // Call LoadScene if it's not the quit button and has a scene to load
                }
            }
            else if (animator.GetBool("pressed"))
            {
                animator.SetBool("pressed", false);
                animatorFunctions.disableOnce = true;
            }
        }
        else
        {
            animator.SetBool("selected", false);
        }
    }

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
