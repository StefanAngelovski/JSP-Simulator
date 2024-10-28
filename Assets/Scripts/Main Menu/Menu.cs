using UnityEngine;

public class Menu : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip audioClip;             
    public Animator buttonAnimator;           
    public GameObject current;
    public GameObject next;

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

        // Set the animator of the button to "selected"
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
        // Check if the Enter key (Return) is pressed
        if (Input.GetKeyDown(KeyCode.Return) && buttonAnimator != null)
        {
            buttonAnimator.SetBool("pressed", true);
            current.SetActive(false);
            next.SetActive(true);
        }

        // Reset "pressed" to false when the Enter key is released
        if (Input.GetKeyUp(KeyCode.Return) && buttonAnimator != null)
        {
            buttonAnimator.SetBool("pressed", false);
        }
    }
}
