using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static bool cannotAct;

    [Header("References")]
    [SerializeField] Animator sceneFadeAnimator;
    [SerializeField] TMPro.TextMeshProUGUI countdownText;
    [SerializeField] PauseMenu pauseMenu;
    [SerializeField] HUDManager hudManager;
    [SerializeField] GameObject stageClearedText;
    [SerializeField] GameObject countdownSFX;

    [Header("Level References")]
    [SerializeField] GameObject checkpointsParent;
    [SerializeField] GameObject collectiblesParent;
    [SerializeField] PlayerMovement player;
    [SerializeField] GhostPlayer ghostPlayer;
    //[SerializeField] MovingSpikes movingSpikes;

    [Header("Songs")]
    [SerializeField] AudioClip stageSong;
    [SerializeField] AudioClip levelEndSong;
    [SerializeField] AudioClip levelFailSong;
    [SerializeField] float normalVolume = 1;
    [SerializeField] float pausedVolume = 0.5f;

    [Header("Level Settings")]
    [SerializeField] bool isATimeTrialStage = false;

    List<Checkpoint> checkpoints = new List<Checkpoint>();
    List<BigCoin> bigCoins = new List<BigCoin>();

    Checkpoint currentCheckpoint;
    bool respawning = false;
    StageSave currentStageSave;

    AudioSource audioSource;
    public float coinAmount { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        //Application.targetFrameRate = 60; //just for testing purposes
        Time.timeScale = 1;
        audioSource = GetComponent<AudioSource>();
        if (isATimeTrialStage)
        {
            //Freeze time to begin with
            Time.timeScale = 0;
        }
        else
        {
            PlayAudioClip(stageSong, true);
        }
        AssignEvents();
        //Load save for this stage
        string sceneName = SceneManager.GetActiveScene().name;
        SaveData data = SaveSystem.Load();
        currentStageSave = new StageSave(sceneName, Enumerable.Repeat(0, bigCoins.Count).ToList());
        if (data != null)
        {
            //If the save exists, store it
            StageSave save = data.stagesSaved.Find(scene => scene.stageName == sceneName);
            if (save != null)
            {
                currentStageSave = save;
            }
        }
        currentCheckpoint = checkpoints[0];
        player.Respawn(currentCheckpoint.transform.position);
        hudManager.InitialiseUI(currentStageSave.bigCoinsFound, player.MaxHealth, 0);
        StartCoroutine(WaitForSceneFade());
    }

    void PlayAudioClip(AudioClip song, bool loop)
    {
        audioSource.time = 0;
        audioSource.clip = song;
        audioSource.Play();
        audioSource.loop = loop;
    }

    void AssignEvents()
    {
        pauseMenu.Quit += QuitLevel;
        pauseMenu.RetryStage += RetryLevel;
        //Checkpoint events
        checkpoints = checkpointsParent.GetComponentsInChildren<Checkpoint>().ToList();
        List<Collectible> allCollectibles = collectiblesParent.GetComponentsInChildren<Collectible>().ToList();
        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (!checkpoint.isEndFlag)
            {
                checkpoint.CheckpointReached += UpdateCurrentCheckpoint;
            }
            else
            {
                //Final flag ends the level
                checkpoint.CheckpointReached += EndLevel;
            }
        }

        //Collectible events on pick up
        foreach (Collectible collectible in allCollectibles)
        {
            switch (collectible.GetCollectibleType())
            {
                case Collectible.CollectibleType.Coin:
                    collectible.PickedUp += UpdateCoinAmount;
                    break;
                case Collectible.CollectibleType.BigCoin:
                    BigCoin bigCoin = (BigCoin)collectible;
                    bigCoin.PickedUpBigCoin += FoundBigCoin;
                    bigCoins.Add(bigCoin);
                    break;
            }
        }
        //Player events
        player.Healed += UpdateHealth;
        player.Hit += UpdateHealth;
        player.Death += Respawn;
    }

    IEnumerator WaitForSceneFade()
    {
        cannotAct = true;
        yield return new WaitForEndOfFrame();
        //if (movingSpikes) movingSpikes.SetPosition();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        if (isATimeTrialStage)
        {
            //Start countdown
            StartCoroutine(StartCountdown());
        }
        else
        {
            cannotAct = false;
        }
    }

    IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);
        Instantiate(countdownSFX); //countdown sound effect
        //Countdown from 3, 2, 1
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1);
        }
        countdownText.gameObject.SetActive(false);
        Time.timeScale = 1;
        //Start level
        ghostPlayer.StartMoving();
        PlayAudioClip(stageSong, true);
        cannotAct = false;
    }

    private void Update()
    {
        if (pauseMenu.paused)
        {
            audioSource.volume = pausedVolume;
        }
        else
        {
            audioSource.volume = normalVolume;
        }
    }

    void UpdateCurrentCheckpoint(Checkpoint checkpoint)
    {
        currentCheckpoint = checkpoint;
    }

    void UpdateHealth(float health)
    {
        //Update the UI for health
        hudManager.UpdateHealthAmount(health);
    }

    void UpdateCoinAmount()
    {
        coinAmount += 1;
        hudManager.UpdateCoinAmount(coinAmount);
    }

    void FoundBigCoin(BigCoin bigCoinFound)
    {
        int bigCoinIndex = bigCoins.IndexOf(bigCoinFound);
        hudManager.UpdateBigCoinIndicator(bigCoinIndex);
        currentStageSave.bigCoinsFound[bigCoinIndex] = 1; //set to found
    }

    void Respawn()
    {
        if (!respawning)
        {
            respawning = true;
            StartCoroutine(RespawnAfterFade());
        }

    }

    IEnumerator RespawnAfterFade()
    {
        //Fade out, and put the player at their last respawn point.
        cannotAct = true;
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        hudManager.UpdateHealthAmount(player.MaxHealth);
        player.Respawn(currentCheckpoint.transform.position);
        yield return new WaitForSeconds(0.5f);
        //if (movingSpikes) movingSpikes.SetPosition();
        sceneFadeAnimator.SetTrigger("FadeIn");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        cannotAct = false;
        respawning = false;
    }

    void EndLevel(Checkpoint endFlag)
    {
        //check if it was reached by a ghost
        if (endFlag.reachedByGhost)
        {
            StartCoroutine(FailedStage());
        }
        else
        {
            //Player reached end of stage
            StartCoroutine(GoNextLevelAfterFade());
        }
    }

    IEnumerator FailedStage()
    {
        //Fade out, save, then load stage select
        PlayAudioClip(levelFailSong, false); //play lose sound
        cannotAct = true;
        Time.timeScale = 0;
        yield return new WaitUntil(() => audioSource.time >= audioSource.clip.length - 0.5f);
        pauseMenu.OpenRetryMenu();
    }

    IEnumerator GoNextLevelAfterFade()
    {
        //Fade out, save, then load stage select
        player.ReachedEndOfLevel();
        yield return new WaitUntil(() => player.onGround);
        PlayAudioClip(levelEndSong, false);
        stageClearedText.SetActive(true);
        cannotAct = true;
        Time.timeScale = 0;
        SaveSystem.Save(currentStageSave);
        yield return new WaitUntil(() => audioSource.time >= audioSource.clip.length - 0.5f);
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("StageSelect");
    }


    void RetryLevel()
    {
        EventSystem.current.enabled = false;
        StartCoroutine(ChangeSceneAfterFade(SceneManager.GetActiveScene().name));
    }

    void QuitLevel()
    {
        EventSystem.current.enabled = false;
        StartCoroutine(ChangeSceneAfterFade("StageSelect"));
    }


    IEnumerator ChangeSceneAfterFade(string sceneName)
    {
        //Return to title screen
        cannotAct = true;
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene(sceneName);
    }

}
