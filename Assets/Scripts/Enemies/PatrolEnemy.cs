using UnityEngine;

public class PatrolEnemy : JumpingEnemy
{
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        ApplyMovement();
    }

    protected override void Patrol()
    {
        if (!playerDetected)
        {
            if (moving && !CanMove())
            {
                //If run into something, change direction
                moveTimer = 0;
            }
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    //Called when changing direction during patrolling.
    protected override void ChangeDirection()
    {
        //If hit a wall, then need to turn backwards. Otherwise, choose a random direction
        if (!CanMove())
        {
            moveDirection = -moveDirection;
        }
        else
        {
            int randomDir = Random.Range(0, 2);
            moveDirection = randomDir == 0 ? moveDirection : -moveDirection;
        }
        base.ChangeDirection();
    }
}