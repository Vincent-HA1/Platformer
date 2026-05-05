using UnityEngine;

//Stage select waypoint
public class StageWaypoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] string stageToLoad;
    [SerializeField] int numberOfBigCoins;

    [SerializeField] bool reachable = false;
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public string GetStage()
    {
        return stageToLoad;
    }

    public int GetNumberOfBigCoins()
    {
        return numberOfBigCoins;
    }
    public void SetStageCompleted()
    {
        reachable = true;
        anim.SetBool("Found", true);
    }

    public void SetStageReachable()
    {
        reachable = true;
    }

    public bool IsReachable()
    {
        return reachable;
    }
}
