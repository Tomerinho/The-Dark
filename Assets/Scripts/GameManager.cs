using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityStandardAssets.Characters.FirstPerson;

public class GameManager : MonoBehaviour
{
    // Game control.
    public static bool isCheckpointReached = false;
    public bool isPaused = false;
    public bool isGameOver = false;
    public bool isCreditsOn = false;


    // Attached components.
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject spider;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private FightManager FightManager;
    [SerializeField] private SwitchStartFight switchStartFight;
    [SerializeField] private SwitchDestroySS2 switchDestroySS2;
    [SerializeField] private ScreenFade screenFade;
    [SerializeField] private EasyEndCredits creditsRoll;
    [SerializeField] private VideoPlayer videoPlayer;
    private Transform playerTrnsfrm;
    private Transform spiderTrnsfrm;
    private Hero playerHeroRef;
    private Spider spiderRef;
    private CharacterController cController;
    private FirstPersonController fpController;
    private MySceneManager mySceneManager;
    private AudioManager audioManager;

    // End game control.
    private float endGameTime = 10.0f;
    private float endGameTimer = 0.0f;
    private float endGameTimerSpeed = 1.5f;
    private FightManager.endFightState EFFECTS1;

    private void Awake()
    {
        // Init the first sub-scene and disable the rest.
        mySceneManager = this.GetComponent<MySceneManager>();
        mySceneManager.EnableSubScene(0);
        for (int i = 1; i < mySceneManager.size; i++)
            mySceneManager.DisableSubScene(i);

        // Init attached components.
        audioManager = this.GetComponent<AudioManager>();
        playerTrnsfrm = player.GetComponent<Transform>();
        spiderTrnsfrm = spider.GetComponent<Transform>();
        playerHeroRef = player.GetComponent<Hero>();
        spiderRef = spider.GetComponent<Spider>();
        cController = player.GetComponent<CharacterController>();
        fpController = player.GetComponent<FirstPersonController>();

        // Save the game on game start.
        SaveGame();
    }

    private void Update()
    {
        // Manages Pause and un-Pause.
        PauseGame();
    }

    // Save the game including all essential positions and parameters.
    public void SaveGame()
    {
        // Create a game save instance and add essential positions to the list.
        GameSave save = new GameSave();
        save.playerPos = (playerTrnsfrm.position.x, playerTrnsfrm.position.y, playerTrnsfrm.position.z);
        save.playerRot = (playerTrnsfrm.rotation.x, playerTrnsfrm.rotation.y, playerTrnsfrm.rotation.z, playerTrnsfrm.rotation.w);
        save.spiderPos = (spiderTrnsfrm.position.x, spiderTrnsfrm.position.y, spiderTrnsfrm.position.z);
        save.spiderRot = (spiderTrnsfrm.rotation.x, spiderTrnsfrm.rotation.y, spiderTrnsfrm.rotation.z, spiderTrnsfrm.rotation.w);

        // Save the data to a file.
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/tmp_gamesave.save");
        bf.Serialize(file, save);
        file.Close();
    }

    // Load a game save including all essential positions and parameters.
    public void LoadGame()
    {
        // If the fight has started already.
        if (isCheckpointReached == true)
        {
            // Check for a temporary game save.
            if (File.Exists(Application.persistentDataPath + "/tmp_gamesave.save"))
            {
                // Load the data from file to a game save instance.
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/tmp_gamesave.save", FileMode.Open);
                GameSave save = (GameSave)bf.Deserialize(file);
                file.Close();

                // Disable the player's controllers.
                cController.enabled = false;
                //fpController.enabled = false;
                fpController.isLoading = false;
                // Load the player's essential positions and parameters.
                playerTrnsfrm.position = new Vector3(save.playerPos.x, save.playerPos.y, save.playerPos.z);
                playerTrnsfrm.rotation = new Quaternion(save.playerRot.x, save.playerRot.y, save.playerRot.z, save.playerRot.w);
                playerHeroRef.heroHealth = save.playerHealth;
                // Enable the player's controllers.
                cController.enabled = true;
                //fpController.enabled = true;
                fpController.isLoading = true;

                // Load the spider's essential positions and parameters.
                spiderTrnsfrm.position = new Vector3(save.spiderPos.x, save.spiderPos.y, save.spiderPos.z);
                spiderTrnsfrm.rotation = new Quaternion(save.spiderRot.x, save.spiderRot.y, save.spiderRot.z, save.spiderRot.w);
                spiderRef.spiderState = save.spiderState;
                spiderRef.spiderHealth = save.spiderHealth;
                spiderRef.isVulnerable = save.isVulnerable;
                spiderRef.didReachCenter = save.didReachCenter;
                spiderRef.didHitWall = save.didHitWall;
                spiderRef.isExitingIdle = save.isExitingIdle;
                spiderRef.isDeadAnimTriggered = save.isDeadAnimTriggered;

                // Indicate the fight is restarting and init the fight and switch parameters.
                if (FightManager.CREndFightEffects != null)
                {
                    StopCoroutine(FightManager.CREndFightEffects);
                }
                FightManager.isFightRestarting = true;
                FightManager.lastDelayTime = 0.0f;
                FightManager.isFightEnded = false;
                FightManager.endFightTimer = 0.0f;
                FightManager.endFightSM = FightManager.endFightState.EFFECTS1;
                FightManager.StopEndGameEffects();
                FightManager.isEffects1Called = false;
                FightManager.isEffects2Called = false;
                FightManager.isEffects3Called = false;
                if (switchStartFight.IncreaseLightIntensityCR != null)
                {
                    StopCoroutine(switchStartFight.IncreaseLightIntensityCR);
                }
                if (switchStartFight.SpiderHealthRegenCR != null)
                {
                    StopCoroutine(switchStartFight.SpiderHealthRegenCR);
                }
                if (switchStartFight.AudioSpiderWakeCR != null)
                {
                    StopCoroutine(switchStartFight.AudioSpiderWakeCR);
                }
                switchStartFight.gameObject.SetActive(true);
                switchStartFight.isFightStarted = false;
                switchStartFight.LEDs.gameObject.SetActive(true);
                switchStartFight.spiderLightP1.SetActive(false);
                switchStartFight.distortion.SetActive(false);
                switchStartFight.isFinishedLightingUp = false;
                switchStartFight.didReachMaxGlow = false;
                switchStartFight.lightSource.intensity = 0.0f;
                switchStartFight.spiderHUD.SetActive(false);
                switchStartFight.isRegenStarted = false;
                switchStartFight.redGlow.intensity = 0.0f;
                switchStartFight.hasPlayedWake2 = false;
                switchStartFight.countAudio = 0;
                switchStartFight.walkAudioTimer = 1.8f;
                switchDestroySS2.isTriggered = false;
                if (screenFade.CRFadeIn != null)
                {
                    StopCoroutine(screenFade.CRFadeIn);
                }
                screenFade.SetTransparent();
                screenFade.gameObject.SetActive(false);
                isCreditsOn = false;
                creditsRoll.Stop();
                creditsRoll.outlineSpeed = 0.2f;
                creditsRoll.gameObject.SetActive(false);

                // Play the appropriate background music.
                audioManager.PlayTrack(0);
                audioManager.LoopOn();
                // Increase the game Audio volume.
                audioManager.IncreaseGlobalAudio();
            }
            else
            {
                Debug.Log("No game saved!");
            }
        }
        else
        {
            // Re-load the scene.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // Exit the game.
    public void QuitGame()
    {
        Application.Quit();
    }

    // Pause the game.
    public void Pause()
    {
        // Pause the time.
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0;
        }

        // Pause the Audio.
        AudioListener.pause = true;
        // Pause the Video.
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }

        // Open-up the menu.
        pauseMenu.SetActive(true);
        isPaused = true;
    }

    // Un-pause the game.
    public void UnPause()
    {
        // Un-pause the time.
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
        }

        // Un-pause the Audio.
        AudioListener.pause = false;
        // Un-pause the Video.
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }

        // Close the menu.
        pauseMenu.SetActive(false);
        isPaused = false;
    }

    // Manages the game over sequence.
    public void GameOver()
    {
        // Pause the game.
        isGameOver = true;
        isPaused = true;

        // Stop the background music.
        audioManager.StopTrack();
        // Decrease game Audio volume.
        audioManager.DecreaseGlobalAudio();

        // Start a co-routine to end the game.
        endGameTimer = 0.0f;
        StartCoroutine("CRGameOver");
    }

    // Co-routine for the end game sequence..
    private IEnumerator CRGameOver()
    {
        // While haven't finished the sequence.
        while (endGameTimer < endGameTime)
        {
            // Turn the screen red.
            canvasGroup.alpha = 1.0f;

            // Increase the timer.
            endGameTimer += Time.deltaTime * endGameTimerSpeed;

            yield return null;
        }

        // If the sequence has finished.
        if (endGameTimer >= endGameTime)
        {
            // Re-load the game.
            LoadGame();
            UnPause();
            isGameOver = false;

            // Remove the red screen.
            canvasGroup.alpha = 0.0f;
        }
    }

    // Manages Pause and un-Pause.
    private void PauseGame()
    {
        // Upon pressing the Esc button, toggle the Pause menu.
        if (Input.GetKeyDown(KeyCode.Escape) && isGameOver == false)
        {
            if (isPaused == false)
            {
                // Pause the game.
                Pause();
            }
            else
            {
                // Un-pause the game.
                UnPause();
            }
        }
    }

    // Manages the end game credits dispaly.
    public void Credits()
    {
        isCreditsOn = true;

        // Stop all running effects and sounds.
        FightManager.StopEndGameEffects();

        // Fade-in the credits screen image.
        screenFade.gameObject.SetActive(true);
        screenFade.FadeIn();

        // Play the credits roll.
        creditsRoll.gameObject.SetActive(true);
        creditsRoll.Play();

        // Play the appropriate background track.
        audioManager.PlayTrack(3);
        audioManager.LoopOff();
    }
}
