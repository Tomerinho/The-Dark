using System.Collections;
using UnityEngine;

public class ThunderClap : MonoBehaviour
{
    public AudioClip clip;
    private bool canFlicker = true;

    private Light lightSource;
    private AudioSource audioSource;


    private void Awake()
    {
        lightSource = GetComponent<Light>();
        audioSource = GetComponent<AudioSource>();
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

            yield return new WaitForSeconds(Random.Range(0.8f, 1.0f));
            int rand = Random.Range(1, 3);
            if (rand == 1)
            {
                audioSource.volume = Random.Range(0.3f, 1);
                audioSource.PlayOneShot(clip);
            }
            lightSource.enabled = true;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            lightSource.enabled = false;
            yield return new WaitForSeconds(Random.Range(0.1f, 5));

            canFlicker = true;
        }
    }
}
