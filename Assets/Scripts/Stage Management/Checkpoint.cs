using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool isEndFlag = false;
    public bool reachedByGhost = false;
    public Action<Checkpoint> CheckpointReached;
    Animator animator;

    bool reached = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Trigger the checkpoint when the player touches it, or if a ghost reaches the end flag
        if ((collision.CompareTag("Player")||(collision.CompareTag("Ghost") && isEndFlag)) && !reached)
        {
            reachedByGhost = collision.CompareTag("Ghost");
            CheckpointReached?.Invoke(this);
            animator.SetBool("Found", true);
            reached = true;
        }
    }
}
