using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBlock : MonoBehaviour
{
    public LayerMask groundLayer;
    [Header("Ice Block Attributes")]
    [SerializeField] float iceSlideSpeed = 7;
    [SerializeField] float wallCheckDistance = 0.05f;

    CompositeCollider2D compositeCollider;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rigid;
    [SerializeField] bool sliding = false;
    float slideDirection;

    // Start is called before the first frame update
    void Start()
    {
        compositeCollider = GetComponent<CompositeCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
            int wallsInFront = compositeCollider.Raycast(new Vector2(slideDirection, 0), results, wallCheckDistance, groundLayer);
            // Stop sliding if hitting a wall (that is not self)
            if (wallsInFront > 0)
            {
                print(results[0].collider);
                sliding = false;
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hitbox") && !sliding)
        {
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
        }

    }

    private void OnDrawGizmos()
    {
        Vector2 checkPos = (Vector2)transform.position + new Vector2(slideDirection * spriteRenderer.bounds.extents.x, 0);
        Gizmos.DrawLine(checkPos, checkPos + new Vector2(slideDirection, 0));
    }
}
