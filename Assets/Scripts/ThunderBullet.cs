using UnityEngine;

public class ThunderBullet : MonoBehaviour
{
    // Attached audio components.
    public AudioClip audioCharge = null;
    public AudioClip audioShoot = null;
    public AudioClip audioHit = null;
    // Attached particle system.
    public ParticleSystem particle = null;

    // Laser bullet's hitpoint and travel speed.
    public static int hitpoint { get; set; } = 120;
    public float speed { get; set; } = 33.0f;

    
    // Attached components.
    private Renderer rend;
    private Collider coll;
    private AudioSource audioSource;

    // Source and direction.
    private Transform sourceTrnsfrm;
    private Transform directionTrnsfrm;
    private Vector3 direction;
    // Movement parameters.
    private float followDistance = 7.5f;
    private float thunderChargeTime = 1.8f;
    private float thunderChargeLevel = 0.0f;
    private bool canMove = true;
    private bool shouldFollowDirection = true;
    // Sound control.
    private bool hasPlayed = false;


    private void Awake()
    {
        // Init the direction.
        sourceTrnsfrm = this.transform;
        directionTrnsfrm = GameObject.Find("/player").GetComponent<Transform>();
        direction = directionTrnsfrm.position - sourceTrnsfrm.position;
        direction.Normalize();

        // Init attached components.
        rend = this.GetComponent<Renderer>();
        coll = this.GetComponent<Collider>();
        audioSource = this.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Calculate and normalize the direction.
        direction = directionTrnsfrm.position - sourceTrnsfrm.position;
        direction.Normalize();

        // Init the bullet parameters.
        hasPlayed = false;
        shouldFollowDirection = true;
        canMove = true;
        speed = 40.0f;
        thunderChargeLevel = 0.0f;

        // Play charge sound.
        audioSource.PlayOneShot(audioCharge);

        // Start particle system.
        particle.Play();
    }

    private void Update()
    {
        MoveObject();
    }

    // Move the bullet projectile.
    private void MoveObject()
    {
        // If finished the charging the shot.
        if (thunderChargeLevel > thunderChargeTime)
        {
            // If shooting sound hasn't played yet.
            if (hasPlayed == false)
            {
                // Play shooting sound.
                audioSource.PlayOneShot(audioShoot);
                hasPlayed = true;
            }

            if (canMove)
            {
                // Move the bullet in the target's direction.
                this.transform.Translate(direction * speed * Time.deltaTime, Space.World);
                
                // If distance is large enough, follow the direction.
                if (shouldFollowDirection == true)
                {
                    // Update the shot direction.
                    direction = directionTrnsfrm.position - sourceTrnsfrm.position;
                    direction.Normalize();
                    
                    // Measure distance and disable following once reached the follow distance threshold.
                    float distance = Vector3.Distance(this.transform.position, directionTrnsfrm.position);
                    if (distance < followDistance)
                    {
                        shouldFollowDirection = false;
                    }
                }
            }
        }
        else
        {
            // Update the shot direction and increase charge level.
            direction = directionTrnsfrm.position - sourceTrnsfrm.position;
            direction.Normalize();
            thunderChargeLevel += Time.deltaTime;
        }

    }

    // Trigger event handler for bullet hit.
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "player")
        {
            // Play hit sound.
            audioSource.PlayOneShot(audioHit);
        }

        // Disable the bullet rendering and destroy it.
        canMove = false;
        speed = 0;
        //rend.enabled = false;
        //coll.enabled = false;
        particle.Stop();
        float destroyDuration;
        if (audioHit != null)
            destroyDuration = audioHit.length;
        else
            destroyDuration = 1.0f;
        //Destroy(this.gameObject, destroyDuration);
        Invoke("DisableBullet", destroyDuration);
    }

    // Disable the bullet.
    private void DisableBullet()
    {
        this.gameObject.SetActive(false);
    }
}
