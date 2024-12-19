using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreRawImageController : MonoBehaviour
{
    public Texture defaultImage;  // Default texture
    public Texture scoreUpImage;  // Texture for score increase
    public Texture scoreDownImage; // Texture for score decrease
    private RawImage rawImage;    // Reference to the RawImage component

    public float displayDuration = 1f; // Time to display the score up/down image

    private void Start()
    {
        rawImage = GetComponent<RawImage>();
        SetDefaultImage();
    }

    public void SetDefaultImage()
    {
        if (rawImage != null)
        {
            rawImage.texture = defaultImage;
            rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 1f); // Ensure it's visible
        }
    }

    public void ShowScoreUpImage()
    {
        if (rawImage != null)
        {
            StopAllCoroutines(); // Stop any ongoing coroutine to avoid conflicts
            StartCoroutine(ShowTemporaryImage(scoreUpImage, displayDuration));
        }
    }

    public void ShowScoreDownImage()
    {
        if (rawImage != null)
        {
            StopAllCoroutines(); // Stop any ongoing coroutine to avoid conflicts
            StartCoroutine(ShowTemporaryImage(scoreDownImage, displayDuration));
        }
    }

    private IEnumerator ShowTemporaryImage(Texture temporaryImage, float duration)
    {
        if (rawImage != null)
        {
            rawImage.texture = temporaryImage;
            rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, 1f); // Fully visible
            yield return new WaitForSeconds(duration);

            // Directly switch back to the default image
            SetDefaultImage();
        }
    }
}
