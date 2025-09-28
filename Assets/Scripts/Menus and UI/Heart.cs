using UnityEngine;
using UnityEngine.UI;

public class Heart : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Sprite filledHeartSprite;
    [SerializeField] Sprite unfilledHeartSprite;

    Image heartImage;

    void Start()
    {
        heartImage = GetComponent<Image>();
    }


    public void SetFilled()
    {
        heartImage.sprite = filledHeartSprite;
    }

    public void SetEmpty()
    {
        heartImage.sprite = unfilledHeartSprite;
    }
}
