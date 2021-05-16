using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    // Externally accessed Co-Routine.
    [HideInInspector] public Coroutine CRFadeIn;
    // New color with updated alpha.
    [HideInInspector] public Color newColor;


    // Fade control.
    private float fadeSpeed = 0.2f;
    private float minAlpha = 0.0f;
    private float maxAlpha = 1.0f;


    private void Awake()
    {
        // Init the new color.
        newColor = this.GetComponent<Image>().color;
        SetTransparent();
    }

    // Set the screen fully transparent.
    public void SetTransparent()
    {
        newColor.a = minAlpha;
        this.GetComponent<Image>().color = newColor;
    }

    // Fade the screen image in.
    public void FadeIn()
    {
        // Start a co-routine to fade the screen image in.
        CRFadeIn = StartCoroutine(FadeInCoRoutine());
    }

    // Co-routine to fade the screen image in.
    private IEnumerator FadeInCoRoutine()
    {
        // Increase the alpha until the image is fully opaque.
        while (newColor.a < maxAlpha)
        {
            newColor.a += Time.deltaTime * fadeSpeed;
            this.GetComponent<Image>().color = newColor;

            yield return null;
        }
    }
}
