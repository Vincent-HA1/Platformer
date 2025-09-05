using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : BaseEnemy
{
    public LayerMask groundLayer;
    public LayerMask waterLayer;

    [Header("Bird Attributes")]
    [SerializeField] float xMoveSpeed = 4;
    [SerializeField] float yMoveSpeed = 2;
    [SerializeField] float maxMovementPerFrame = 4;
    [SerializeField] float wallCheckDistance = 0.8f;

    Vector3 startPoint;
    bool returningToStartPoint = false;

    protected override void Start()
    {
        base.Start();
        startPoint = transform.position;
    }

    protected override void DetectPlayer()
    {
        //Detect the player through collider. Bird never stops
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        if (playerCollider != null && !returningToStartPoint)
        {
            player = playerCollider.transform;
            if (!playerDetected)
            {
                playerDetected = true;
                moving = true;
            }
        }

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (hurt) return;
        if (returningToStartPoint)
        {
            if(Vector2.Distance(startPoint, transform.position) <= 0.1f)
            {
                print("at start ppint");
                returningToStartPoint = false;
                playerDetected = false;
                //start patrolling agian
                moveDirection = new Vector2(UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1, 0);
            }
            else
            {
                print("moving to start poit");
                MoveTowardsTarget(startPoint);
            }
        }
        else if(playerDetected)
        {
            if (!CanSwim())
            {
                print(playerDetected);
                //return to start point
                returningToStartPoint = true;
            }
            else
            {
                print("moving to plaayer");
                MoveTowardsTarget(player.position);
            }

        }
    }

    //Called when changing direction during patrolling.
    protected override void ChangeDirection()
    {
        //Make sure wall is not ahead, if so, have to turn around
        if (!CanSwim())
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

    void MoveTowardsTarget(Vector2 target)
    {
        Vector2 current = transform.position;
        //Vector2 target = player.position;

        Vector2 delta = target - current;
        float dist = delta.magnitude;
        if (dist <= 0f) return;

        Vector2 dir = delta / dist; // normalized
        moveDirection = dir;

        // per-frame axis max deltas
        float xMax = xMoveSpeed * Time.deltaTime;
        float yMax = yMoveSpeed * Time.deltaTime;

        // compute max scalar s so that |s * dir.x| <= xMax and |s * dir.y| <= yMax
        float s = dist; // don't overshoot the target
        const float EPS = 1e-6f;
        if (Mathf.Abs(dir.x) > EPS)
            s = Mathf.Min(s, xMax / Mathf.Abs(dir.x));
        if (Mathf.Abs(dir.y) > EPS)
            s = Mathf.Min(s, yMax / Mathf.Abs(dir.y));

        Vector2 newPos = current + dir * s;
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
    }

    protected override void FixedUpdate()
    {
        if (hurt || returningToStartPoint) return;
        Patrol();
    }

    protected override void Patrol()
    {
        if (!playerDetected && moving && !returningToStartPoint)
        {
            //Move until moving is false. The move timers dictate this
            if (CanSwim())
            {
                rigid.MovePosition(rigid.position + moveDirection * Time.fixedDeltaTime * xMoveSpeed);
            }
            else
            {
                moveTimer = 0; //stop moving
            }
        }
    }

    bool CanSwim()
    {
        //Check if there is wall ahead, or no water ahead
        //raycast for wall
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, moveDirection, wallCheckDistance, groundLayer);
        RaycastHit2D waterHit = Physics2D.Raycast(transform.position, moveDirection, wallCheckDistance, waterLayer);
        return wallHit.collider == null && waterHit.collider != null;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        ////Check if this is ground
        //int otherLayer = collision.gameObject.layer;
        //print(collision.gameObject);
        //if ((groundLayer.value & (1 << otherLayer)) == 0) return;
        //print("GELL");
        //if (playerDetected)
        //{
        //    print(playerDetected);
        //    //return to start point
        //    returningToStartPoint = true;
        //}

    }
}
