using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayGame : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip audioClip;            
    public Animator buttonAnimator;         
    public float sceneLoadDelay = 0.5f; 
    public string Scene;     

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

       
        if (audioSource != null && audioClip != null)
        {
            audioSource.clip = audioClip;   
            audioSource.Play();             
        }
        else if (audioClip == null)
        {
            Debug.LogWarning("No AudioClip assigned to play.");
        }
        else
        {
            Debug.LogWarning("No AudioSource component found on this GameObject.");
        }

        
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool("selected", true);
        }
        else
        {
            Debug.LogWarning("No Animator component assigned for the button.");
        }
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Return) && buttonAnimator != null)
        {
            buttonAnimator.SetBool("pressed", true);
            StartCoroutine(LoadSceneWithDelay(sceneLoadDelay));
        }

        if (Input.GetKeyUp(KeyCode.Return) && buttonAnimator != null)
        {
            buttonAnimator.SetBool("pressed", false);
        }
    }

    private IEnumerator LoadSceneWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(Scene);
    }
}
