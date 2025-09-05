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

    bool setXPosition;
    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float xPos;
        if (!setXPosition)
        {
            //If not setting it manually, move along with the normal speed
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
            //Set position manually as player has teleported somehwere
            xPos = camera.ViewportToWorldPoint(new Vector3(startingXRatio, 0, 0)).x;
            setXPosition = false;
        }
        Vector2 final = new Vector2(xPos, transform.position.y); //Don't move the y position
        rigid.MovePosition(final);
    }

    public void SetPosition()
    {
        //Set the bool so next physics update set the position
        setXPosition = true;
    }
}
