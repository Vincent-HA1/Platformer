using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{

    public Action Hit;
    public Action Death;

    public LayerMask playerLayer;

    [Header("References")]
    [SerializeField] GameObject deathExplosionPrefab;
    [SerializeField] GameObject foodPrefab;


    [Header("Enemy Attributes")]
    [SerializeField] float maxHealth;
    [SerializeField] bool patrol = true;
    [SerializeField] float hurtTime = 0.5f;
    [SerializeField] float minPatrolTime = 1.5f;
    [SerializeField] float maxPatrolTime = 2;
    [SerializeField] float minWaitTime = 1;
    [SerializeField] float maxWaitTime = 1.25f;
    [SerializeField] protected float detectionRadius = 2;
    [SerializeField] float maxDistanceToPlayer = 5f;


    [Header("Miscellaneous")]
    [SerializeField] float chanceForFoodDrop = 0.2f;

    protected Transform player;
    protected Vector2 moveDirection;

    protected float moveTimer;
    protected float waitTimer;

    protected bool moving = false;
    protected bool hurt = false;
    protected bool playerDetected = false;
    protected bool playerTooFar = false;
    protected Rigidbody2D rigid;
    protected Animator anim;

    private float hurtTimer;
    private float health;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = maxHealth;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        //randomly decide start direcction
        moveDirection = new Vector2(UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1, 0);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        CheckHurtTimer();
        ManageMoveTimers();
        UpdateAnims();
        DetectPlayer();
    }

    void CheckHurtTimer()
    {
        if (hurt)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0)
            {
                hurt = false;
            }
        }

    }

    protected virtual void ManageMoveTimers()
    {
        if (hurt) return;
        if (!playerDetected)
        {
            if (moving)
            {
                //Move for the duration. If finished, wait a bit
                moveTimer -= Time.deltaTime;
                if (moveTimer <= 0)
                {
                    moving = false;
                    waitTimer = UnityEngine.Random.Range(minWaitTime, maxWaitTime);

                }
            }
            else
            {
                //Wait for a set duration before continuing to move
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0)
                {
                    ChangeDirection();
                }
            }
        }

    }

    //Called when changing direction during patrolling.
    protected virtual void ChangeDirection()
    {
        //Base, do not fill with anything
        moveTimer = UnityEngine.Random.Range(minPatrolTime, maxPatrolTime);
        moving = true;
    }

    protected virtual void UpdateAnims()
    {
        spriteRenderer.flipX = moveDirection.x >= 0;
        anim.SetFloat("Speed", moving ? 1 : 0);
        anim.SetBool("Hurt", hurt);
    }

    protected virtual void DetectPlayer()
    {
        //This is just meant to detect the player collider, it is not meant to detect the player
        //Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        //if (playerCollider != null)
        //{
        //    playerColliderThere = true;
        //    player = playerCollider.transform;
        //}
        //else
        //{
        //    playerColliderThere = false;
        //}

        //Detect distance to player
        if (!player || Vector2.Distance(player.position, transform.position) > maxDistanceToPlayer)
        {
            playerTooFar = true;
        }
        else
        {
            playerTooFar = false;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (hurt) return;
        Patrol();
    }


    protected virtual void Patrol() { }


    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hitbox"))
        {
            GetHit();
        }

    }

    protected virtual void GetHit()
    {
        //Get hit, play the animation
        health -= 1;
        hurt = true;
        hurtTimer = hurtTime;
        anim.SetTrigger("Hit");
        if (health <= 0)
        {
            Die();
        }
        else
        {
            Hit?.Invoke();
        }
    }

    void Die()
    {
        Death?.Invoke();
        Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity, null);
        DropFood();
        Destroy(gameObject);
    }

    void DropFood()
    {
        float random = UnityEngine.Random.Range(1, 11) / 10;
        if (random <= chanceForFoodDrop)
        {
            Instantiate(foodPrefab, transform.position, Quaternion.identity, null);
        }
    }
}
