using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Enemy does not move, but when it detects the player is reasonably close, it will start its popping in and out pattern
/// Once the player moves away, it will stop
/// Can't be attacked?
/// </summary>
public class Slime : BaseEnemy
{
    [Header("Slime Attributes")]
    [SerializeField] float idleTimeMin = 2;
    [SerializeField] float idleTimeMax = 3;
    [SerializeField] float hideTimeMin = 4;
    [SerializeField] float hideTimeMax = 5;

    bool hiding;
    bool poppedOut;
    float actionTimer;

    protected override void Update()
    {
        base.Update();
        PopInAndOut();
    }

    protected override void UpdateAnims()
    {
        base.UpdateAnims();
        anim.SetBool("Hiding", hiding);

    }

    protected override void DetectPlayer()
    {
        //Detect the player through collider.
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        if (playerCollider != null)
        {
            player = playerCollider.transform;
            if (!playerDetected)
            {
                playerDetected = true;
                actionTimer = hideTimeMin / 2; //Fast coming out for the first time
            }
        }
        else if(playerDetected)
        {
            print("paleyr gone");
            playerDetected = false;
            Hide();
        }

    }

    void PopInAndOut()
    {
        //So after a few seconds, should pop out and go into idle
        //After another timer, go back into the ground
        if (playerDetected)
        {
            //Wait to do the next action
            actionTimer -= Time.deltaTime;
            if(actionTimer <= 0)
            {
                // If hiding, pop out, and vice versa
                if (hiding)
                {
                    hiding = false;
                    actionTimer = Random.Range(idleTimeMin, idleTimeMax);
                    anim.SetTrigger("PopOut");
                }
                else
                {
                    actionTimer = Random.Range(hideTimeMin, hideTimeMax);
                    hiding = true;
                }
            }
        }
    }
    void Hide()
    {
        //return into the ground if the player is not there
        hiding = true;
        //actionTimer = 0;//reset the timer
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        //Slime has no collisions. It cannot get hurt
    }
}
