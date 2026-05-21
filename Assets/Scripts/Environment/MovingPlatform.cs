using System.Collections;
using UnityEngine;

public class MovingPlatform : PlatformToFollow
{
    [Header("References")]
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;
    [SerializeField] Transform platform;

    [Header("Attributes")]
    [SerializeField] float moveSpeed = 5;
    [SerializeField] int minWaitTime = 2;
    [SerializeField] int maxWaitTime = 3;
    [SerializeField] bool waitForPlayer = false; // whether the platform only starts moving if the player gets on it

    Rigidbody2D rigid;
    Vector2 currentStartPos;
    Vector2 currentEndPos;

    Vector2 lastPos;
    float lerp = 0;

    bool waiting = false;
    bool canMove = false;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        currentStartPos = startPoint.position;
        currentEndPos = endPoint.position;
        lastPos = currentStartPos;
        canMove = !waitForPlayer;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MovePlatform();
    }

    void MovePlatform()
    {
        if (waiting || !canMove) return;
        if (lerp < 1)
        {
            //Move towards the position for now
            lerp += moveSpeed * Time.fixedDeltaTime;
            Vector2 origPos = platform.position;
            Vector2 newPos = Vector3.Lerp(currentStartPos, currentEndPos, lerp);
            Vector2 difference = newPos - lastPos; //calculate the difference so can move the player along with it
            rigid.MovePosition(newPos);
            SetPlatformDelta(difference);
            lastPos = newPos;
        }
        else
        {
            //Stop moving, and wait for a set amount of time
            Vector2 temp = currentStartPos;
            currentStartPos = currentEndPos;
            currentEndPos = temp;
            lerp = 0;
            SetPlatformDelta(Vector2.zero);//playerMovement.SetPlatformDelta(Vector2.zero);
            StartCoroutine(WaitAtPosition());
        }
    }


    IEnumerator WaitAtPosition()
    {
        //Wait at the current position for a period of time.
        waiting = true;
        int waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);
        waiting = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            Vector2 normal = contact.normal;
            if (normal.y < 0) //player above/landed on platform
            {
                // landed on top of the object (ground under us)
                Debug.Log("Collision from above (landed on top of object).");
                if(waitForPlayer) canMove = true;
                SetPlatformDelta(Vector2.zero);
            }
        }
    }
}
