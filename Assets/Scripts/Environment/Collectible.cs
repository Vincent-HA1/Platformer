using System;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] CollectibleType collectibleType;

    public enum CollectibleType
    {
        BigCoin,
        Coin,
        Food
    }
    public Action PickedUp;


    public CollectibleType GetCollectibleType()
    {
        return collectibleType;
    }

    protected virtual void Collect()
    {
        PickedUp?.Invoke();
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Collect when player collides 
        if (collision.CompareTag("Player"))
        {
            Collect();
        }
    }
}
