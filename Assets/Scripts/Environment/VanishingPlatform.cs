using UnityEngine;

public class VanishingPlatform : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] float vanishTime = 1.5f;
    [SerializeField] float vanishDelay = 0.5f;

    SpriteRenderer spriteRenderer;
    Collider2D platformCollider;

    float vanishTimer = 0;
    float vanishDelayTimer = 0;
    bool changing = false;

    Color startColor;
    Color endColor;
    // Start is called before the first frame update
    void Start()
    {
        platformCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        //Set the colours to lerp to. So just visible and transparent
        startColor = spriteRenderer.color;
        endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
    }

    // Update is called once per frame
    void Update()
    {
        ManageVanishTimer();
        platformCollider.enabled = spriteRenderer.color.a > 0.2f; //if the platform is below this alpha, then disable the collider
    }

    void ManageVanishTimer()
    {
        //Change alpha
        if (changing)
        {
            vanishTimer -= Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, 1 - vanishTimer / vanishTime);
            if (vanishTimer <= 0)
            {
                //Delay for a set amount of time before changing alpha again
                vanishDelayTimer = vanishDelay;
                changing = false;
            }
        }
        else
        {
            //Wait until the delay is over
            vanishDelayTimer -= Time.deltaTime;
            if (vanishDelayTimer <= 0)
            {
                //Flip the colours (so go back to transparent/visible), and reset the timer
                vanishTimer = vanishTime;
                changing = true;
                Color temp = startColor;
                startColor = endColor;
                endColor = temp;
            }

        }
    }
}
