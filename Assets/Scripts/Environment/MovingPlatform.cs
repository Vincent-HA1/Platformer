using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;
    [SerializeField] Transform platform;

    [Header("Attributes")]
    [SerializeField] float moveSpeed = 5;
    [SerializeField] int minWaitTime = 2;
    [SerializeField] int maxWaitTime = 3;

    Rigidbody2D rigid;
    PlayerMovement playerMovement;
    Vector2 currentStartPos;
    Vector2 currentEndPos;

    Vector2 lastPos;
    float lerp = 0;

    bool waiting = false;

    public void SetPlayer(PlayerMovement playerMovement)
    {
        this.playerMovement = playerMovement;
    }

    public void Disengage()
    {
        playerMovement = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        currentStartPos = startPoint.position;
        currentEndPos = endPoint.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MovePlatform();
    }

    void MovePlatform()
    {
        if (waiting) return;
        if (lerp < 1)
        {
            lerp += Time.fixedDeltaTime;
            Vector2 origPos = platform.position;
            Vector2 newPos = Vector3.Lerp(currentStartPos, currentEndPos, lerp);
            Vector2 difference = newPos - lastPos; //calculate the difference so can move the player along with it
            rigid.MovePosition(newPos);
            if (playerMovement)
            {
                playerMovement.SetPlatformDelta(difference);
            }
            lastPos = newPos;
        }
        else
        {
            //Stop moving, and wait for a set amount of time
            Vector2 temp = currentStartPos;
            currentStartPos = currentEndPos;
            currentEndPos = temp;
            lerp = 0;
            if(playerMovement) playerMovement.SetPlatformDelta(Vector2.zero);
            StartCoroutine(WaitAtPosition());
        }
    }


    IEnumerator WaitAtPosition()
    {
        waiting = true;
        int waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);
        waiting = false;
    }
}
