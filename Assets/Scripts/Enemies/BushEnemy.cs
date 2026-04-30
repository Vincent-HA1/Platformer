using System.Collections;
using UnityEngine;


public class BushEnemy : JumpingEnemy
{
    [Header("Bush Unique Attributes")]
    [SerializeField] float maxDistanceToPlayer = 5f;

    [Header("Bush References")]
    [SerializeField] BoxCollider2D detectionCollider;

    bool comingOut = false;
    bool hiding = false;
    bool returningToStart = false;
    BoxCollider2D boxCollider;
    Vector3 startPosition;
    

    protected override void Start()
    {
        base.Start();
        boxCollider = GetComponent<BoxCollider2D>();
        startPosition = transform.position;
        Hide();
    }

    void Hide()
    {
        hiding = true;
        moving = false;
        boxCollider.enabled = false; //Disable enemy collider while hiding
    }

    protected override void Update()
    {
        base.Update();
        print(CanMove());
        detectionCollider.enabled = hiding; //only have detection collider open when hiding
    }

    protected override void ManageMoveTimers()
    {
        //Do not have any move timers
    }

    protected override void UpdateAnims()
    {
        base.UpdateAnims();
        anim.SetBool("Hiding", hiding);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        //Chase after player
        if (playerDetected)
        {
            if (!hurt) MoveTowardsPlayer();
        }
        else
        {
            //If player not there, return to start and hide
            if (!hiding)
            {
                ReturnToStart();
            }
            if (!hiding && !returningToStart && onGround)
            {
                Hide();
            }
        }
        ApplyMovement();
    }

    void ReturnToStart() 
    {
        //If returned to start, stop moving
        if (AtStartPosition()) returningToStart = false;
        //If need to return to start
        if (!returningToStart && !AtStartPosition())
        {
            //Set the move direction towards the start point and begin moving
            Vector2 difference = (startPosition - transform.position);
            moveDirection = new Vector2(Mathf.Sign(difference.x), 0);
            returningToStart = true;
            moving = true;
        }

    }

    bool AtStartPosition()
    {
        return Vector2.Distance(transform.position, startPosition) <= 0.2f;
    }

    protected override void Patrol()
    {
        //Bush Enemy does not patrol
        //If run into something, stop moving
        if (moving && !CanMove())
        {
            moveTimer = 0;
            moving = false;
        }
    }

    // Function is identical to Jumping Enemy except there is no actual detection, only undetection code.
    // So need to set it to false whenever the parent of this function sets it to true
    protected override void DetectPlayer()
    {
        bool wasPlayerDetected = playerDetected; //was player detected already true? 
        base.DetectPlayer();
        // If playerDetected is true now, and it wasn't before, then this function has set it to true. So set it back
        if(playerDetected && !wasPlayerDetected)
        {
            playerDetected = false;
        }

    }
    protected override void ApplyMovement()
    {
        if (hiding) return;
        base.ApplyMovement();
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        //Detect player when hiding as a bush
        if (collision.CompareTag("Player") && !playerDetected && !comingOut && hiding)
        {
            //player detected
            StartCoroutine(ComeOutOfHiding());
        }
    }

    IEnumerator ComeOutOfHiding()
    {
        print("come out");
        comingOut = true;
        yield return new WaitForSeconds(0.5f);
        playerDetected = true;
        boxCollider.enabled = true;
        comingOut = false;
        hiding = false;
        moving = true;
    }
}