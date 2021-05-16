using System.Collections;
using UnityEngine;

public class lampBlink : MonoBehaviour
{
    public AudioClip clip;
    public bool canFlicker = true;


    // Attached components.
    private AudioSource audioSource;
    private Light lightSource;


    private void Awake()
    {
        // Init attached components.
        audioSource = GetComponent<AudioSource>();
        lightSource = GetComponent<Light>();
    }

    private void Update()
    {
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        if (canFlicker)
        {
            canFlicker = false;

            audioSource.volume = Random.Range(0.1f, 0.3f);
            audioSource.PlayOneShot(clip);
            lightSource.enabled = false;
            yield return new WaitForSeconds(0.1f);
            lightSource.enabled = true;
            yield return new WaitForSeconds(Random.Range(0.1f, 1.5f));

            canFlicker = true;
        }
    }
}