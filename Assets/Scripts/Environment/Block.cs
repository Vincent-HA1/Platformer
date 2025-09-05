using System;
using System.Collections;
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
        //Get hit, play the animation
        health -= 1;
        if(health <= maxHealth / 2)
        {
            //At half health, show the damaged animation.
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
        //get broken
        Break?.Invoke();
        boxCollider.enabled = false;
        anim.SetBool("Destroyed", true);
        StartCoroutine(DestroyCoroutine());
    }

    IEnumerator DestroyCoroutine()
    {
        //Destroy block after set time
        yield return new WaitForSeconds(debrisTime);
        Destroy(gameObject);
    }
}
