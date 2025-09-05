using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChangeColliderFromSprite : MonoBehaviour
{
    PolygonCollider2D poly;
    SpriteRenderer spriteRenderer;

    Sprite last;
    // Start is called before the first frame update
    void Start()
    {
        poly = GetComponent<PolygonCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (spriteRenderer.sprite != last)
        {
            last = spriteRenderer.sprite;
            UpdateColliderFromSprite(last);
        }
    }

    void UpdateColliderFromSprite(Sprite sprite)
    {
        if (sprite == null)
        {
            poly.pathCount = 0;
            return;
        }

        int shapeCount = sprite.GetPhysicsShapeCount();
        poly.pathCount = shapeCount;

        for (int i = 0; i < shapeCount; i++)
        {
            List<Vector2> points = new List<Vector2>();
            sprite.GetPhysicsShape(i, points);
            // physics shape points are in *local sprite* coordinates (pixels / PPU),
            // PolygonCollider2D expects local units (Sprite's pivots and PPU are handled automatically)
            poly.SetPath(i, points.ToArray());
        }
    }
}
