using System;
using UnityEngine;

/// <summary>
/// Simple utilities to get sprite size *excluding* blank/transparent space.
/// - GetTrimmedSizeFromRect(sprite): returns sprite.rect.width/height (fast, matches Inspector).
/// - GetTightSizeFromPixels(sprite, alphaThreshold): reads pixels and computes tight bounds (pixel-accurate).
/// NOTE: GetTightSizeFromPixels requires the sprite texture to be readable (enable Read/Write in importer),
///       or use a readable copy helper if you need runtime support for non-readable textures.
/// </summary>
public static class SpriteSizeSimple
{
    /// <summary>
    /// Fast: returns the trimmed rectangle size (pixels) that Unity shows in the Inspector.
    /// This is usually what you want.
    /// </summary>
    public static Vector2Int GetTrimmedSizeFromRect(Sprite s)
    {
        if (s == null) return Vector2Int.zero;
        return new Vector2Int(Mathf.RoundToInt(s.rect.width), Mathf.RoundToInt(s.rect.height));
    }

    /// <summary>
    /// Returns the tight bounding pixel size (width, height) of non-transparent pixels inside the sprite.
    /// Uses safe clamped GetPixels. If texture is not readable this will log and return Vector2Int.zero.
    /// </summary>
    public static Vector2Int GetTightSizeFromPixelsSafe(Sprite s, float alphaThreshold = 0.01f)
    {
        if (s == null) return Vector2Int.zero;
        var tex = s.texture;
        if (tex == null) return Vector2Int.zero;

        // Use textureRect (may be fractional). Compute integer read rectangle safely.
        Rect tr = s.textureRect;

        int x0 = Mathf.FloorToInt(tr.x);
        int y0 = Mathf.FloorToInt(tr.y);
        int x1 = Mathf.CeilToInt(tr.x + tr.width);
        int y1 = Mathf.CeilToInt(tr.y + tr.height);

        // Clamp to texture bounds
        x0 = Mathf.Clamp(x0, 0, tex.width - 1);
        y0 = Mathf.Clamp(y0, 0, tex.height - 1);
        x1 = Mathf.Clamp(x1, x0 + 1, tex.width);   // ensure at least 1 pixel width
        y1 = Mathf.Clamp(y1, y0 + 1, tex.height);

        int readW = x1 - x0;
        int readH = y1 - y0;

        Color[] pixels;
        try
        {
            pixels = tex.GetPixels(x0, y0, readW, readH);
        }
        catch (Exception ex)
        {
            Debug.LogError($"GetPixels failed for sprite '{s.name}': {ex.Message}. Make sure texture Read/Write is enabled.");
            return Vector2Int.zero;
        }

        // If the readW/readH differ from the sprite.rect (due to rounding/clamping),
        // we will map from the read block to the intended sprite pixel dimensions.
        int intendedW = Mathf.RoundToInt(s.rect.width);
        int intendedH = Mathf.RoundToInt(s.rect.height);

        // If the read area differs, resample nearest-neighbour to intended size.
        if (readW != intendedW || readH != intendedH)
        {
            pixels = ResamplePixelsNearest(pixels, readW, readH, intendedW, intendedH);
            readW = intendedW;
            readH = intendedH;
        }

        // Now compute tight bounds of non-transparent pixels inside the read block.
        int minX = readW, minY = readH, maxX = -1, maxY = -1;
        for (int y = 0; y < readH; ++y)
        {
            for (int x = 0; x < readW; ++x)
            {
                float a = pixels[y * readW + x].a;
                if (a > alphaThreshold)
                {
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (maxX < 0) // nothing opaque
            return Vector2Int.zero;

        int tightW = maxX - minX + 1;
        int tightH = maxY - minY + 1;
        return new Vector2Int(tightW, tightH);
    }

    // Nearest-neighbour resampling of pixel block (used when read area differs from intended sprite rect)
    private static Color[] ResamplePixelsNearest(Color[] src, int srcW, int srcH, int dstW, int dstH)
    {
        if (src == null || src.Length == 0 || srcW <= 0 || srcH <= 0 || dstW <= 0 || dstH <= 0)
            return new Color[dstW * dstH];

        Color[] dst = new Color[dstW * dstH];
        for (int y = 0; y < dstH; ++y)
        {
            int sy = Mathf.Clamp(Mathf.RoundToInt((y / (float)dstH) * srcH), 0, srcH - 1);
            for (int x = 0; x < dstW; ++x)
            {
                int sx = Mathf.Clamp(Mathf.RoundToInt((x / (float)dstW) * srcW), 0, srcW - 1);
                dst[y * dstW + x] = src[sy * srcW + sx];
            }
        }
        return dst;
    }

    /// <summary>
    /// Convenience: returns whether two sprites have the same tight pixel size (using pixel-accurate test).
    /// </summary>
    public static bool HaveSameTightSize(Sprite a, Sprite b, float alphaThreshold = 0.01f)
    {
        var sa = GetTightSizeFromPixelsSafe(a, alphaThreshold);
        var sb = GetTightSizeFromPixelsSafe(b, alphaThreshold);
        return sa == sb;
    }
}
