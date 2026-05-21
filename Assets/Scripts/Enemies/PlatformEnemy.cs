using UnityEngine;

public class PlatformEnemy : PatrolEnemy
{
    PlatformToFollow platformScript;
    Vector2 lastPos;

    //Platform Handling
    PlatformToFollow platformToFollow;
    Vector2 platformDelta;

    public void SetPlatformDelta(Vector2 movement)
    {
        platformDelta = movement;
    }

    protected override void Start()
    {
        base.Start();
        platformScript = GetComponent<PlatformToFollow>();
        lastPos = transform.position;
    }

    protected override void Update()
    {
        base.Update();
        if(platformToFollow && platformDelta != new Vector2())
        {
            waitTimer = 1;
        }
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
            if (playerDetected || platformToFollow && platformDelta != new Vector2())
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

        if (platformToFollow && platformDelta != new Vector2())
        {
            print(platformDelta);
            //Following another platform
            rigid.MovePosition(rigid.position + platformDelta);
        }

    }


    protected override void GetHit()
    {
        //This enemy does not get hurt
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("MovingPlatform") && platformToFollow == null && onGround)
        {
            print("Touch moving platform");
            //Set the moving platform
            platformToFollow = collision.GetComponentInParent<PlatformToFollow>();
            platformToFollow.SetEnemy(this);
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("MovingPlatform") && platformToFollow)
        {
            ExitPlatform();
        }
    }

    void ExitPlatform()
    {
        if (platformToFollow)
        {
            platformToFollow.Disengage();
            platformToFollow = null;
        }
        platformDelta = Vector2.zero;
    }

}
