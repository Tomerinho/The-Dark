using System;
using System.Collections;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    // External Game Manager.
    public GameManager GM;
    // Attached Audio Manager.
    public AudioManager audioManager;
    // Attached hero prefab.
    public Hero hero = null;
    // Attached spider prefabs.
    public Spider spider = null;
    public GameObject distortion = null;
    // Attached spider laser prefab.
    public OrbitalBeamLaserEdited laserScriptRef = null;
    // Attached pit center Transform.
    public Transform pitCenterTrnsfrm = null;
    public SwitchStartFight switchStartFight;

    // Fight control.
    public Coroutine CREndFightEffects;
    public enum endFightState { EFFECTS1, EFFECTS2, EFFECTS3, END }
    public endFightState endFightSM { get; set; } = endFightState.EFFECTS1;
    public float endFightTimer = 0.0f;
    public float lastDelayTime { get; set; } = 0.0f;
    public bool isFightRestarting { get; set; } = false;
    public bool isFightEnded { get; set; } = false;
    public bool isEffects1Called = false;
    public bool isEffects2Called = false;
    public bool isEffects3Called = false;


    // Attached prefabs.
    [SerializeField] private GameObject explosion1;
    [SerializeField] private GameObject explosion2;
    [SerializeField] private GameObject smoke;
    [SerializeField] private GameObject flicker;
    [SerializeField] private AudioClip audioExplosion1;
    [SerializeField] private AudioClip audioExplosion2;
    [SerializeField] private AudioClip audioThud;
    [SerializeField] private AudioClip audioPowerDown1;
    [SerializeField] private AudioClip audioPowerDown2;
    [SerializeField] private AudioClip audioPowerDown3;
    private AudioSource audioSourcePlayer;
    private AudioSource audioSource;
    private Transform playerTrnsfrm = null;
    private Transform spiderTrnsfrm = null;

    // End fight state machine control.
    private float endFightTimerSpeed = 1.0f;
    private float effects1Time = 3.0f;
    private float effects2Time = 5.5f;
    private float effects3Time = 19.0f;

    // Spider's shots already fired.
    private int shotCount = 0;
    // Spider's states delay times.
    private float nextDelayTime = 100.0f;
    private float walkMinTime = 2.0f;
    private float walkMaxTime = 6.0f;
    private float shootLaserTime = 6.0f;
    private float shootThunderTime = 8.0f;
    private float shootElectricTime = 8.0f;
    private float wakeTime = 10.0f;
    private float wakeToWalkTime = 0.1f;
    private float walkToShootTime = 0.1f;
    private float fallbackTime = 20.0f;
    private float shootToWalkTime = 0.1f;
    private float shootToFallbackTime = 0.1f;
    // Spider speed control.
    private float minSpeed = 0.8f;
    private float maxSpeed = 1.2f;

    // Audio control.
    private float volumeEffects = 4.0f;


    private void Awake()
    {
        // Navigational paramter for the spider.
        spider.didReachCenter = false;

        // Audio sources.
        audioSource = this.GetComponent<AudioSource>();
        audioSourcePlayer = hero.GetComponent<AudioSource>();

        // Attached prefabs' Transforms
        playerTrnsfrm = hero.GetComponent<Transform>();
        spiderTrnsfrm = spider.GetComponent<Transform>();
    }

    private void Update()
    {
        if (GM.isPaused == false)
        {
            // Update spider laser length parameters.
            SpiderLaserUpdate();

            // Update spider's state.
            ManageSpiderState();
        }
    }

    // Spider state machine management.
    void ManageSpiderState()
    {
        // If the spider died.
        if (spider.spiderHealth <= 0)
        {
            // A flag to initiate these only once.
            if (spider.spiderState != Spider.State.DEAD)
            {
                // Move on to the dead state
                spider.spiderState = Spider.State.DEAD;
                // Initiate the end fight sequence.
                EndFight();
            }
        }
        // Else, manage the spider's state machine.
        else
        {
            // If time has elapsed for the current state, move on to the next state;
            // Or, if spider hit a collider (wall, player, etc.), 
            // or it has just exited idle state,
            // or, the fight is restarting (e.g. due to a game reload), also move on to the next state.
            if ((Time.time > lastDelayTime + nextDelayTime)
                || (spider.didReachCenter == true && spider.spiderState == Spider.State.FALLBACK)
                || (spider.didHitWall == true && spider.spiderState == Spider.State.WALK)
                || (spider.isExitingIdle == true)
                || isFightRestarting == true)
            {
                lastDelayTime = Time.time;
                switch (spider.spiderState)
                {
                    case Spider.State.IDLE:
                        // If the fight has started
                        if (spider.isExitingIdle == true)
                        {
                            // Disable the exiting-idle state trigger.
                            spider.isExitingIdle = false;
                            // move on to the idle-to-walk transition.
                            spider.spiderState = Spider.State.WAKE;
                            nextDelayTime = wakeTime;
                        }

                        // If the fight restart indication has been set, unset it.
                        if (isFightRestarting == true)
                        {
                            isFightRestarting = false;
                        }
                        break;

                    case Spider.State.WAKE:
                        // Stop the distortion effect.
                        distortion.SetActive(false);

                        // Turn spider vulnerability ON.
                        spider.isVulnerable = true;

                        // Move on to the wake-to-walk transition.
                        spider.spiderState = Spider.State.WAKEtoWALK;
                        nextDelayTime = wakeToWalkTime;
                        break;

                    case Spider.State.WAKEtoWALK:
                        // Move on to walking.
                        spider.spiderState = Spider.State.WALK;
                        nextDelayTime = GetRand(walkMinTime, walkMaxTime);
                        spider.spiderSpeed = GetRand(minSpeed, maxSpeed);

                        // Disable the fight start switch.
                        spider.spiderHealth = spider.spiderMaxHealth;
                        switchStartFight.gameObject.SetActive(false);
                        break;

                    case Spider.State.WALK:
                        // Move on to the walk-to-shoot transition.
                        spider.spiderState = Spider.State.WALKtoSHOOT;
                        nextDelayTime = walkToShootTime;
                        break;

                    case Spider.State.FALLBACK:
                        // Move on to the walk-to-shoot transition.
                        spider.spiderState = Spider.State.WALKtoSHOOT;
                        nextDelayTime = walkToShootTime;
                        break;

                    case Spider.State.WALKtoSHOOT:
                        // Move on to shooting - decide according to distance from spider to target.
                        float distancePlayerSpider = Vector3.Distance(playerTrnsfrm.position, spiderTrnsfrm.position);
                        if (distancePlayerSpider < 10)
                        {
                            // Shoot an electric field.
                            spider.shotType = Spider.ShotType.ELECTRIC;
                            nextDelayTime = shootElectricTime;

                            // Turn spider invulnerable to attacks.
                            spider.isVulnerable = false;
                        }
                        else
                        {
                            // Rabdomise between laser and thunder.
                            if (UnityEngine.Random.Range(0, 2) == 0)
                            {
                                // Shoot laser.
                                spider.shotType = Spider.ShotType.LASER;
                                nextDelayTime = shootLaserTime;
                            }
                            else
                            {
                                // Shoot thunder.
                                spider.shotType = Spider.ShotType.THUNDER;
                                nextDelayTime = shootThunderTime;
                                // Reset thunder bullets counter.
                                spider.thunderBulletCount = 3;
                            }
                        }
                        spider.spiderState = Spider.State.SHOOT;
                        break;

                    case Spider.State.SHOOT:
                        // If hit a wall during walk.
                        if (spider.didHitWall == true)
                        {
                            // Fallback (but go through shoot-to-walk transition).
                            spider.spiderState = Spider.State.SHOOTtoFALLBACK;
                            nextDelayTime = shootToFallbackTime;
                        }
                        else
                        {
                            // Move on to the shoot-to-walk transition.
                            spider.spiderState = Spider.State.SHOOTtoWALK;
                            nextDelayTime = shootToWalkTime;
                        }

                        // If spider is invlunerable.
                        if (spider.isVulnerable == false)
                        {
                            // Turn spider vulnerable to attacks.
                            spider.isVulnerable = true;
                        }
                        break;

                    case Spider.State.SHOOTtoWALK:
                        // Move on to walking.
                        spider.spiderState = Spider.State.WALK;
                        nextDelayTime = GetRand(walkMinTime, walkMaxTime);
                        spider.spiderSpeed = GetRand(minSpeed, maxSpeed);
                        break;

                    case Spider.State.SHOOTtoFALLBACK:
                        // Move on to fallback.          
                        spider.spiderState = Spider.State.FALLBACK;
                        nextDelayTime = fallbackTime;
                        break;

                    default:
                        break;
                }
            }
        }
    }

    // Update spider laser length parameters.
    private void SpiderLaserUpdate()
    {
        float distanceBeamSpider = Vector3.Distance(spider.laserBeam.position, spiderTrnsfrm.position);
        int roundedUp = (int)Math.Ceiling(distanceBeamSpider);
        laserScriptRef.activeLength = Math.Max(roundedUp - 13, 2);
    }

    // End fight sequence.
    private void EndFight()
    {
        // Start a co-routine for the end fight visual and audio effects.
        CREndFightEffects = StartCoroutine(EndFightEffects());
    }

    // Co-routine for the end fight visual and audio effects.
    private IEnumerator EndFightEffects()
    {
        // While the sequence hasn't finished.
        while (isFightEnded == false)
        {
            // Increase the timer.
            endFightTimer += Time.deltaTime * endFightTimerSpeed;

            // Act according to the end fight state.
            switch (endFightSM)
            {
                case endFightState.EFFECTS1:
                    // Flag to call only once.
                    if (isEffects1Called == false)
                    {
                        // Disable spider HUD.
                        switchStartFight.spiderHUD.SetActive(false);

                        // Visual and audio effects 1.
                        EndGameEffects(1);
                        isEffects1Called = true;

                        // Play the appropriate background track.
                        audioManager.PlayTrack(2);
                        audioManager.LoopOff();
                    }

                    // If the time has ended, move on to the next state.
                    if (endFightTimer > effects1Time)
                    {
                        endFightSM = endFightState.EFFECTS2;
                    }
                    break;

                case endFightState.EFFECTS2:
                    // Flag to call only once.
                    if (isEffects2Called == false)
                    {
                        // Visual and audio effects 2.
                        EndGameEffects(2);
                        isEffects2Called = true;
                        
                        // Disable spider lighting.
                        switchStartFight.redGlow.intensity = 1.0f;
                        switchStartFight.spiderLightP1.SetActive(false);
                    }

                    // If the time has ended, move on to the next state.
                    if (endFightTimer > effects2Time)
                    {
                        endFightSM = endFightState.EFFECTS3;
                    }
                    break;

                case endFightState.EFFECTS3:
                    // Flag to call only once.
                    if (isEffects3Called == false)
                    {
                        // Visual and audio effects 3.
                        EndGameEffects(3);
                        isEffects3Called = true;
                    }

                    // If the time has ended, move on to the next state.
                    if (endFightTimer > effects3Time)
                    {
                        endFightSM = endFightState.END;
                    }
                    break;

                case endFightState.END:
                    // Inidicate the end of the fight and the sequence.
                    isFightEnded = true;

                    // Call the credits screen.
                    GM.Credits();
                    break;

                default:
                    break;
            }

            yield return null;
        }
    }

    // Visual and audio effects according to caller's choice.
    private void EndGameEffects(int choice)
    {
        // Select the desired effect.
        switch (choice)
        {
            case 1:
                explosion1.gameObject.SetActive(true);
                audioSource.PlayOneShot(audioExplosion1, volumeEffects);
                audioSource.PlayOneShot(audioPowerDown1, volumeEffects * 2.0f);
                break;

            case 2:
                explosion2.gameObject.SetActive(true);
                audioSource.PlayOneShot(audioExplosion1, volumeEffects);
                audioSource.PlayOneShot(audioExplosion2, volumeEffects);
                audioSource.PlayOneShot(audioPowerDown2, volumeEffects * 2.0f);
                break;

            case 3:
                smoke.gameObject.SetActive(true);
                flicker.gameObject.SetActive(true);
                audioSource.PlayOneShot(audioThud, volumeEffects);
                audioSource.PlayOneShot(audioPowerDown3, volumeEffects * 2.0f);
                break;

            default:
                break;
        }
    }

    // Stops the visual and audio effects.
    public void StopEndGameEffects()
    {
        explosion1.gameObject.SetActive(false);
        explosion2.gameObject.SetActive(false);
        smoke.gameObject.SetActive(false);
        flicker.gameObject.SetActive(false);
    }

    // Returns a random number from a given range.
    private float GetRand(float minDelay, float maxDelay)
    {
        return UnityEngine.Random.Range(minDelay, maxDelay);
    }
}