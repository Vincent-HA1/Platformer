using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBlock : MonoBehaviour
{
    PlatformToFollow platformScript;
    public LayerMask groundLayer;
    [Header("Ice Block Attributes")]
    [SerializeField] float iceSlideSpeed = 7;
    [SerializeField] float wallCheckDistance = 0.05f;

    CompositeCollider2D compositeCollider;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rigid;
    bool sliding = false;
    float slideDirection;

    ContactFilter2D contactFilter;
    // Start is called before the first frame update
    void Start()
    {
        compositeCollider = GetComponent<CompositeCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformScript = GetComponent<PlatformToFollow>();
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(groundLayer);
        contactFilter.useTriggers = false;
        contactFilter.useLayerMask = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CheckForWall()
    {
        if (sliding)
        {
            //Start the check outside of the sprite
            Vector2 checkPos = (Vector2)transform.position + new Vector2(slideDirection * spriteRenderer.bounds.extents.x, 0);
            RaycastHit2D[] results = new RaycastHit2D[10];
            //Physics2D.BoxCast(transform.position, new Vector2(wallCheckDistance * slideDirection, 0.2f), 0, new Vector2(slideDirection, 0), contactFilter, results, wallCheckDistance);
            int wallsInFront = Physics2D.BoxCastNonAlloc(checkPos, new Vector2(wallCheckDistance, 0.2f), 0, new Vector2(slideDirection, 0), results, 0, groundLayer);//compositeCollider.Raycast(new Vector2(slideDirection, 0), results, wallCheckDistance, groundLayer);
            // Stop sliding if hitting a wall (that is not self)
            if (wallsInFront > 0)
            {
                // Remove self colliders
                foreach(RaycastHit2D hit in results)
                {
                    if(hit.collider !=null && hit.collider.gameObject == gameObject)
                    {
                        wallsInFront -= 1;
                    }
                }
                if(wallsInFront > 0) sliding = false;
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hitbox") && !sliding)
        {
            //Check if it contacted the actual ice block
            KnockAway();
            //Slide away from the hit
            slideDirection = Mathf.Sign(transform.position.x - collision.transform.position.x);
        }
        if (collision.CompareTag("Gap") && !LevelManager.cannotAct) //so if not already dead
        {
            StartCoroutine(Disappear());
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (sliding && !collision.collider.CompareTag("Player"))
        {
            //sliding = false;
        }
    }



    IEnumerator Disappear()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }

    void KnockAway()
    {
        sliding = true;
    }

    private void FixedUpdate()
    {
        CheckForWall();
        if (sliding)
        {
            //Move the block if it needs to be moved
            Vector2 movement = new Vector2(slideDirection * iceSlideSpeed * Time.fixedDeltaTime, 0);
            rigid.MovePosition(rigid.position + movement);
            if(platformScript) platformScript.SetPlatformDelta(movement);
        }

    }

    private void OnDrawGizmos()
    {
        Vector2 checkPos = (Vector2)transform.position + new Vector2(slideDirection * spriteRenderer.bounds.extents.x, 0);
        Gizmos.DrawLine(checkPos, checkPos + new Vector2(slideDirection, 0));
    }
}
