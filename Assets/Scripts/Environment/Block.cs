using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Action Break;
    public Action Hit;
    [Header("Attributes")]
    [SerializeField] float maxHealth = 3;
    [SerializeField] float debrisTime = 0.1f;

    float health;
    Animator anim;
    BoxCollider2D boxCollider;
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hitbox"))
        {
            GetHit();
        }

    }

    void GetHit()
    {
        print("get hit");
        //Get hit, play the animation
        health -= 1;
        if(health <= maxHealth / 2)
        {
            anim.SetBool("Damaged", true);
        }
        if (health <= 0)
        {
            Destroy();
        }
        else
        {
            Hit?.Invoke();
        }
    }

    void Destroy()
    {
        Break?.Invoke();
        boxCollider.enabled = false;
        anim.SetBool("Destroyed", true);
        //get broken
        StartCoroutine(DestroyCoroutine());
    }

    IEnumerator DestroyCoroutine()
    {

        yield return new WaitForSeconds(debrisTime);
        Destroy(gameObject);
    }
}
