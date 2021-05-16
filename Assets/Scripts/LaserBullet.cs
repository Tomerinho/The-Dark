using UnityEngine;

public class LaserBullet : MonoBehaviour
{
    // Attached audio components.
    public AudioClip audioShoot = null;
    public AudioClip audioHit = null;
    // Attached particle system.
    public ParticleSystem particle = null;
    // Laser bullet's hitpoint, ammo cost and travel speed.
    //public int hitpoint = 40;
    //public float ammoCost = 800.0f;
    //public float speed = 20.0f;
    public static int hitpoint { get; set; } = 20;
    public static float ammoCost { get; set; } = 150.0f;
    public float speed { get; set; } = 40.0f;


    // Attached components.
    private Renderer rend;
    private Collider coll;
    private AudioSource audioSource;

    // Bullet source and direction parameters.
    private Camera cam;
    private Vector3 sourcePos;
    // Audio control.
    private float audioHitVolume = 8.0f;
    private bool didPlayHit = false;
    // Movement enabled.
    private bool canMove = true;


    private void Awake()
    {   
        // Init bullet source and direction parameters.
        sourcePos = this.transform.position;
        cam = Camera.main;

        // Init attached components.
        rend = this.GetComponent<Renderer>();
        coll = this.GetComponent<Collider>();
        audioSource = this.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Init bullet source and direction parameters.
        sourcePos = this.transform.position;

        // Init the bullet parameters.
        didPlayHit = false;
        canMove = true;
        speed = 40.0f;

        // Play shooting sound.
        audioSource.PlayOneShot(audioShoot);

        // Start particle system.
        particle.Play();
    }

    private void Update()
    {
        // Move the bullet projectile.
        MoveObject();
    }

    // Move the bullet projectile.
    private void MoveObject()
    {
        if (canMove)
        {
            // Create a ray from the camera going through the middle of the screen.
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            // Ray hits only the enemy and default layers.
            int layerMask = 0b00000000000000000000001000000001;
            // Check whether pointing to something so as to adjust the direction.
            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit, 25.0f, layerMask))
                targetPoint = hit.point;
            else
                targetPoint = ray.GetPoint(1000);

            // Calculate and normalize the direction.
            Vector3 direction = targetPoint - sourcePos;
            direction.Normalize();

            // Move the bullet in the target's direction.
            this.transform.Translate(direction, Space.World);
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        // Play hit sound once.
        if (didPlayHit == false)
        {
            didPlayHit = true;
            audioSource.PlayOneShot(audioHit, audioHitVolume);
        }
    }

    // Trigger event handler for bullet hit.
    private void OnTriggerEnter(Collider other)
    {
        // Disable the bullet rendering and destroy it.
        canMove = false;
        speed = 0;
        //rend.enabled = false;
        //coll.enabled = false;
        //particle.Stop();
        float destroyDuration;
        if (audioHit != null)
            destroyDuration = audioHit.length;
        else
            destroyDuration = 1.0f;
        //Destroy(this.gameObject, destroyDuration);
        //Invoke("DisableBullet", destroyDuration);
    }

    // Disable the bullet.
    private void DisableBullet()
    {
        this.gameObject.SetActive(false);
    }
}
