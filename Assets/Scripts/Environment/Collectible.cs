using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public Action PickedUp;

    [Header("Attributes")]
    [SerializeField] CollectibleType collectibleType;

    public CollectibleType GetCollectibleType()
    {
        return collectibleType;
    }

    public enum CollectibleType
    {
        BigCoin,
        Coin,
        Food
    }

    protected virtual void Collect()
    {
        PickedUp?.Invoke();
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Collect();
        }
    }
}
