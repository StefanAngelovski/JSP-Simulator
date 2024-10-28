using UnityEngine;
using UnityEngine.SceneManagement;

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
                    QuitGame();
                }
                else if (!string.IsNullOrEmpty(sceneToLoad))
                {
                    LoadScene(); 
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

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
