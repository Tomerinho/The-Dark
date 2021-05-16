using UnityEngine;

public class Spider : MonoBehaviour
{
    // External Game Manager.
    public GameManager GM;
    // Attached electric field prefab.
    public GameObject electricField = null;
    public GameObject electricFieldInner = null;
    // Attached laser beam prefabs.
    public GameObject spiderLaser = null;
    public Transform laserBeam = null;
    // Attached player prefab.
    public Transform playerTrnsfrm = null;
    // Attached fallback prefab.
    public Transform fallbackTrnsfrm = null;
    // Attached spider health bar script.
    public Healthbar spiderHealthbarScript;
    // Attached audio clips.
    public AudioClip audioHit = null;
    public AudioClip audioWalk = null;
    public AudioClip audioElectricityStop = null;


    // Spider's state machine type.
    public enum State { IDLE, WAKE, WAKEtoWALK, WALK, FALLBACK, WALKtoSHOOT, SHOOT, SHOOTtoWALK, SHOOTtoFALLBACK, DEAD }
    // Spider's state machine.
    public State spiderState { get; set; } = State.IDLE;
    // Thunder bullet control.
    public enum ShotType { LASER, THUNDER, ELECTRIC }
    public ShotType shotType { get; set; } = ShotType.LASER;
    public int thunderBulletCount = 3;
    // Spider's HP.
    public int spiderHealth { get; set; } = 48;
    public int spiderMaxHealth = 1000;
    public int spiderHealthRegenSpeed = 32;
    public bool isVulnerable = false;
    // Spider movement speed control.
    public float baseMovementSpeed { get; set; } = 5.0f;
    public float spiderSpeed { get; set; } = 1.0f;
    // Spider animation control.
    public bool isDeadAnimTriggered = false;
    // Spider collision and state machine controls.
    public bool didReachCenter = false;
    public bool didHitWall = false;
    public bool isExitingIdle = false;
    // Electricity parameters.
    public bool isElectricityOn = false;


    // Audio component.
    private AudioSource audioSource = null;
    // Laser gun socket component.
    private Transform laserSocket = null;
    //private Rigidbody myRigidBody = null;
    private Animator myAnimator = null;
    private Collider laserBeamCollider = null;
    private Collider electricFieldCollider = null;
    private OrbitalBeamLaserEdited laserBeamScriptRef;

    // Laser hit location and speed parameters.
    //private Vector3 vecPlayerHeight = new Vector3(0, -0.5f, 0);
    private Vector3 vecPlayerHeight = new Vector3(0, 0, 0);
    private Vector3 forward;
    private Vector3 left;
    private Vector3 right;
    private int hitDistance = 15;
    private int randLeftRight;
    private float laserSpeed = 0.02f;
    // Thunder bullets weapon parameters.
    private float shotTimer = 0.0f;
    private float bulletsPerSecond = 0.5f;
    private float fireRate = 0.0f;
    // Spider's movement parameters.
    private float rotateSpeed = 2.0f;
    private float walkSoundTimer = 0.0f;
    private float walkSoundTime = 2.0f;
    // Spider's animation control.
    private float idleToWalkEndSpeed = 1.0f;
    private float idleToWalkBlendSpeed = 0.2f;
    private float walktoShootEndSpeed = 0.0f;
    private float walktoShootBlendSpeed = 0.2f;
    private float blendAnimSpeed = 0.0f;


    private void Awake()
    {
        // Init spider's health bar.
        spiderHealthbarScript.health = spiderHealth;

        // Init laser beam position and turn it off.
        laserBeamScriptRef = spiderLaser.GetComponent<OrbitalBeamLaserEdited>();
        laserBeamScriptRef.LaserActive = false;
        laserSocket = this.transform.GetChild(30).GetChild(0);
        laserBeam = this.transform.GetChild(35);
        laserBeamCollider = laserBeam.GetComponent<Collider>();

        // Init thunder bullets weapon parameters.
        fireRate = 1 / bulletsPerSecond;
        shotTimer = fireRate;

        // Init electric field.
        electricField.SetActive(false);
        electricFieldInner.SetActive(false);
        electricFieldCollider = electricField.GetComponent<Collider>();

        // Init the spider's Animator component.
        myAnimator = GetComponent<Animator>();
        myAnimator.enabled = false;

        // Init the audio component.
        audioSource = this.GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (GM.isPaused == false)
        {
            // Update spider's health bar.
            spiderHealthbarScript.health = spiderHealth;

            // Resolve the spider's state each frame.
            ActOnSpiderState();
        }
    }

    // Resolve the spider's state and act appropriately.
    private void ActOnSpiderState()
    {
        switch (spiderState)
        {
            case State.IDLE:
                Idle();
                break;
            case State.WAKE:
                Wake();
                break;
            case State.WAKEtoWALK:
                WakeToWalk(playerTrnsfrm);
                break;
            case State.WALK:
                Walk(playerTrnsfrm);
                break;
            case State.FALLBACK:
                Walk(fallbackTrnsfrm);
                break;
            case State.WALKtoSHOOT:
                WalkToShoot(playerTrnsfrm);
                break;
            case State.SHOOT:
                Shoot(playerTrnsfrm);
                break;
            case State.SHOOTtoWALK:
                ShootToWalk(playerTrnsfrm);
                break;
            case State.SHOOTtoFALLBACK:
                ShootToWalk(fallbackTrnsfrm);
                break;
            case State.DEAD:
                Dead();
                break;
            default:
                break;
        }
    }

    // Spider idle animation.
    private void Idle()
    {
        // Turn off weapons.
        TurnLaserOff();
        TurnElectricFieldOff();

        // Turn off the Animator and reset its state machine.
        if (myAnimator.enabled == true)
        {
            myAnimator.Play("Wake");
            myAnimator.enabled = false;
        }
    }

    // Spider wake-up animation.
    private void Wake()
    {
        // Turn on the Animator and kick-off its state machine.
        myAnimator.enabled = true;
    }

    // Transition from idle to walking.
    private void WakeToWalk(Transform trnsfrm)
    {
        // Gradually transition into walk animation.
        if (myAnimator)
        {
            if (blendAnimSpeed < idleToWalkEndSpeed)
            {
                blendAnimSpeed += idleToWalkBlendSpeed;
            }
            myAnimator.SetFloat("speed", blendAnimSpeed);
        }

        // Start following the player.
        FollowPlayer(rotateSpeed);
    }

    // Walk towards the player.
    private void Walk(Transform trnsfrm)
    {
        // Continue gradually transitioning into walk animation.
        if (myAnimator)
        {
            if (blendAnimSpeed < idleToWalkEndSpeed)
            {
                blendAnimSpeed += idleToWalkBlendSpeed;
            }
            myAnimator.SetFloat("speed", blendAnimSpeed);
        }

        // Follow and move towards the player.
        FollowPlayer(rotateSpeed * 4.0f);
        Vector3 direction = trnsfrm.position - this.transform.position;
        direction.y = 0;
        this.transform.Translate(direction.normalized * baseMovementSpeed * spiderSpeed * Time.deltaTime, Space.World);

        // Play the walking audio.
        PlayWalkingAudio();
    }

    // Transition from walking to shooting.
    private void WalkToShoot(Transform trnsfrm)
    {
        // Gradually transition into shoot animation.
        if (myAnimator)
        {
            if (blendAnimSpeed > walktoShootEndSpeed)
            {
                blendAnimSpeed -= walktoShootBlendSpeed;
            }
            myAnimator.SetFloat("speed", blendAnimSpeed);
        }

        FollowPlayer(rotateSpeed);
    }

    // Shooting state.
    private void Shoot(Transform trnsfrm)
    {
        // Fire the chosen shot type.
        switch (shotType)
        {
            case ShotType.LASER:
                FireLaser(trnsfrm);
                break;
            case ShotType.THUNDER:
                FireThunder();
                break;
            case ShotType.ELECTRIC:
                FireElectricity();
                break;
            default:
                break;
        }

        // Look towards the player.
        FollowPlayer(rotateSpeed);

        // Continue gradually transitioning into shoot animation.
        if (myAnimator)
        {
            if (blendAnimSpeed > walktoShootEndSpeed)
            {
                blendAnimSpeed -= walktoShootBlendSpeed;
            }
            myAnimator.SetFloat("speed", blendAnimSpeed);
        }
    }

    // Transition from shooting to walking.
    private void ShootToWalk(Transform trnsfrm)
    {
        TurnLaserOff();
        TurnElectricFieldOff();

        if (myAnimator)
        {
            // Gradually transition into walk animation.
            if (blendAnimSpeed < idleToWalkEndSpeed)
            {
                blendAnimSpeed += idleToWalkBlendSpeed;
            }
            myAnimator.SetFloat("speed", blendAnimSpeed);
        }

        FollowPlayer(rotateSpeed);
    }

    // Fire the spider's laser, spawning randomly near the target, and following the target.
    private void FireLaser(Transform trnsfrm)
    {
        if (!laserBeamScriptRef.LaserActive)
        {
            // Init randomization and positional variables.
            randLeftRight = Random.Range(0, 2);
            forward = (trnsfrm.position + vecPlayerHeight) - laserSocket.position;
            left = Quaternion.AngleAxis(90, Vector3.up) * forward.normalized;
            right = Quaternion.AngleAxis(-90, Vector3.up) * forward.normalized;
            left.Scale(new Vector3(hitDistance, hitDistance, hitDistance));
            right.Scale(new Vector3(hitDistance, hitDistance, hitDistance));

            // Calculate the laser beam's initial position.
            if (randLeftRight == 0)
            {
                laserBeam.position = trnsfrm.position + left + vecPlayerHeight;
            }
            else
            {
                laserBeam.position = trnsfrm.position + right + vecPlayerHeight;
            }
            laserBeam.LookAt(laserSocket.position);
            laserBeam.Rotate(new Vector3(90, 0, 0));
                
            // Turn laser beam ON.
            laserBeamScriptRef.LaserActive = true;
            laserBeamCollider.enabled = true;
        }

        // Create an offset for the target in the opposite direction of the laser's starting point.
        Vector3 offsetLeftRight;
        if (randLeftRight == 0)
        {
            offsetLeftRight = Quaternion.AngleAxis(-90, Vector3.up) * forward.normalized;
        }
        else
        {
            offsetLeftRight = Quaternion.AngleAxis(90, Vector3.up) * forward.normalized;
        }
        offsetLeftRight.Scale(new Vector3(hitDistance * 0.1f, hitDistance * 0.1f, hitDistance * 0.1f));
        // Create an offset for the target on its back.
        Vector3 offsetBack;
        offsetBack = (trnsfrm.position + vecPlayerHeight - this.transform.position).normalized;
        offsetBack.Scale(new Vector3(hitDistance * 0.5f, hitDistance * 0.5f, hitDistance * 0.5f));
        // Move the laser beam towards the player.
        laserBeam.position = Vector3.Lerp(laserBeam.position, trnsfrm.position + offsetLeftRight + offsetBack + vecPlayerHeight, laserSpeed);
        laserBeam.LookAt(laserSocket.position);
        laserBeam.Rotate(new Vector3(90, 0, 0));
    }

    // Turn the spider's laser OFF.
    private void TurnLaserOff()
    {
        // Turn the laser OFF, in the case of coming from an ON state.
        if (laserBeamScriptRef.LaserActive)
        {
            laserBeamScriptRef.LaserActive = false;
            laserBeamCollider.enabled = false;
        }
    }

    // Fire an electricity field around the spider.
    private void FireElectricity()
    {
        if (isElectricityOn == false)
        {
            // Activate electric field.
            electricField.SetActive(true);
            electricFieldInner.SetActive(true);
            electricFieldCollider.enabled = true;
            isElectricityOn = true;
        }
    }

    // Turn the electric field OFF.
    private void TurnElectricFieldOff()
    {
        if (isElectricityOn == true)
        {
            electricField.SetActive(false);
            electricFieldInner.SetActive(false);
            electricFieldCollider.enabled = false;
            audioSource.PlayOneShot(audioElectricityStop);
            isElectricityOn = false;
        }
    }

    // Fire the spider's thunder bullet, directed at the target.
    private void FireThunder()
    {
        // Timer counting until the next shot is allowed.
        shotTimer += Time.deltaTime;
        // If time has elapsed.
        if (shotTimer >= fireRate)
        {
            if (thunderBulletCount > 0)
            {
                // Fire the thunder bullet.
                //GameObject objThunder = Instantiate(thunderBullet, laserSocket.position, laserSocket.rotation) as GameObject;
                GameObject objThunder = ObjectPoolEnemy.SharedInstance.GetPooledObject();
                if (objThunder != null)
                {
                    objThunder.name = "thunderBullet";
                    objThunder.transform.position = laserSocket.position;
                    objThunder.transform.rotation = laserSocket.rotation;
                    objThunder.SetActive(true);
                }
                shotTimer = 0;
                thunderBulletCount--;
            }
        }
    }

    // Gradually rotate towards the player.
    private void FollowPlayer(float rotSpeed)
    {
        // Form a vector of the position difference between the player and the spider.
        Vector3 relativePos = playerTrnsfrm.position - this.transform.position;
        // Zero-out the Y component of the vector.
        relativePos = Vector3.Scale(relativePos, new Vector3(1, 0, 1));
        // Create a rotation using the relative position vector.
        Quaternion toRotation = Quaternion.LookRotation(relativePos);
        // Rotate the spider slowly towards that rotation.
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, toRotation, rotSpeed * Time.deltaTime);
    }

    // Play the walking audio.
    private void PlayWalkingAudio()
    {
        // If the walking sound timer hasn't finished.
        if (walkSoundTimer < walkSoundTime)
        {
            // Increment the walking sound timer according to the movement speed.
            walkSoundTimer += Time.deltaTime * baseMovementSpeed * spiderSpeed;
        }
        else
        {
            // Play the walking audio
            audioSource.PlayOneShot(audioWalk, 18.0f);
            walkSoundTimer = 0;
        }
    }

    // Upon defeating the spider, i.e. entering the Dead state.
    private void Dead()
    {
        // Turn off weapons.
        TurnLaserOff();
        TurnElectricFieldOff();

        // Turn-off vulnerability.
        isVulnerable = false;

        // If the Animator is enabled and the dead animation wasn't triggered yet.
        if (myAnimator && isDeadAnimTriggered == false)
        {
            // Transition into the dead animation.
            myAnimator.SetFloat("speed", 100);
            isDeadAnimTriggered = true;
        }
    }

    // Stay collider event handler.
    private void OnTriggerStay(Collider other)
    {
        // Upon reaching the pit center.
        if (other.name == "pitCenter")
        {
            // Indicate reaching the trigger.
            didReachCenter = true;
        }

        // Upon hitting a wall or reaching the spider area's limit.
        if (other.name == "spiderLimit")
        {
            // Indicate reaching the trigger.
            didHitWall = true;
        }
    }

    // Enter collider event handler.
    private void OnTriggerEnter(Collider other)
    {
        // Upon getting hit by a player's weapon
        if (isVulnerable == true && other.name == "bulletBlue")
        {
            // Reduce spider's hp by the collider's hitpoint.
            int hp = LaserBullet.hitpoint;
            GetSpiderHealth(hp);

            // Play hit sound.
            audioSource.PlayOneShot(audioHit, 3.0f);
        }
    }

    // Exit trigger event handler.
    private void OnTriggerExit(Collider other)
    {
        // Upon exiting the pit center.
        if (other.name == "pitCenter")
        {
            // Indicate exiting the trigger.
            didReachCenter = false;
        }

        // Upon exiting a wall or spider area's limit trigger.
        if (other.name == "spiderLimit")
        {
            // Indicate exiting the trigger.
            didHitWall = false;
        }
    }

    // Reduce spider's hp, or destroy spider if health is 0.
    private void GetSpiderHealth(int hp)
    {
        if (spiderHealth > 0)
        {
            spiderHealth -= hp;
        }
    }
}
