using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Attached components.
    public GameObject objBackgroundTracks;
    public AudioClip[] backgroundTracks;

    // Audio source component attached to the game object.
    private AudioSource audioSource;
    // Audio control;
    private float volumeMin= 0.0f;
    private float volumeMax = 1.0f;
    private float volumeTimerSpeed = 0.5f;
    private bool isIncreaseVolumeCR = false;
    private bool isDecreaseVolumeCR = false;


    private void Awake()
    {
        audioSource = objBackgroundTracks.GetComponent<AudioSource>();
    }

    // Play a numbered background track.
    public void PlayTrack(int i)
    {
        audioSource.clip = backgroundTracks[i];
        audioSource.time = 0.0f;
        audioSource.Play();
    }

    // Set the AudioClip's time.
    public void SetTrackTime(float time)
    {
        audioSource.time = time;
    }

    // Stop the background track.
    public void StopTrack()
    {
        audioSource.Stop();
    }

    // Turn on looping.
    public void LoopOn()
    {
        audioSource.loop = true;
    }

    // Turn off looping.
    public void LoopOff()
    {
        audioSource.loop = false;
    }

    public void IncreaseGlobalAudio()
    {
        // Start a co-routine to increase the game Audio volume.
        StartCoroutine("IncreaseAudioVolume");
    }

    public void DecreaseGlobalAudio()
    {
        // Start a co-routine to decrease the game Audio volume.
        StartCoroutine("DecreaseAudioVolume");
    }

    // Co-routine to increase the game Audio volume.
    private IEnumerator IncreaseAudioVolume()
    {
        // Indicate the CR is running.
        isIncreaseVolumeCR = true;

        // If the volume decrease co-routine is running, stop it.
        if (isIncreaseVolumeCR == true)
        {
            StopCoroutine("DecreaseAudioVolume");
            // Indicate the co-routine has stopped.
            isDecreaseVolumeCR = false;
        }

        // While haven't finished the sequence.
        while (AudioListener.volume < volumeMax)
        {
            // Increase the volume.
            AudioListener.volume += Time.deltaTime * volumeTimerSpeed;
            yield return null;
        }

        // Indicate the CR isn't running.
        isIncreaseVolumeCR = false;
    }

    // Co-routine to increase the game Audio volume.
    private IEnumerator DecreaseAudioVolume()
    {
        // Indicate the CR is running.
        isDecreaseVolumeCR = true;

        // If the volume increase co-routine is running, stop it.
        if (isIncreaseVolumeCR == true)
        {
            StopCoroutine("IncreaseAudioVolume");
            // Indicate the co-routine has stopped.
            isIncreaseVolumeCR = false;
        }

        // While haven't finished the sequence.
        while (AudioListener.volume > volumeMin)
        {
            // Decrease the volume.
            AudioListener.volume -= Time.deltaTime * volumeTimerSpeed;
            yield return null;
        }

        // Indicate the CR isn't running.
        isDecreaseVolumeCR = false;
    }
}
