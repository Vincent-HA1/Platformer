using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BigCoinIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Sprite bigCoin;
    [SerializeField] Sprite bigCoinOutline;

    Image coinImage;
    // Start is called before the first frame update
    void Awake()
    {
        coinImage = GetComponent<Image>();
    }

    public void SetFound()
    {
        coinImage.sprite = bigCoin;
    }

    public void SetEmpty()
    {
        coinImage.sprite = bigCoinOutline;
    }
}
