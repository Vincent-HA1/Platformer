using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] GameObject stageWaypointsParent;
    [SerializeField] GameObject playerCharacter;
    [SerializeField] Animator sceneFadeAnimator;
    [SerializeField] StageSelectCamera cameraScript;
    [SerializeField] List<GameObject> Parallaxes;

    [Header("UI Elements")]
    [SerializeField] GameObject bigCoinIndicatorPrefab;
    [SerializeField] GameObject bigCoinsParent;
    [SerializeField] TMPro.TextMeshProUGUI stageNameText;
    [SerializeField] Image holdOutline;

    [Header("Player Character Attributes")]
    [SerializeField] float moveSpeed;

    Animator playerAnimator;
    SpriteRenderer playerSpriteRenderer;
    InputHandler inputHandler;

    int currentWaypoint = 0;
    int currentWorldIndex = 0;

    Vector2 movementInput;
    Vector2 moveDirection;
    bool moving;
    bool movingToNextWorld;

    SaveData saveData;
    List<StageWaypoint> stageWaypoints;

    float exitTimer;
    float exitTime = 1;

    // Start is called before the first frame update
    void Awake()
    {
        //SaveSystem.DeleteSave();
        Time.timeScale = 1;
        loadingScene = true;
        playerAnimator = playerCharacter.GetComponent<Animator>();
        playerSpriteRenderer = playerCharacter.GetComponent<SpriteRenderer>();
        inputHandler = GetComponent<InputHandler>();
        stageWaypoints = stageWaypointsParent.GetComponentsInChildren<StageWaypoint>().ToList();
        LoadStageSaves();
        ChangeParallax(currentWorldIndex); 
        UpdateUI();
        StartCoroutine(WaitForSceneFade());
    }

    void LoadStageSaves()
    {
        saveData = SaveSystem.Load(SaveSystem.currentSaveSlotUsed);
        //Find the furthest stage completed to see how many stages to enable
        StageWaypoint furthestStageCompleted = null;
        if (saveData != null)
        {
            //Foreach stage that has been saved (i.e. completed), set the relevant flag to show it
            foreach (StageWaypoint stageWaypoint in stageWaypoints)
            {
                StageSave stageSave = saveData.stagesSaved.Find(stage => stage.stageName == stageWaypoint.GetStage());
                if (stageSave != null)
                {
                    stageWaypoint.SetStageCompleted();
                    furthestStageCompleted = stageWaypoint; //Update the furthest stage completed
                }
            }
            //Set player and camera position depending on what stage they last entered
            currentWaypoint = saveData.lastStageEntered;
            currentWorldIndex = saveData.lastWorldEntered;
            cameraScript.SetCameraPosition(currentWorldIndex);
            playerCharacter.transform.position = stageWaypoints[currentWaypoint].transform.position;
        }
        if (furthestStageCompleted != null)
        {
            //allow the next stage from this stage to be reachable
            int index = stageWaypoints.FindIndex(x => x == furthestStageCompleted);
            if (index < stageWaypoints.Count - 1) stageWaypoints[index + 1].SetStageReachable();
        }

    }

    void ChangeParallax(int newIndex)
    {
        Parallaxes[currentWorldIndex].SetActive(false);
        Parallaxes[newIndex].SetActive(true);
        currentWorldIndex = newIndex;
    }

    IEnumerator WaitForSceneFade()
    {
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        loadingScene = false;
        movingToNextWorld = false;
        exitTimer = exitTime;
    }


    // Update is called once per frame
    void Update()
    {
        if (loadingScene) return;
        GetMovementInput();
        UpdateAnims();
        MoveToWaypoint();
        CheckToLoadStage();
        if (inputHandler.cancelPressed && !movingToNextWorld)
        {
            exitTimer = exitTime;
        }
        if (inputHandler.cancelHeld && !movingToNextWorld)
        {
            exitTimer -= Time.deltaTime;
            holdOutline.fillAmount = (exitTime - exitTimer) / exitTime;
            if (exitTimer <= 0 && !loadingScene)
            {
                StartCoroutine(QuitToTitle());
            }
        }
        else
        {
            holdOutline.fillAmount = 0;
        }
    }

    Vector2 lastDirection;
    //Don't allow holding the movement input.  
    void GetMovementInput()
    {
        if (movingToNextWorld || inputHandler.cancelHeld) return; //no inputs when moving to next world
        if (!moving)
        {
            Vector2 newInput = new Vector2(inputHandler.movement.x, 0f);
            //If the vector has changed, assign it. This prevents holding a direction (so the player has to do presses to move)
            if (newInput != lastDirection)
            {
                movementInput = newInput;
                lastDirection = movementInput;
            }
        }
    }

    void UpdateAnims()
    {
        playerAnimator.SetFloat("Speed", moving ? 1 : 0);
        playerSpriteRenderer.flipX = moveDirection.x == -1;
    }

    float moveLerp = 0;
    Vector3 destination;
    Vector3 startPoint;
    void MoveToWaypoint()
    {
        if (moving)
        {
            if (moveLerp < 1)
            {
                //move with the lerp
                playerCharacter.transform.position = Vector3.Lerp(startPoint, destination, moveLerp);
                moveLerp += moveSpeed * Time.deltaTime;
            }
            else
            {
                //Stop moving as at a stage waypoint. Update the UI to show this
                moving = false;
                movementInput = Vector2.zero;
                UpdateUI();
            }

        }
        else
        {
            if (movementInput != Vector2.zero)
            {

                //Calculate next destination (i.e. next waypoint to move to)
                int nextWaypointIndex = currentWaypoint + (int)movementInput.x;
                if (nextWaypointIndex >= 0 && nextWaypointIndex < stageWaypoints.Count)
                {
                    //Check if the next waypoint is reachable
                    StageWaypoint nextWaypoint = stageWaypoints[nextWaypointIndex];
                    if (nextWaypoint.IsReachable())
                    {
                        StageWaypoint lastWaypoint = stageWaypoints[currentWaypoint];
                        moving = true;
                        moveLerp = 0;
                        moveDirection = movementInput;
                        destination = nextWaypoint.transform.position;
                        startPoint = lastWaypoint.transform.position;
                        currentWaypoint = nextWaypointIndex;
                        //Check if moving worlds
                        if(nextWaypoint.transform.parent != lastWaypoint.transform.parent)
                        {
                            float direction = Mathf.Sign(nextWaypoint.transform.position.x - lastWaypoint.transform.position.x);
                            StartCoroutine(MoveToNextWorld((int)direction));
                        }
                    }

                }
            }
        }

    }

    void UpdateUI()
    {
        StageWaypoint currentStage = stageWaypoints[currentWaypoint];
        StageSave stageSave = saveData != null ? saveData.stagesSaved.Find(stage => stage.stageName == currentStage.GetStage()) : null;
        stageNameText.text = currentStage.GetStage();
        List<BigCoinIndicator> list = bigCoinsParent.GetComponentsInChildren<BigCoinIndicator>().ToList();
        //Redraw all the big coin indicators
        foreach (BigCoinIndicator indicator in list)
        {
            Destroy(indicator.gameObject);
        }
        for (int i = 0; i < currentStage.GetNumberOfBigCoins(); i++)
        {
            //instantiate all the big coins
            GameObject bigCoin = Instantiate(bigCoinIndicatorPrefab, bigCoinsParent.transform);
            if (stageSave != null)
            {
                //if those coins were found, show it
                BigCoinIndicator bigCoinIndicator = bigCoin.GetComponent<BigCoinIndicator>();
                if (stageSave.bigCoinsFound[i] == 1)
                {
                    bigCoinIndicator.SetFound();
                }
            }
        }
    }

    bool loadingScene = false;
    void CheckToLoadStage()
    {
        if (inputHandler.confirmPressed && !moving)
        {
            loadingScene = true;
            StartCoroutine(LoadStage());
        }
    }

    IEnumerator MoveToNextWorld(int direction)
    {
        movingToNextWorld = true;
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        cameraScript.ShiftCamera(direction); //Slide the camera over
        ChangeParallax(currentWorldIndex + direction);
        yield return new WaitForSeconds(1f);
        sceneFadeAnimator.SetTrigger("FadeIn");
        StartCoroutine(WaitForSceneFade());
    }

    IEnumerator LoadStage()
    {
        SaveSystem.SaveLastStageEntered(currentWaypoint, currentWorldIndex);
        playerAnimator.SetBool("Victory", true);
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene(stageWaypoints[currentWaypoint].GetStage()); //Load the stage selected
    }

    IEnumerator QuitToTitle()
    {
        loadingScene = true;
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("TitleScreen"); //Load the stage selected
    }
}
