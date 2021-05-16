using UnityEngine;

public class Hero : MonoBehaviour
{
    // Attached Game Manager.
    public GameManager GM;
    // Attached player health bar script.
    public Healthbar2 healthbarScriptRef;
    // Attached enemy weapons.
    public OrbitalBeamLaserEdited laserBeamScriptRef;
    public ElectricField electricFieldScriptRef;

    // Hero's Health.
    public int heroHealth { get; set; }


    // Health control.
    private int maxHealth = 1000;
    private bool isAlive = true;

    // Audio parameters.
    [SerializeField] private AudioClip[] audioHit;
    [SerializeField] private AudioClip audioDead;
    private AudioSource audioSource;
    private float hitTimer = 0;
    private float hitTime = 1.0f;

    // Attached Canvas Group and red screen parameters.
    [SerializeField] private CanvasGroup m_canvasGroup;
    [SerializeField] private AnimationCurve CurveFade;
    private float FadeSpeed = 0.7f;
    private float DelayFade = 1.0f;
    private float MinAlpha = 0.45f;
    public float Alpha = 0;
    private float NextDelay = 0;

    
    private void Awake()
    {
        // Init player's health.
        heroHealth = maxHealth;
        healthbarScriptRef.health = heroHealth;

        // Init Audio.
        audioSource = this.GetComponent<AudioSource>();
        hitTimer = hitTime;
    }

    private void Update()
    {
        // Update player's health bar.
        healthbarScriptRef.health = heroHealth;

        // Update hit audio timer.
        hitTimer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        // If the game isn't over.
        if (heroHealth > 0)
        {
            // Apply the red fade effect to the HUD.
            FadeRedScreen();
        }
    }

    // Apply the red fade effect to the HUD.
    private void FadeRedScreen()
    {
        if (m_canvasGroup.alpha != Alpha)
        {
            if (Time.time > NextDelay && Alpha > 0)
            {
                Alpha = Mathf.Lerp(Alpha, 0, Time.deltaTime);
                Alpha = CurveFade.Evaluate(Alpha);
            }
            m_canvasGroup.alpha = Mathf.Lerp(m_canvasGroup.alpha, Alpha, Time.deltaTime * FadeSpeed);
        }
    }

    // Update the hero's health and HUD parameters with an incoming hitpoint.
    public void OnDamage(int hp)
    {
        // Decrease hero's health.
        heroHealth -= hp;
        // If the hero's health is at or below 0.
        if (heroHealth <= 0)
        {
            // Check the alive indication.
            if (isAlive == true)
            {
                // Call the Game Over sequence.
                GM.GameOver();

                // Play the dieing Audio.
                audioSource.PlayOneShot(audioDead, 200.0f);

                // Un-set the alive indication.
                isAlive = false;
            }
        }
        else
        {
            // Set the alive indication.
            if (isAlive == false)
            {
                isAlive = true;
            }

            // Play one of the hit sounds and init the hit timer.
            int i = UnityEngine.Random.Range(0, 4);
            if (audioHit[i] != null && hitTimer >= hitTime)
            {
                audioSource.PlayOneShot(audioHit[i], 5.0f);
                hitTimer = 0;
            }

            // Apply the Alpha according to the amount of health left.
            Alpha = (maxHealth - heroHealth) / 100;
            // Ensure that the Alpha is never less than the minimum allowed.
            Alpha = Mathf.Clamp(Alpha, MinAlpha, 1);
            // Update fade delay.
            NextDelay = Time.time + DelayFade;
        }
    }

    // Collider stay event handler.
    private void OnTriggerStay(Collider other)
    {
        // Upon getting hit by an enemy, reduce player's hp by the collider's hitpoint.
        if (other.name == "Orbital_Beam_Laser_Red")
        {
            int hp = laserBeamScriptRef.hitpoint;
            OnDamage(hp);
        }
        else if (other.name == "ElectricField")
        {
            int hp = electricFieldScriptRef.hitpoint;
            OnDamage(hp);
        }
    }

    // Collider enter event handler.
    private void OnTriggerEnter(Collider other)
    {
        // Upon getting hit by an enemy, reduce player's hp by the collider's hitpoint.
        if (other.name == "thunderBullet")
        {
            int hp = ThunderBullet.hitpoint;
            OnDamage(hp);
        }
    }
}
