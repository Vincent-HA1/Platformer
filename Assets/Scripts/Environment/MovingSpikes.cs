using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSpikes : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Camera camera;
    [SerializeField] Transform player;

    [Header("Attributes")]
    [SerializeField] float moveSpeed = 6;
    [SerializeField] float startingXRatio = 0.2f;

    Rigidbody2D rigid;

    float storedXPosition;
    bool setXPosition;
    float yDifference;
    float startingY;
    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        startingY = transform.position.y;
    }

    private void LateUpdate()
    {
        //transform.position = new Vector3(transform.position.x, player.position.y, transform.position.z);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        //if (LevelManager.cannotAct && !setXPosition) return;
        float xPos;
        if (!setXPosition)
        {
            if (LevelManager.cannotAct)
            {
                xPos = rigid.position.x; //dont move
            }
            else
            {
                xPos = rigid.position.x + moveSpeed * Time.fixedDeltaTime;
            }
        }
        else
        {
            xPos = camera.ViewportToWorldPoint(new Vector3(startingXRatio, 0, 0)).x;
            setXPosition = false;
        }
        Vector2 final = new Vector2(xPos, transform.position.y);//camera.transform.position.y);//startingY + yDifference);
        rigid.MovePosition(final);
    }

    public void SetPosition()
    {
        setXPosition = true;
        //print(new Vector3(startingXRatio, 0, 0));
        //print(camera.ViewportToWorldPoint(new Vector3(startingXRatio, 0, 0)));
        //float xPosition = camera.ViewportToWorldPoint(new Vector3(startingXRatio, 0, 0)).x;
        //transform.position = new Vector3(xPosition, transform.position.y, transform.position.z);
    }
}
