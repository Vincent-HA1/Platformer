using UnityEngine;

public class PlatformEnemy : PatrolEnemy
{
    PlatformToFollow platformScript;
    Vector2 lastPos;
    protected override void Start()
    {
        base.Start();
        platformScript = GetComponent<PlatformToFollow>();
        lastPos = transform.position;
    }

    protected override void Patrol()
    {
        if (moving)
        {
            if(!CanMove())
            {
                //If run into something, change direction
                moveTimer = 0;
            }
            //if player is there, just stop moving entirely
            if (playerDetected)
            {
                moving = false;
            }
        }

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        //After movement, find the difference, and update the player's delta on it
        Vector2 difference = (Vector2)transform.position - lastPos;
        platformScript.SetPlatformDelta(difference);
        lastPos = transform.position;
    }


    protected override void GetHit()
    {
        //This enemy does not get hurt
    }
}
