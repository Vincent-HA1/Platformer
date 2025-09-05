using System.Collections;
using System.Collections.Generic;
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
        startColor = spriteRenderer.color;
        endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
    }

    // Update is called once per frame
    void Update()
    {
        ManageVanishTimer();
        platformCollider.enabled = spriteRenderer.color.a > 0.2f; 
    }

    void ManageVanishTimer()
    {

        if (changing)
        {
            vanishTimer -= Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, 1 - vanishTimer / vanishTime);
            if (vanishTimer <= 0)
            {
                vanishDelayTimer = vanishDelay;
                changing = false;
            }
        }
        else
        {
            vanishDelayTimer -= Time.deltaTime;
            if (vanishDelayTimer <= 0)
            {
                vanishTimer = vanishTime;
                changing = true;
                Color temp = startColor;
                startColor = endColor;
                endColor = temp;
            }

        }
    }
}
