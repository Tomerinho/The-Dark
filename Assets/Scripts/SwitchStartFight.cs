using System.Collections;
using UnityEngine;

public class SwitchStartFight : MonoBehaviour
{
    // Attached components.
    public GameManager GM;
    public AudioManager audioManager;
    public Spider spider;
    public Light lightSource;
    public Light redGlow;
    public GameObject LEDs;
    public GameObject spiderLightP1;
    public GameObject distortion;
    public GameObject spiderHUD;
    public AudioClip audioWake1;
    public AudioClip audioWake2;
    public AudioClip audioWalk;
    public AudioClip audioRoar;

    // Fight start controls.
    public Coroutine IncreaseLightIntensityCR;
    public Coroutine SpiderHealthRegenCR;
    public Coroutine AudioSpiderWakeCR;
    public bool isFightStarted { get; set; } = false;
    public bool didReachMaxGlow { get; set; } = false;
    public bool isFinishedLightingUp { get; set; } = false;
    public bool isRegenStarted { get; set; } = false;
    public bool hasPlayedWake2 { get; set; } = false;
    public int countAudio { get; set; } = 0;
    public float walkAudioTimer { get; set; } = 1.8f;


    // Audio sources.
    [SerializeField] private AudioSource audioSourcePlayer;
    private AudioSource audioSource;

    // Light intensity control.
    private float intensitySpeed = 0.2f;
    private float intermediateIntensity = 0.25f;
    private float maxIntensity = 0.5f;
    private float glowIntensitySpeed = 2.5f;
    private float minGlowIntensity = 2.15f;
    private float maxGlowIntensity = 6.0f;
    // Audio control.
    private float walkAudioTime = 2.0f;
    private float audioSpeed = 1.5f;


    private void Awake()
    {
        audioSource = spider.GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "player" && isFightStarted == false)
        {
            // Disable second-time triggering.
            isFightStarted = true;

            // Destroy LEDs.
            LEDs.gameObject.SetActive(false);

            // Turn on spider lights.
            spiderLightP1.SetActive(true);
            // Start a co-routine to slowly turn the directional light on.
            IncreaseLightIntensityCR = StartCoroutine(IncreaseLightIntensity());

            // Start a co-routine to regenerate spider's health.
            SpiderHealthRegenCR = StartCoroutine(SpiderHealthRegen());

            // Start a co-routine to sync audio to the spider's wake-up sequence.
            AudioSpiderWakeCR = StartCoroutine(AudioSpiderWake());

            // Exit spider idle state.
            spider.isExitingIdle = true;

            // Play the appropriate background track.
            audioManager.PlayTrack(1);
            // Set the desired time.
            audioManager.SetTrackTime(37.4f);
        }
    }

    // Co-routine to increase light intensity over time.
    private IEnumerator IncreaseLightIntensity()
    {
        // While haven't finished lighting-up all aspects of the pit and spider lights.
        while (isFinishedLightingUp == false)
        {
            // Once glow intensity reaches a certain level, start increasing directional lighting.
            if (didReachMaxGlow == true)
            {
                if (lightSource.intensity < maxIntensity)
                {
                    lightSource.intensity += Time.deltaTime * intensitySpeed;
                }

                if (redGlow.intensity > minGlowIntensity)
                {
                    redGlow.intensity -= Time.deltaTime * glowIntensitySpeed * 0.5f;
                }
                else
                {
                    // Update light and distortion effects.
                    isFinishedLightingUp = true;
                    distortion.SetActive(true);

                    // Turn on spider HUD.
                    spiderHUD.SetActive(true);
                    // Regenerate spider's health.
                    isRegenStarted = true;
                    // Play the roar audio
                    audioSource.PlayOneShot(audioRoar, 0.5f);
                }
            }
            // Increase glow intensity, then decrease it after reaching the max threshold.
            else
            {
                if (redGlow.intensity < maxGlowIntensity)
                {
                    redGlow.intensity += Time.deltaTime * glowIntensitySpeed;
                }
                else
                {
                    didReachMaxGlow = true;
                    lightSource.intensity = intermediateIntensity;
                }
            }

            yield return null;
        }
    }

    // Co-routine to regenerate spider's health.
    private IEnumerator SpiderHealthRegen()
    {
        // While haven't finished regenerating.
        while (spider.spiderHealth < spider.spiderMaxHealth)
        {
            if (isRegenStarted == true && GM.isPaused == false)
            {
                // Regenerate health.
                spider.spiderHealth += spider.spiderHealthRegenSpeed;
                if (spider.spiderHealth > spider.spiderMaxHealth)
                {
                    spider.spiderHealth = spider.spiderMaxHealth;
                }
            }

            yield return null;
        }
    }

    // Co-routine to sync audio to the spider's wake up sequence.
    private IEnumerator AudioSpiderWake()
    {
        // Play the wake-up audio.
        audioSource.PlayOneShot(audioWake1, 0.8f);

        // While haven't finished the sequence.
        while (countAudio < 4)
        {
            // If the walking sound timer hasn't finished.
            if (walkAudioTimer < walkAudioTime)
            {
                // Increment the walking sound timer according to the movement speed.
                walkAudioTimer += Time.deltaTime * audioSpeed;
            }
            else
            {
                // Play the walking audio
                audioSource.PlayOneShot(audioWalk, 12.0f);
                walkAudioTimer = 0;
                countAudio++;
                if (countAudio == 2 && hasPlayedWake2 == false)
                {
                    hasPlayedWake2 = true;
                    audioSource.PlayOneShot(audioWake2, 0.8f);
                }
            }

            yield return null;
        }
    }
}
