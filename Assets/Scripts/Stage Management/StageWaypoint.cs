using UnityEngine;

//Stage select waypoint
public class StageWaypoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] string stageToLoad;
    [SerializeField] int numberOfBigCoins;

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
        anim.SetBool("Found", true);
    }
}
