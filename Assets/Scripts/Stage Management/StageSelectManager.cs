using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject stageWaypointsParent;
    [SerializeField] GameObject playerCharacter;
    [SerializeField] Animator sceneFadeAnimator;

    [Header("UI Elements")]
    [SerializeField] GameObject bigCoinIndicatorPrefab;
    [SerializeField] GameObject bigCoinsParent;
    [SerializeField] TMPro.TextMeshProUGUI stageNameText;

    [Header("Player Character Attributes")]
    [SerializeField] float moveSpeed;

    Animator playerAnimator;
    SpriteRenderer playerSpriteRenderer;
    InputHandler inputHandler;

    int currentWaypoint = 0;

    Vector2 movementInput;
    Vector2 moveDirection;
    bool moving;

    SaveData saveData;
    List<StageWaypoint> stageWaypoints;

    float exitTimer;
    float exitTime = 1;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        loadingScene = true;
        playerAnimator = playerCharacter.GetComponent<Animator>();
        playerSpriteRenderer = playerCharacter.GetComponent<SpriteRenderer>();
        inputHandler = GetComponent<InputHandler>();
        stageWaypoints = stageWaypointsParent.GetComponentsInChildren<StageWaypoint>().ToList();
        saveData = SaveSystem.Load();
        if (saveData != null)
        {
            //Foreach stage that has been saved (i.e. completed), set the relevant flag to show it
            foreach (StageWaypoint stageWaypoint in stageWaypoints)
            {
                StageSave stageSave = saveData.stagesSaved.Find(stage => stage.stageName == stageWaypoint.GetStage());
                if (stageSave != null)
                {
                    stageWaypoint.SetStageCompleted();
                }
            }
        }

        UpdateUI();
        StartCoroutine(WaitForSceneFade());
    }

    IEnumerator WaitForSceneFade()
    {
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        loadingScene = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (loadingScene) return;
        GetMovementInput();
        UpdateAnims();
        MoveToWaypoint();
        CheckToLoadStage();
        if (inputHandler.jumpPressed)
        {
            exitTimer = exitTime;
        }
        if(inputHandler.jumpHeld)
        {
            exitTimer -= Time.deltaTime;
            if (exitTimer <= 0 && !loadingScene)
            {
                StartCoroutine(QuitToTitle());
            }
        }
    }

    Vector2 lastDirection;
    //Don't allow holding the movement input.  
    void GetMovementInput()
    {
        if (!moving)
        {
            Vector2 newInput = new Vector2(inputHandler.movement.x, 0f);
            //If the vector has changed, assign it. This prevents holding a direction (so the player has to do presses to move)
            if(newInput != lastDirection)
            {
                print(newInput);
                print(movementInput);
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
            if(moveLerp < 1)
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
                if(nextWaypointIndex >= 0 && nextWaypointIndex < stageWaypoints.Count)
                {
                    moving = true;
                    moveLerp = 0;
                    moveDirection = movementInput;
                    destination = stageWaypoints[nextWaypointIndex].transform.position;
                    startPoint = stageWaypoints[currentWaypoint].transform.position;
                    currentWaypoint = nextWaypointIndex;
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
        for(int i = 0; i< currentStage.GetNumberOfBigCoins(); i++)
        {
            //instantiate all the big coins
            GameObject bigCoin = Instantiate(bigCoinIndicatorPrefab, bigCoinsParent.transform);
            if (stageSave != null)
            {
                print(stageSave.bigCoinsFound.Count);
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
        if (inputHandler.attackPressed && !moving)
        {
            loadingScene = true;
            StartCoroutine(LoadStage());
        }
    }

    IEnumerator LoadStage()
    {
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
