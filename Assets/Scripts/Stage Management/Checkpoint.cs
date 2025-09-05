using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool isEndFlag = false;
    public Action<Checkpoint> CheckpointReached;
    Animator animator;

    bool reached = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Trigger the checkpoint when the player touches it
        if (collision.CompareTag("Player") && !reached)
        {
            CheckpointReached?.Invoke(this);
            animator.SetBool("Found", true);
            reached = true;
        }
    }
}
