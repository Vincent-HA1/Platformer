using UnityEngine;

public class Bird : BaseEnemy
{
    [Header("Bird Attributes")]
    [SerializeField] float xMoveSpeed = 4;
    [SerializeField] float yMoveSpeed = 2;
    [SerializeField] float maxMovementPerFrame = 4;


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (hurt || !playerDetected) return;
        MoveTowardsPlayer();
    }

    protected override void DetectPlayer()
    {
        //Detect the player through collider. Bird never stops
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        if (playerCollider != null)
        {
            player = playerCollider.transform;
            if(!playerDetected)
            {
                playerDetected = true;
                moving = true;
            }
        }

    }

    void MoveTowardsPlayer()
    {
        //Tries to move in a linear way towards the target, avoiding jerky movements
        Vector2 current = transform.position;
        Vector2 target = player.position;

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

    protected override void Patrol()
    {
        if (!playerDetected && moving)
        {
            //Move until moving is false. The move timers dictate this
            rigid.MovePosition(rigid.position + moveDirection * Time.fixedDeltaTime * xMoveSpeed);
        }
    }
}
