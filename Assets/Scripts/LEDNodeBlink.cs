using System.Collections;
using UnityEngine;

// A LED node emitting light based on start/stop signals from the previous node in the chain.
public class LEDNodeBlink : MonoBehaviour
{
    // Light flicker control.
    public Coroutine CRFlicker;
    public GameObject[] LEDGameObjects;


    // LED lights parameters.
    private LEDNodeAlwaysOn[] LEDNodes;
    private int size;

    // Attached components.
    private AudioSource audioSource;

    // Audio control.
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private bool isAudioEnabled = false;
    private bool canFlicker = true;


    private void Awake()
    {
        // Init the LEDs' array size.
        size = LEDGameObjects.Length;

        // Init the LED Node component.
        LEDNodes = new LEDNodeAlwaysOn[size];
        for (int i = 0; i < size; i++)
        {
            LEDNodes[i] = LEDGameObjects[i].GetComponent<LEDNodeAlwaysOn>();
        }

        // Init attached components.
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Co-routine to flicker the light.
        CRFlicker = StartCoroutine(Flicker());
    }

    private void OnDisable()
    {
        canFlicker = true;
    }

    // Co-routine to flicker the light.
    private IEnumerator Flicker()
    {
        if (canFlicker)
        {
            canFlicker = false;

            // If audio is enabled, play the audio clip with a random volume.
            if (isAudioEnabled == true)
            {
                audioSource.volume = Random.Range(0.1f, 0.3f);
                audioSource.PlayOneShot(audioClip);
            }

            // Flicker the light.
            for (int i = 0; i < size; i++)
            {
                LEDNodes[i].intensity = LEDNodes[i].maxIntensity;
            }
            yield return new WaitForSeconds(0.1f);
            for (int i = 0; i < size; i++)
            {
                LEDNodes[i].intensity = LEDNodes[i].minIntensity;
            }
            yield return new WaitForSeconds(Random.Range(0.1f, 1.5f));

            canFlicker = true;
        }
    }
}
