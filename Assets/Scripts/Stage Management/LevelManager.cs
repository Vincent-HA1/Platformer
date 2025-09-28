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
    [SerializeField] PauseMenu pauseMenu;
    [SerializeField] HUDManager hudManager;

    [Header("Level References")]
    [SerializeField] GameObject checkpointsParent;
    [SerializeField] GameObject collectiblesParent;
    [SerializeField] PlayerMovement player;
    [SerializeField] MovingSpikes movingSpikes;

    [Header("Songs")]
    [SerializeField] AudioClip stageSong;
    [SerializeField] AudioClip levelEndSong;
    [SerializeField] float normalVolume = 1;
    [SerializeField] float pausedVolume = 0.5f;

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
        audioSource = GetComponent<AudioSource>();
        PlayAudioClip(stageSong, true);
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
        if (movingSpikes) movingSpikes.SetPosition();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
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
        if (movingSpikes) movingSpikes.SetPosition();
        sceneFadeAnimator.SetTrigger("FadeIn");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        cannotAct = false;
        respawning = false;
    }

    void EndLevel(Checkpoint endFlag)
    {
        StartCoroutine(GoNextLevelAfterFade());
    }


    IEnumerator GoNextLevelAfterFade()
    {
        //Fade out, save, then load stage select
        player.ReachedEndOfLevel();
        yield return new WaitUntil(() => player.onGround);
        PlayAudioClip(levelEndSong, false);
        cannotAct = true;
        Time.timeScale = 0;
        SaveSystem.Save(currentStageSave);
        yield return new WaitUntil(() => audioSource.time >= audioSource.clip.length - 0.5f);
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("StageSelect");
    }

    void QuitLevel()
    {
        EventSystem.current.enabled = false;
        StartCoroutine(QuitAfterFade());
    }

    IEnumerator QuitAfterFade()
    {
        //Return to title screen
        cannotAct = true;
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("StageSelect");
    }
}
