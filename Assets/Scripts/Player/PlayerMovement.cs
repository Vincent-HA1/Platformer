using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Action<float> Healed;
    public Action<float> Hit;
    public Action Death;
    public Action Jump;
    public Action OpenParachute;
    public Action Bounce;
    public Action PunchAction;
    public Action KickAction;
    public LayerMask groundLayer;
    public LayerMask waterLayer;
    public LayerMask iceLayer;

    public float MaxHealth
    {
        get
        {
            return maxHealth;
        }
    }
    public float Health
    {
        get
        {
            return health;
        }
    }

    [SerializeField] bool ignoreOnGround = false;
    [Header("References")]
    [SerializeField] Transform leftFootPoint, rightFootPoint;
    [SerializeField] Transform headPosition;
    [SerializeField] Vector2 footSize = new Vector2(0.25f, 0.12f);
    [SerializeField] BoxCollider2D punchHitbox;
    [SerializeField] BoxCollider2D kickHitbox;


    [Header("Player Attributes")]
    [SerializeField] float maxHealth = 3;
    [SerializeField] float hitInvincibilityDuration = 2;

    [Header("Player Movement Attributes")]
    [SerializeField] float groundMoveSpeed = 6;
    [SerializeField] float iceTurnAcceleration = 5;
    [SerializeField] float maxAirAccelerationChangeRate = 5;
    [SerializeField] float jumpForce = 8.5f;
    [SerializeField] float extraJumpForce = 0.38f;
    [SerializeField] float gravityForce = -35;
    [SerializeField] float terminalNegativeVelocity = -45;
    [SerializeField] float platformBonkVelocityHit = 0.05f;
    [SerializeField] float maxVerticalVelocity = 15;
    [SerializeField] float jumpBufferTime = 0.15f;
    [SerializeField] float coyoteTime = 0.08f;
    [SerializeField] float glideNegativeVelocity = -20;
    [SerializeField] AnimationCurve airAccelCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] bool useAccel = true; // toggle for air acceleration

    [Header("Player Attack Attributes")]
    [SerializeField] float attackBufferWindow = 0.2f;
    [SerializeField] float attackInputBufferWindow = 0.15f;

    [Header("Player Kick Attributes")]
    [SerializeField] float kickLength = 0.7f;
    [SerializeField] float kickInitialSpeed = 9;
    [SerializeField] float kickSlowdownRate = -5;
    [SerializeField] AnimationCurve kickAccelCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] float groundKickLag = 1f;
    [SerializeField] float swimKickLag = 2;

    [Header("Player Swim Attributes")]
    [SerializeField] float swimSpeed = 7;
    [SerializeField] float swimBobForce = 9;
    [SerializeField] float waterGravityForce = -10;
    [SerializeField] float terminalWaterNegativeVelocity = -20;


    [Header("Player Hurt Attributes")]
    [SerializeField] float hurtTime = 0.5f;

    [Header("Player Spring Attributes")]
    [SerializeField] float springXModifier = 0.05f;
    [Tooltip("Percentage falloff per second")]
    [SerializeField] float additiveForceFalloff = 0.3f;


    // Player Components
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    InputHandler inputHandler;
    BoxCollider2D boxCollider;

    //Platform Handling
    PlatformToFollow platformToFollow;

    // Input & movement state
    Vector2 movementInput;
    float xMovement;
    float currentHorizontalDir;
    Vector2 glideCheckSize;
    Vector2 platformDelta;

    // Velocity state
    float horizontalVelocity = 0f;
    float verticalVelocity = 0f;

    // Jump buffers
    float jumpBuffer = 0f;
    float coyoteBuffer = 0f;

    // Attack buffers
    float attackBuffer = 0;
    float attackInputBuffer = 0;

    // Kicking buffers
    float kickTimer = 0;

    //Hurt Timers
    float hurtTimer = 0;
    float invincibilityTimer = 0;

    Vector2 additiveForce;
    float additiveForcePercentage;

    // State flags
    public bool onGround { get; private set; } = false;
    bool canActivateCoyote = false;
    bool isJumping = false;
    bool storingJumpInput = false;
    bool stoppedHoldingJump = false;

    //Gliding states
    bool gliding = false;
    bool canGlide = false;

    // Attacking states
    bool attacking = false;
    bool attackFinished = false;

    // Kicking states
    bool kicking = false;
    bool canKick = false;
    bool inKickLag = false;


    // hurt states
    bool hurt = false;

    //Swimming
    bool inWater = false;
    bool waterAbove = false;

    //Ice
    bool onIce = false;
    float directionOfIceAcceleration;

    bool blockHorizontalMovement = false;
    float health;

    public void ReachedEndOfLevel()
    {
        StartCoroutine(EndAnimation());
    }

    IEnumerator EndAnimation()
    {
        //Prevent player from moving and play the victory animation after they land
        blockHorizontalMovement = true;
        verticalVelocity = 0;
        yield return new WaitUntil(() => onGround);
        anim.SetBool("Victory", true);
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;

    }


    void Awake()
    {
        // Cache component references
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inputHandler = GetComponent<InputHandler>();
        boxCollider = GetComponent<BoxCollider2D>();
        glideCheckSize = footSize + new Vector2(0, 1f);
        spriteRenderer.color = Color.white;
        health = maxHealth;
        anim.SetFloat("Horizontal", 1); //start by facing right
    }

    void Update()
    {
        CheckForSurfaces();
        // Update animator parameters
        UpdateAnimations();
        if (LevelManager.cannotAct) return;
        CheckHurtTimer();
        ManageHitboxDirection();
        CheckInvincibilityTimer();
        InvincibilityFlash();
        if (hurt) return;
        // Ground check and timers
        UpdateCoyoteBuffer();
        UpdateJumpBuffer();
        CheckAttackBuffer();
        CheckKickTimer();
        // Process movement input and potentially trigger jump
        ProcessMovementInput();
    }


    void LateUpdate()
    {
        if (LevelManager.cannotAct) return;
        // Handle horizontal acceleration in air or direct ground movement
        UpdateHorizontalMovement();
    }

    void FixedUpdate()
    {
        if (LevelManager.cannotAct) return;
        // Apply physics-based movement
        ApplyMovement();
    }

    // ----------------------------------------
    // Ground & Coyote
    // ----------------------------------------

    void CheckForSurfaces()
    {
        //Vector3 offset = platformToFollow == null ? Vector2.zero : new Vector2(0, -0.1f);
        //bool left = Physics2D.OverlapBox(leftFootPoint.position + offset, footSize, 0, groundLayer);
        //bool right = Physics2D.OverlapBox(rightFootPoint.position + offset, footSize, 0, groundLayer);
        bool leftGlideCheck = Physics2D.OverlapBox((Vector2)leftFootPoint.position - new Vector2(0, 0.3f), glideCheckSize, 0, groundLayer);
        bool rightGlideCheck = Physics2D.OverlapBox((Vector2)rightFootPoint.position - new Vector2(0, 0.3f), glideCheckSize, 0, groundLayer);
        onGround = CheckForSpecificGroundLayer(groundLayer, footSize);
        //If on ice, make the check larger so there is no sudden switch to on ground. If not, keep it as normal onGround check
        onIce = CheckForSpecificGroundLayer(iceLayer, onIce? footSize + new Vector2(0.3f, 0) : footSize);
        //test
        //ignoreOnGround = platformToFollow != null;
        //onGround = ignoreOnGround ? true : onGround;
        canGlide = !(leftGlideCheck || rightGlideCheck);
        if (onGround)
        {
            canActivateCoyote = true;
            stoppedHoldingJump = false;
            gliding = false;
            if (verticalVelocity <= 0)
            {
                additiveForce = Vector2.zero;
            }
        }

        //Check for water surface
        RaycastHit2D hit = Physics2D.Raycast(headPosition.position, Vector2.up, 0.1f, waterLayer);
        waterAbove = hit.collider != null;
        if (hit.collider == null && verticalVelocity > 0 && inWater) //so if the raycast shows we are at the top of the water
        {
            //get out of the water
            GetOutOfWater();
        }
    }

    bool CheckForSpecificGroundLayer(LayerMask layerToCheck, Vector2 boxSize)
    {
        Vector3 offset = platformToFollow == null ? Vector2.zero : new Vector2(0, -0.1f);
        bool left = Physics2D.OverlapBox(leftFootPoint.position + offset, boxSize, 0, layerToCheck);
        bool right = Physics2D.OverlapBox(rightFootPoint.position + offset, boxSize, 0, layerToCheck);
        return left || right;
    }

    void UpdateCoyoteBuffer()
    {
        // Start coyote time when leaving ground
        if (!onGround && !isJumping && canActivateCoyote)
        {
            coyoteBuffer = coyoteTime;
            canActivateCoyote = false;
        }
        coyoteBuffer -= Time.deltaTime;
    }

    // ----------------------------------------
    // Jump Buffer & Input
    // ----------------------------------------

    void UpdateJumpBuffer()
    {
        // Detect jump press and release
        if (isJumping && !inputHandler.jumpHeld)
        {
            stoppedHoldingJump = true;
        }

        //If the buffer has ended, stop storing the input
        if (jumpBuffer <= 0f)
        {
            storingJumpInput = false;
        }

        // Buffer the jump press so the timing for a jump is not as strict
        if (inputHandler.jumpPressed)
        {
            jumpBuffer = jumpBufferTime;
            storingJumpInput = true;
        }




        jumpBuffer -= Time.deltaTime;
    }

    // ----------------------------------------
    // Input Processing & Jump Trigger
    // ----------------------------------------

    void ProcessMovementInput()
    {
        if (kicking || attacking) return;
        movementInput = new Vector2(inputHandler.movement.x, 0f);

        // Handle jump action if in buffer and within coyote time
        if (storingJumpInput && (((onGround || coyoteBuffer > 0f) && !isJumping) || inWater))
        {
            PerformJump();
            if (!canKick && !inWater) //only renable kicking when jumping from ground to air
            {
                canKick = true;
                ShowFlash();
            }
        }
        else if (onGround && (!inputHandler.jumpHeld || storingJumpInput) && verticalVelocity <= 0f)
        {
            // Reset jump once button has been released (or if the button is being held, it is within the jump buffer for now)
            isJumping = false;
        }

        //So if let go once, and now holding again, gliding can be activated as long as the player is not on the ground
        if (stoppedHoldingJump || (!isJumping && !onGround))//isJumping && stoppedHoldingJump)
        {
            //Can only glide when falling (to prevent weird rising glide shennanigans)
            if (!gliding && canGlide && verticalVelocity <= 0 && !inWater)
            {
                gliding = inputHandler.jumpHeld && !onGround;
                if (gliding) OpenParachute?.Invoke();
            }
            else if (gliding)
            {
                //gliding = inputHandler.jumpHeld && !onGround;
            }
            if (gliding)
            {
                gliding = inputHandler.jumpHeld && !onGround;
                storingJumpInput = false;
            }
        }


        // On ground or input zero, snap horizontal direction to input
        if (onGround || movementInput.x == 0f)
            xMovement = movementInput.x;
    }

    void PerformJump(bool jumpWithFullForce = false, bool playSFX = true)
    {
        if (playSFX) Jump?.Invoke();
        //if jump with full force is true, just do the jump regardless of where the player is
        verticalVelocity = inWater && !jumpWithFullForce ? swimBobForce : jumpForce;
        if(inWater) stoppedHoldingJump = false; //Because in water, you are bobbing, so you don't land, meaning this doesn't turn false
        isJumping = true;
        storingJumpInput = false;
        //make sure gliding has stopped
        gliding = false;

        //Get out of any child things, e.g. moving platforms
        ExitPlatform();
    }

    // ----------------------------------------
    // Attacking 
    // ----------------------------------------

    void Attack()
    {
        //If not attacking, start the attack
        if (!attacking)
        {
            PunchAction?.Invoke();
            attacking = true;
            attackFinished = false;
        }
        else if (attacking)
        {
            // If the window to start another attack is still open, and the previous attack has finished
            if (attackBuffer > 0 && attackFinished)
            {
                PunchAction?.Invoke();
                //Attack again
                anim.SetTrigger("Attack");
                attackFinished = false;
            }
            else if (!attackFinished)
            {
                //Otherwise, store the input for now (because the current attack hasn't finished)
                attackInputBuffer = attackInputBufferWindow;
            }
        }

    }

    void CheckAttackBuffer()
    {
        // If on the ground, it is possible to attack. The is jumping check is to ensure that the player isn't holidng space (whihc is why they wouldnt be able to jump)
        if (inputHandler.attackPressed && onGround && (!isJumping || inputHandler.jumpHeld) && !kicking)
        {
            Attack();
        }
        if (attacking)
        {
            //If attacking, when the window for the next attack runs out, stop attacking
            if (attackBuffer <= 0 && attackFinished)
            {
                attacking = false;
                attackInputBuffer = 0;
            }
            attackBuffer -= Time.deltaTime;
        }
        //Decrement the input buffer regardless of anything
        attackInputBuffer -= Time.deltaTime;

    }

    // Called from the animation event at the end of the attack animation
    public void AttackFinished()
    {
        punchHitbox.enabled = false;
        attackFinished = true;
        attackBuffer = attackBufferWindow; //allow the window for the next attack
        //If an input has already come through (and is within the buffer), attack again
        if (attackInputBuffer > 0)
        {
            Attack();
        }
    }

    void ManageHitboxDirection()
    {
        punchHitbox.transform.rotation = Quaternion.Euler(0, Mathf.Clamp(180 - anim.GetFloat("Horizontal") * 180, 0, 180), 0);
        kickHitbox.transform.rotation = Quaternion.Euler(0, Mathf.Clamp(180 - anim.GetFloat("Horizontal") * 180, 0, 180), 0);

    }

    // Called from animation event
    public void EnableHitbox()
    {
        punchHitbox.enabled = true;
    }

    // ----------------------------------------
    // Kicking 
    // ----------------------------------------

    void Kick()
    {
        if (!kicking)
        {
            KickAction?.Invoke();
            kicking = true;
            kickTimer = kickLength;
            canKick = false; //can only kick once in the air, have to wait to land again
            kickHitbox.enabled = true;
            horizontalVelocity = kickInitialSpeed * anim.GetFloat("Horizontal");
            additiveForce = Vector2.zero;//cancel all forces
            ExitPlatform();
        }
    }

    void CheckKickTimer()
    {
        if (inputHandler.specialPressed && !kicking && !attacking && canKick)
        {
            Kick();
        }

        kickTimer -= Time.deltaTime;
        if (kickTimer <= 0)
        {
            if (!kicking && !canKick)
            {
                //If the delay has passed, allow kicking agian
                canKick = (onGround && (!inputHandler.jumpHeld || storingJumpInput) && verticalVelocity <= 0f) || inWater;//true;
                if (canKick) ShowFlash();
            }
            else if (kicking)
            {
                StopKick();
            }
        }
    }

    void StopKick()
    {
        //If kicking, stop kicking, and set up the kick lag (i.e. time before being allowed to kick again)
        kickTimer = inWater ? swimKickLag : onGround ? groundKickLag : 0;
        kicking = false;
        canKick = false;
        kickHitbox.enabled = false;
    }

    // ----------------------------------------
    // Animation Updates
    // ----------------------------------------

    void UpdateAnimations()
    {
        // Horizontal movement parameters
        if (movementInput.x != 0f)
            anim.SetFloat("Horizontal", movementInput.x);
        anim.SetFloat("Speed", Mathf.Abs(movementInput.x));
        if (movementInput.x != 0f)
            spriteRenderer.flipX = movementInput.x < 0f;

        // Jumping & falling states
        anim.SetBool("Jumping", isJumping && verticalVelocity > 0f);
        anim.SetBool("Falling", verticalVelocity < 0f && !onGround);

        // Attacking and kicking states
        anim.SetBool("Attacking", attacking);
        anim.SetBool("Kicking", kicking);
        anim.SetBool("Hurt", hurt);
        anim.SetBool("Gliding", gliding);

        //Swimming
        anim.SetBool("Swimming", inWater);
    }

    // ----------------------------------------
    // Hurt
    // ----------------------------------------


    void CheckHurtTimer()
    {
        if (hurt)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0)
            {
                hurt = false;
                //enable invincibility
                invincibilityTimer = hitInvincibilityDuration;
                invincible = true;
            }
        }
    }

    bool invincible = false;
    void CheckInvincibilityTimer()
    {
        if (invincible)
        {
            if (invincibilityTimer <= 0)
            {
                invincible = false;
            }
            invincibilityTimer -= Time.deltaTime;
        }
    }

    // ----------------------------------------
    // Swimming
    // ----------------------------------------

    void StartSwimming()
    {
        //set all states to false when in water
        inWater = true;
        verticalVelocity = verticalVelocity /= 2;
        //if(verticalVelocity > 0) verticalVelocity /= 2; //halve velocity
        ResetStates(false);
        //kickTimer = 0; //Don't make the kick timer delay upon hitting the water
    }

    void GetOutOfWater()
    {
        inWater = false;
        //Only if going up, then jump out of water
        if (verticalVelocity > 0 && !waterAbove)
        {
            PerformJump(true, false); //jump out
        }
        //So if got out of the water, reduce the kick lag again
        if (!canKick)
        {
            kickTimer -= (swimKickLag - groundKickLag);
        }
    }

    // ----------------------------------------
    // Platform Logic
    // ----------------------------------------
    public void SetPlatformDelta(Vector2 movement)
    {
        platformDelta = movement;
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

    // ----------------------------------------
    // Flash
    // ----------------------------------------

    void ShowFlash()
    {
        StartCoroutine(ControlFlashTiming());
    }

    IEnumerator ControlFlashTiming()
    {
        var mat = spriteRenderer.material;
        // 0 = normal, 1 = full white
        mat.SetFloat("_Flash", 1.0f);
        yield return new WaitForSeconds(0.05f);
        mat.SetFloat("_Flash", 0f);
    }

    float startAlpha = 1;
    float endAlpha = 0;
    float alphaLerp;
    void InvincibilityFlash()
    {
        if (invincible)
        {
            if (alphaLerp >= 1)
            {
                alphaLerp = 0;
                float tempAlpha = startAlpha;
                startAlpha = endAlpha;
                endAlpha = tempAlpha;
            }
            //set alpha to and fro
            spriteRenderer.color = new Color(1, 1, 1, Mathf.Lerp(startAlpha, endAlpha, alphaLerp));
            alphaLerp += 20 * Time.deltaTime;
        }
        else
        {
            spriteRenderer.color = new Color(1, 1, 1, 1);
            startAlpha = 1;
            endAlpha = 0;
        }
    }

    // ----------------------------------------
    // Death
    // ----------------------------------------
    void Die()
    {
        hurt = true;
        Death?.Invoke();
        ResetStates();
        inWater = false;
        boxCollider.enabled = false;
    }

    void ResetStates(bool resetVelocity = true)
    {
        gliding = false;
        isJumping = false;
        stoppedHoldingJump = false;
        gliding = false;
        movementInput = Vector2.zero;
        if (resetVelocity) verticalVelocity = 0;
        attacking = false;
        if(!inWater || kicking) StopKick();
    }

    // ----------------------------------------
    // Horizontal Movement in Air & Ground
    // ----------------------------------------

    void UpdateHorizontalMovement()
    {
        // A test bool to ignore the air acceleration changes
        if (!useAccel)
        {
            xMovement = movementInput.x;
            currentHorizontalDir = movementInput.x;
            return;
        }

        // If changing direction mid-air, reset momentum (so changing direction stops the movement immediately)
        if (currentHorizontalDir != movementInput.x)
            xMovement = 0f;

        //If getting off the ice (jumping off) in the opposite direction, make sure to slow the change of direction down as well
        if(directionOfIceAcceleration == movementInput.x && Mathf.Sign(horizontalVelocity) != directionOfIceAcceleration && !onIce)
        {
            xMovement = 0f;
            directionOfIceAcceleration = 0;
        }

        // If the current direction of movement is different to the player's input, accelerate towards the correct direction (not immediate)
        if (xMovement != movementInput.x)
        {
            //Use the difference as an input for the curve I want for the acceleration
            float speedRatio = Mathf.Clamp01(Mathf.Abs(xMovement));
            float curveValue = airAccelCurve.Evaluate(speedRatio);
            //Accelerate by the value on the curve. The curve starts slow and then spikes (so after a short period, acceleration is instant)
            float accelThisFrame = curveValue * maxAirAccelerationChangeRate;
            xMovement += Mathf.Sign(movementInput.x) * accelThisFrame * Time.deltaTime;

            // Clamp to full input once exceeded
            if (Mathf.Abs(xMovement) > 1f)
                xMovement = movementInput.x;
        }

        currentHorizontalDir = movementInput.x;
    }

    // ----------------------------------------
    // Physics Movement
    // ----------------------------------------

    void ApplyMovement()
    {
        Vector2 finalMovement = new Vector2();
        //Only add new movement if not attacking
        if (!attacking)
        {

        }
        // Extra jump force when holding jump (only if they haven't let go yet). Don't apply when above max velocity
        if (inputHandler.jumpHeld && verticalVelocity > 0f && !stoppedHoldingJump && !inWater && verticalVelocity < maxVerticalVelocity)
        {
            verticalVelocity += extraJumpForce;
        }

        //Decide on stats based on if in water or not (or attacking)
        float moveSpeed = inWater ? swimSpeed :  attacking ? 0 :groundMoveSpeed;
        float gravityValue = inWater ? waterGravityForce : gravityForce;
        float maximumNegativeVelocity = inWater ? terminalWaterNegativeVelocity : (gliding ? glideNegativeVelocity : terminalNegativeVelocity);

        // Ice movement
        if (onIce && !kicking)
        {
            //float directionOfIceAcceleration; //direction of acceleration
            bool skidding = false; //If the player has stopped moving, acceleration should go in the opposite way of travel
            if (xMovement == 0)
            {
                directionOfIceAcceleration = -Mathf.Sign(horizontalVelocity); //so decelerate in the other direction
                skidding = true;
            }
            else
            {
                //If the player is starting from a standing point, then make sure they have to accelerate on the ice
                if (horizontalVelocity == 0)
                {
                    directionOfIceAcceleration = -xMovement;
                }
                else
                {
                    directionOfIceAcceleration = xMovement;
                }
            }
            if (directionOfIceAcceleration <= 0)
            {
                //So if moving left 
                print("moving left");
                if (skidding)
                {
                    //If skidding, come to a stop (i.e. set max velocity to 0)
                    horizontalVelocity = Mathf.Max(0, horizontalVelocity + iceTurnAcceleration * directionOfIceAcceleration * Time.fixedDeltaTime);
                }
                else
                {
                    //Otherwise, move towards that direction (up to max movement speed)
                    horizontalVelocity = Mathf.Max(directionOfIceAcceleration * moveSpeed, horizontalVelocity + iceTurnAcceleration * directionOfIceAcceleration * Time.fixedDeltaTime);
                }
            }
            else
            {
                print("moving right");
                //So if moving right
                if (skidding)
                {
                    //If skidding, come to a stop
                    horizontalVelocity = Mathf.Min(0, horizontalVelocity + iceTurnAcceleration * directionOfIceAcceleration * Time.fixedDeltaTime);
                }
                else
                {
                    //Otherwise, move towards that direction (up to max movement speed)
                    horizontalVelocity = Mathf.Min(directionOfIceAcceleration * moveSpeed, horizontalVelocity + iceTurnAcceleration * directionOfIceAcceleration * Time.fixedDeltaTime);
                }
            }
        }
        else
        {
            // Horizontal velocity is directly from xMovement (unless on ice). If kicking, then move forwards according to the kick speed
            horizontalVelocity = !kicking ? moveSpeed * xMovement : horizontalVelocity;
            horizontalVelocity = inKickLag ? 0 : horizontalVelocity;
        }

        // Compute displacements (SUVAT)
        // If on ice, accelerate into chosen direction
        float dx = onIce ?
            horizontalVelocity * Time.fixedDeltaTime + 0.5f * iceTurnAcceleration * xMovement * Time.fixedDeltaTime * Time.fixedDeltaTime :
            horizontalVelocity * Time.fixedDeltaTime;


        //Don't move downwards if on the ground or kicking
        bool groundedAndNotRisingOrKicking = (verticalVelocity <= 0f && onGround) || kicking;
        float dy = groundedAndNotRisingOrKicking ?
            0f :
            verticalVelocity * Time.fixedDeltaTime + 0.5f * gravityValue * Time.fixedDeltaTime * Time.fixedDeltaTime;

        // Update vertical velocity (SUVAT), assuming initial velocity is 0. If on the ground, velocity is automatically 0
        verticalVelocity = groundedAndNotRisingOrKicking
            ? 0f
            : Mathf.Max(maximumNegativeVelocity, verticalVelocity + gravityValue * Time.fixedDeltaTime);
        if (kicking)
        {
            float speedRatio = Mathf.Clamp01(1 - kickTimer / kickLength);
            float curveValue = kickAccelCurve.Evaluate(speedRatio);
            //Accelerate by the value on the curve. The curve starts slow and then spikes (so after a short period, acceleration is instant)
            float accelThisFrame = curveValue * kickSlowdownRate * anim.GetFloat("Horizontal");
            //Make sure to move the velocity to 0 from the correct direction when kicking
            horizontalVelocity = anim.GetFloat("Horizontal") * kickInitialSpeed * curveValue;
        }
        //If no horizontal movement, block it
        if (blockHorizontalMovement) dx = 0;
        finalMovement = new Vector2(dx, dy);

        //Platform movement
        if (platformToFollow)
        {
            finalMovement += platformDelta;
        }

        //Additive force falloff
        additiveForcePercentage = Mathf.Max(0, additiveForcePercentage - Time.deltaTime * additiveForceFalloff);
        finalMovement += additiveForce * additiveForcePercentage; //add any additional force.

        // Apply movement to Rigidbody2D
        rigid.MovePosition(rigid.position + finalMovement);
    }

    void GetHit()
    {
        //Get hit and force the player down (so halt all their movement)
        health -= 1;
        hurtTimer = hurtTime;
        hurt = true;
        ResetStates();
        Hit?.Invoke(health);
        if (health <= 0)
        {
            Die();
        }

    }

    public void Respawn(Vector3 respawnPosition)
    {
        //Puts the player back to the respawn position
        transform.position = respawnPosition;
        health = maxHealth;
        hurt = false;
        ResetStates();
        inWater = false;
        boxCollider.enabled = true;
        hurtTimer = 0;

    }

    void Heal(float healAmount)
    {
        health = Mathf.Clamp(health + healAmount, 0, maxHealth);
        Healed?.Invoke(health);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            Heal(1); //heal 1 for now
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Water") && !(!waterAbove && verticalVelocity > 0) && canGlide && !inWater) //So if we are in water (and it is not shallow by using canGlide), and not currently jumping out of it, then start swimming
        {
            StartSwimming();
        }
        if (collision.CompareTag("Gap") && !LevelManager.cannotAct) //so if not already dead
        {
            Die();
        }
        if ((collision.CompareTag("Enemy") || collision.CompareTag("PlayerObstacle")) && !hurt && !invincible && !kicking)//invincibleTimer <= 0)
        {
            //get hit
            GetHit();
        }
        if (collision.CompareTag("MovingPlatform") && platformToFollow == null && onGround)
        {
            print("Touch moving platform");
            //Set the moving platform
            platformToFollow = collision.GetComponentInParent<PlatformToFollow>();
            platformToFollow.SetPlayer(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("MovingPlatform") && platformToFollow)
        {
            ExitPlatform();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Spring"))
        {
            Bounce?.Invoke();
            //if kicking, cancel it
            if (kicking) StopKick();
            float temp = jumpForce;
            jumpForce *= 2;
            PerformJump(true, false);
            jumpForce = temp;
            stoppedHoldingJump = false;
        }
        if (collision.gameObject.CompareTag("SuperSpring"))
        {
            Bounce?.Invoke();
            //if kicking, cancel it
            if (kicking) StopKick();
            float temp = jumpForce;
            jumpForce *= 3;
            PerformJump(true, false);
            jumpForce = temp;
            additiveForce = new Vector2(currentHorizontalDir, 0) * springXModifier; //Add this onto the player's movement as a result of touching this spring
            additiveForcePercentage = 1;
            stoppedHoldingJump = false;
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        //CHECK FOR COLLISIONS WITH SOLID SURFACES
        //Check if this is ground
        int otherLayer = collision.gameObject.layer;
        if ((groundLayer.value & (1 << otherLayer)) == 0) return;

        // guard: expect one contact but handle 0 defensively
        if (collision.contactCount == 0)
            return;
        // use the single contact (no averaging)
        ContactPoint2D contact = collision.GetContact(0);
        const float threshold = 0.5f;
        Vector2 normal = contact.normal; // points from the other collider -> this collider
        if (normal.y > 0) //For standing on top of objects, there is no threshold
        {
            // landed on top of the object (ground under us)
            Debug.Log("Collision from above (landed on top of object).");
        }
        else if (normal.y < -threshold) //For bumping head on objects
        {
            // hit underside (head bump, so stop the rising input)
            Debug.Log("Collision from below (hit head).");
            inputHandler.jumpHeld = false;
            if(verticalVelocity > 0)
            {
                //Make the player lose velocity somewhat (don't want to float under the platform)
                verticalVelocity -= maxVerticalVelocity * platformBonkVelocityHit;
            }
            //verticalVelocity = 0;
        }

    }
}

