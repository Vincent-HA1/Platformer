using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class JumpingEnemy : BaseEnemy
{
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    [Header("References")]
    [SerializeField] protected Transform groundPos;

    [Header("Ground Enemy Attributes")]
    [SerializeField] protected float moveSpeed = 2;
    [SerializeField] protected float groundCheckXOffset = 0.6f;
    [SerializeField] protected float wallCheckDistance = 0.8f;
    [SerializeField] protected float playerCheckDistance = 4;
    [SerializeField] protected float minDistanceToPlayer = 0.3f;

    [Header("Jump Enemy Attributes")]
    [SerializeField] protected float minJumpTime = 1;
    [SerializeField] protected float maxJumpTime = 2;
    [SerializeField] protected float jumpLagTime = 0.2f;
    [SerializeField] protected float jumpForce = 6;
    [SerializeField] protected float gravityForce = -20;
    [SerializeField] protected float terminalNegativeVelocity = -20;
    [SerializeField] protected bool canJump = false;

    protected bool onGround = false;
    protected bool jumping = false;
    protected bool wallThere = false;
    protected bool hitOtherEnemy = false;
    protected bool setInitialJumpTimer = false;
    protected float jumpTimer;
    protected float jumpLagTimer;
    protected float verticalVelocity;
    protected Vector2 moveDirBeforeGettingHit;

    protected override void Update()
    {
        base.Update();
        float xOffset = jumping ? 0 : groundCheckXOffset; //if jumping, dont use the offset
        Collider2D[] groundColliders = Physics2D.OverlapCircleAll(groundPos.position + new Vector3(xOffset * moveDirection.x, 0), 0.18f, groundLayer); //offset the check so dont go over the ledge
        int groundCount = groundColliders.Count(x => x.gameObject != this.gameObject); //Ignore all ground colliders that are on this gameobject
        onGround = groundCount > 0; 
        ManageJumpTimer();
        HandleJumpInteraction();
    }

    protected override void UpdateAnims()
    {
        base.UpdateAnims();
        anim.SetBool("Jumping", jumping && verticalVelocity > 0);
        anim.SetBool("Falling", jumping && verticalVelocity <= 0);
    }

    protected override void ResumeFromGettingHit()
    {
        base.ResumeFromGettingHit();
        //After getting hit, make sure to return back to normal functionality
        if (moveDirection == Vector2.zero)
        {
            //So continue moving like before
            moveDirection = moveDirBeforeGettingHit;
        }
    }

    protected override void DetectPlayer()
    {
        base.DetectPlayer();
        RaycastHit2D[] wallHitResults = new RaycastHit2D[1];
        int wallHitCount = boxCollider.Raycast(moveDirection, wallHitResults, wallCheckDistance, groundLayer);//RaycastHit2D wallHit = Physics2D.Raycast(transform.position, moveDirection, wallCheckDistance, groundLayer);
        RaycastHit2D playerHit = Physics2D.Raycast(transform.position, moveDirection, playerCheckDistance, playerLayer);
        wallThere = wallHitCount > 0;//wallHit.collider != null;
        if (!player && playerHit.collider != null)
        {
            player = playerHit.collider.transform;
        }
        if ((playerHit.collider != null || !playerTooFar && playerDetected) && CanMove()) //Can only detect player if can move
        {
            //Check if wall is not in between the player and the enemy by doing the same raycast for the player but for the ground now
            //RaycastHit2D wallPlayerCheck = Physics2D.Raycast(transform.position, moveDirection, playerCheckDistance, groundLayer);
            wallHitCount = boxCollider.Raycast(moveDirection, wallHitResults, playerCheckDistance, groundLayer);
            float playerDistance = playerHit.collider == null ? 0 : Vector2.Distance(playerHit.point, transform.position);
            float wallDistance = Vector2.Distance(wallHitResults[0].point, transform.position);//wallPlayerCheck.point, transform.position);
            if (wallHitCount <= 0 || wallDistance > playerDistance)//wallPlayerCheck.collider == null || wallDistance > playerDistance)
            {
                if (!playerDetected)
                {
                    //So player in front of wall, so can detect now
                    playerDetected = true;
                    moving = true;
                }
            }
            //Check if wall now in between player and the enemy. If so, have to cancel the chase, regardless of distance
            else if (wallHitCount > 0 && wallDistance <= playerDistance)//wallPlayerCheck.collider != null && wallDistance <= playerDistance)
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
            //Stop moving for now, as player too far or cannot move over there (for real, not just in jump lag)
            if (playerDetected && ((!CanMove() && jumpLagTimer <=0) || playerTooFar))
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

    protected virtual void ManageJumpTimer()
    {
        if (!canJump || hurt) return; //only some enemies can jump, don't jump when hurt        
        if (playerDetected)
        {
            //If not jumping, then set the jump timer
            if (jumpTimer <= 0 && !jumping && jumpLagTimer <= 0)
            {
                //Set timer. If this is the start, then don't jump immediately
                jumpTimer = Random.Range(minJumpTime, maxJumpTime);
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

    // Handle what happens once the enemy has actually jumped
    void HandleJumpInteraction()
    {
        if (!canJump) return; //only some enemies can jump
        // If finished jumping
        if (onGround && verticalVelocity < 0 && jumping)
        {
            //Stop jump and set up lag
            jumping = false;
            jumpLagTimer = jumpLagTime;
            moving = false;
        }

        if (jumpLagTimer >= 0) jumpLagTimer -= Time.deltaTime;

        //Make sure we don't jump through walls
        if (jumping)
        {
            //If jumping, then once reaching a wall, flip immediately (i.e. bounce off it)
            if (wallThere && !hurt)
            {
                print("BOUNCE");
                moveDirection = -moveDirection;
            }
            return;
        }
    }

    protected virtual void PerformJump()
    {
        verticalVelocity = jumpForce;
        jumping = true;
        moveDirBeforeGettingHit = moveDirection;
    }

    protected virtual void MoveTowardsPlayer()
    {
        if (jumping) return;
        Vector2 difference = Vector2.zero;
        if (jumpLagTimer <= 0) //Can't spin around when in jump lag
        {
            difference = (player.position - transform.position);
            moveDirection = new Vector2(Mathf.Sign(difference.x), 0);
        }
        //Move towards player while it is allowed (i.e. player is far away enough)
        if (CanMove() && Mathf.Abs(difference.x) > minDistanceToPlayer)
        {
            moving = true;
        }
        else if(jumpLagTimer <= 0) //So if not in jump lag, and still cant move, then have to stop moving
        {
            //If off ground, or enemy too close
            moving = false;
        }
    }

    protected virtual bool CanMove()
    {
        //Returns the conditions for not being able to move
        return (onGround || jumping) && !wallThere && !hitOtherEnemy && jumpLagTimer <= 0;
    }

    protected virtual void ApplyMovement()
    {
        if ((!moving && !jumping) || jumpLagTimer > 0) return; //Don't do anything if standing still (or in jump lag)

        float dx = (moveDirection * Time.fixedDeltaTime * moveSpeed).x;

        //Don't move downwards if not jumping
        float dy = !jumping ? 0 : verticalVelocity * Time.fixedDeltaTime + 0.5f * gravityForce * Time.fixedDeltaTime * Time.fixedDeltaTime;

        // Update vertical velocity (SUVAT), assuming initial velocity is 0. If on the ground, velocity is automatically 0
        verticalVelocity = !jumping ? 0 : Mathf.Max(terminalNegativeVelocity, verticalVelocity + gravityForce * Time.fixedDeltaTime);

        Vector2 finalMovement = new Vector2(dx, dy); //new Vector2(0, dy);//new Vector2(dx, dy);

        // Apply movement to Rigidbody2D
        rigid.MovePosition(rigid.position + finalMovement);
    }

    protected override void GetHit()
    {
        base.GetHit();
        verticalVelocity = 0; //get knocked down
        moveDirBeforeGettingHit = moveDirection;
        moveDirection = Vector2.zero;
    }
}

