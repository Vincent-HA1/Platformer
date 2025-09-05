using System;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType
    {
        BigCoin,
        Coin,
        Food
    }
    public Action PickedUp;

    [Header("Attributes")]
    [SerializeField] CollectibleType collectibleType;

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
        if (collision.CompareTag("Player"))
        {
            Collect();
        }
    }
}
