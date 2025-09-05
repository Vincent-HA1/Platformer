using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JumpingEnemy : BaseEnemy
{
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    [Header("References")]
    [SerializeField] Transform groundPos;


    [Header("Ground Enemy Attributes")]
    [SerializeField] float moveSpeed = 6;
    [SerializeField] float groundCheckXOffset = 0.6f;
    [SerializeField] float wallCheckDistance = 0.8f;
    [SerializeField] float playerCheckDistance = 4;
    [SerializeField] float minDistanceToPlayer = 0.3f;

    [Header("Jump Enemy Attributes")]
    [SerializeField] float minJumpTime = 1;
    [SerializeField] float maxJumpTime = 2;
    [SerializeField] float jumpForce = 6;
    [SerializeField] float gravityForce = -20;
    [SerializeField] float terminalNegativeVelocity = -20;
    [SerializeField] bool canJump = false;

    bool onGround = false;
    bool jumping = false;
    bool wallThere = false;
    bool hitOtherEnemy = false;
    bool setInitialJumpTimer = false;
    float jumpTimer;
    float verticalVelocity;
    protected override void Update()
    {
        base.Update();
        float xOffset = jumping ? 0 : groundCheckXOffset; //if jumping, dont use the offset
        onGround = Physics2D.OverlapCircle(groundPos.position + new Vector3(xOffset * moveDirection.x, 0), 0.2f, groundLayer);
        ManageJumpTimer();
    }

    protected override void UpdateAnims()
    {
        base.UpdateAnims();
        anim.SetBool("Jumping", jumping && verticalVelocity > 0);
        anim.SetBool("Falling", jumping && verticalVelocity <= 0);
    }

    protected override void DetectPlayer()
    {
        base.DetectPlayer();
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, moveDirection, wallCheckDistance, groundLayer);
        RaycastHit2D playerHit = Physics2D.Raycast(transform.position, moveDirection, playerCheckDistance, playerLayer);
        wallThere = wallHit.collider != null;
        if (!player && playerHit.collider != null)
        {
            player = playerHit.collider.transform;
        }
        if (playerHit.collider != null || !playerTooFar && playerDetected)
        {
            //Check if wall is not in between the player and the enemy by doing the same raycast but for the ground
            RaycastHit2D wallPlayerCheck = Physics2D.Raycast(transform.position, moveDirection, playerCheckDistance, groundLayer);
            float playerDistance = playerHit.collider == null ? 0 : Vector2.Distance(playerHit.point, transform.position);
            float wallDistance = Vector2.Distance(wallPlayerCheck.point, transform.position);
            if (wallPlayerCheck.collider == null || wallDistance > playerDistance)
            {
                if (!playerDetected)
                {
                    //So player in front of wall, so can detect now
                    playerDetected = true;
                    moving = true;
                }

            }
            //Check if wall now in between plaeyr and the enemy. If so, have to cancel the chase, regardless of distance
            else if(wallPlayerCheck.collider != null && wallDistance < playerDistance)
            {
                //Stop moving for now
                if (playerDetected)
                {
                    playerDetected = false;
                    moveTimer = 0;
                }

            }

        }
        else
        {
            //Stop moving for now
            if (playerDetected)
            {
                playerDetected = false;
                moveTimer = 0;
            }
        }
        //Checking for other enemies
        List<RaycastHit2D> enemiesHit = Physics2D.RaycastAll(transform.position, moveDirection, 0.8f, enemyLayer).ToList();
        hitOtherEnemy = false;
        foreach (RaycastHit2D h in enemiesHit)
        {
            if (h.collider != null && h.collider.gameObject != gameObject)
            {
                // First valid non-self hit
                hitOtherEnemy = true;
            }
        }

    }
    void ManageJumpTimer()
    {
        if(!canJump || hurt) return; //only some enemies can jump
        if (onGround && verticalVelocity < 0)
        {
            print("Cancel jump");
            jumping = false;
        }
        if (playerDetected)
        {
            //add jump timer stuff
            if(jumpTimer <= 0 && !jumping)
            {
                //Set timer. If this is the start, then don't jump immediately
                jumpTimer = UnityEngine.Random.Range(minJumpTime, maxJumpTime);
                if (!setInitialJumpTimer)
                {
                    setInitialJumpTimer = true;
                }
                else
                {
                    //Jump
                    PerformJump();
                }

            }
            if (!jumping)
            {
                jumpTimer -= Time.deltaTime;
            }
        }
        else
        {
            //Allow the jump timer to be set next time (to not jump immediately)
            setInitialJumpTimer = false;
        }
    }

    void PerformJump()
    {
        print("perform jump");
        verticalVelocity = jumpForce;
        jumping = true;
    }


    //Called when changing direction during patrolling.
    protected override void ChangeDirection()
    {
        if (!CanMove())
        {
            moveDirection = -moveDirection;
        }
        else
        {
            int randomDir = UnityEngine.Random.Range(0, 2);
            moveDirection = randomDir == 0 ? moveDirection : -moveDirection;
        }
        base.ChangeDirection();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        ApplyMovement();
    }

    protected override void Patrol()
    {
        if (!playerDetected)
        {
            if (moving && !CanMove())
            {
                moveTimer = 0;
            }
        }
        else
        {
            MoveTowardsPlayer();

        }

    }

    void MoveTowardsPlayer()
    {
        if (jumping)
        {
            if (!CanMove())
            {
                print("flip");
                moveDirection = -moveDirection;
            }
            return;
        }
        //move towards the player if they are detected. 
        Vector2 difference = (player.position - transform.position);
        moveDirection = new Vector2(Mathf.Sign(difference.x), 0);
        //Move towards player while it is allowed (i.e. player is far away enough)
        if (CanMove() && Mathf.Abs(difference.x) > minDistanceToPlayer)
        {
            moving = true;
        }
        else
        {
            //If off ground, or enemy too close
            moving = false;
        }
    }

    bool CanMove()
    {
        //Returns the conditions for not being able to move
        return (onGround||jumping) && !wallThere && !hitOtherEnemy;

    }
    void ApplyMovement()
    {
        if (!moving && !jumping) return;

        float dx = (moveDirection * Time.fixedDeltaTime * moveSpeed).x;

        //Don't move downwards if not jumping
        float dy = !jumping ? 0 : verticalVelocity * Time.fixedDeltaTime + 0.5f * gravityForce * Time.fixedDeltaTime * Time.fixedDeltaTime;

        // Update vertical velocity (SUVAT), assuming initial velocity is 0. If on the ground, velocity is automatically 0
        verticalVelocity = !jumping ? 0: Mathf.Max(terminalNegativeVelocity, verticalVelocity + gravityForce * Time.fixedDeltaTime);

        Vector2 finalMovement = new Vector2(dx, dy);

        // Apply movement to Rigidbody2D
        rigid.MovePosition(rigid.position + finalMovement);
    }


    protected override void GetHit()
    {
        base.GetHit();
        verticalVelocity = 0; //get knocked down
        moveDirection = Vector2.zero;
    }

    
}
