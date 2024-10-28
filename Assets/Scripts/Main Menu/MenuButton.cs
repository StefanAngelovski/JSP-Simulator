using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    [SerializeField] private MenuButtonController menuButtonController;
    [SerializeField] private Animator animator;
    [SerializeField] private AnimatorFunctions animatorFunctions;
    [SerializeField] private int thisIndex;
    [SerializeField] private GameObject currentGameObject;
    [SerializeField] private GameObject nextGameObject;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private bool isQuitButton;
    [SerializeField] private bool isExtras;
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
                    QuitGame();
                }
                else if (isExtras)
                {
                    currentGameObject.SetActive(false);
                    nextGameObject.SetActive(true);

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
            animator.SetBool("selected",
 false);
        }
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void QuitGame()
    {

        // If we're running in the editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Otherwise, quit the application
        Application.Quit();
#endif

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif

    }

}