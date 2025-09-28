using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Handles showing the correct button outline for the hold button action
public class ButtonOutline : MonoBehaviour
{
    [Header("Button Image")]
    [SerializeField] Image buttonIcon;
    [Header("Possible Outlines")]
    [SerializeField] List<Sprite> outlines = new List<Sprite>();

    Image outline;
    Sprite lastSprite;
    // Start is called before the first frame update
    void Start()
    {
        outline = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (buttonIcon.sprite != null && buttonIcon.sprite != lastSprite)
        {
            lastSprite = buttonIcon.sprite;
            SetOutlineSprite();
        }
    }

    void SetOutlineSprite()
    {
        Sprite outlineToChoose = null;
        ////Go trhough list of outlines. if no matching bounds, dont use it
        foreach (Sprite outlineSprite in outlines)
        {
            if (SpriteSizeSimple.HaveSameTightSize(buttonIcon.sprite, outlineSprite))
            {
                //If there is no sprite chosen yet
                if (outlineToChoose == null)
                {
                    outlineToChoose = outlineSprite;
                }
                else
                {
                    //Two sprites with the same rect, so cant choose like this
                    outlineToChoose = null;
                    break;
                }
            }
        }
        if (outlineToChoose != null)
        {
            outline.sprite = outlineToChoose; //Only one sprite with this rect, so must be this outline
        }
        else
        {
            //use mask to figure it out
            //This is a fallback, not guaranteed to work
            Tuple<Result, Sprite> bestMatch = new Tuple<Result, Sprite>(new Result(), null);
            foreach (Sprite outlineSprite in outlines)
            {
                Result result = IsOutlineEdgeLike(outlineSprite, buttonIcon.sprite);
                if (result.score > bestMatch.Item1.score)
                {
                    bestMatch = new Tuple<Result, Sprite>(result, outlineSprite);
                }
                //If fits completely, set immediately
                if (result.pass)
                {
                    outline.sprite = outlineSprite;
                    return;
                }
            }
            outline.sprite = bestMatch.Item2; //set to best match
        }
    }

    //Below code checks if the outline matches the sprite by masking the sprite's pixels and seeing which ones match

    public struct Result { public bool pass; public float score; }

    /// <summary>
    /// Tests outline vs fill.
    /// - neighbor8: use 8-neighbourhood (diagonals included) if true; otherwise 4-neighbour.
    /// - failTolerance: allowed fraction of outline pixels that can fail (0 = strict).
    /// - alphaThreshold: alpha > threshold considered opaque.
    /// </summary>
    public static Result IsOutlineEdgeLike(
        Sprite outlineSprite,
        Sprite fillSprite,
        bool neighbor8 = false,
        float failTolerance = 0f,
        float alphaThreshold = 0.01f)
    {
        if (outlineSprite == null || fillSprite == null)
            return new Result { pass = false, score = 0f };

        // 1) read masks (safe)
        if (!GetSpriteAlphaMaskSafe(outlineSprite, out bool[] outlineMask, out int ow, out int oh, alphaThreshold))
            return new Result { pass = false, score = 0f };
        if (!GetSpriteAlphaMaskSafe(fillSprite, out bool[] fillMask, out int fw, out int fh, alphaThreshold))
            return new Result { pass = false, score = 0f };

        // 2) center on common canvas
        int canvasW = Math.Max(ow, fw);
        int canvasH = Math.Max(oh, fh);

        bool[] canvasOutline = CenterMaskOnCanvas(outlineMask, ow, oh, canvasW, canvasH);
        bool[] canvasFill = CenterMaskOnCanvas(fillMask, fw, fh, canvasW, canvasH);

        // 3) choose neighbor offsets
        int[][] offsets = neighbor8
            ? new int[][] {
                new[]{-1,-1}, new[]{0,-1}, new[]{1,-1},
                new[]{-1, 0},            new[]{1, 0},
                new[]{-1, 1}, new[]{0, 1}, new[]{1, 1}
            }
            : new int[][] {
                new[]{0,-1}, new[]{-1,0}, new[]{1,0}, new[]{0,1}
            };

        int outlineCount = 0;
        int satisfied = 0;

        for (int y = 0; y < canvasH; ++y)
        {
            for (int x = 0; x < canvasW; ++x)
            {
                int idx = y * canvasW + x;
                if (!canvasOutline[idx]) continue; // not an outline candidate

                outlineCount++;

                // Requirement A: the pixel at this position in the fill MUST be filled
                if (!canvasFill[idx])
                {
                    // fail requirement A immediately
                    continue;
                }

                // Requirement B: at least one neighbour in the fill must be empty
                bool hasEmptyNeighbour = false;
                foreach (var off in offsets)
                {
                    int nx = x + off[0];
                    int ny = y + off[1];
                    if (nx < 0 || nx >= canvasW || ny < 0 || ny >= canvasH)
                    {
                        // treat out-of-bounds as empty (outline allowed at edge)
                        hasEmptyNeighbour = true;
                        break;
                    }
                    int nidx = ny * canvasW + nx;
                    if (!canvasFill[nidx]) { hasEmptyNeighbour = true; break; }
                }

                if (hasEmptyNeighbour) satisfied++;
            }
        }

        float score = outlineCount == 0 ? 0f : (float)satisfied / outlineCount;
        bool pass = (1f - score) <= failTolerance;

        return new Result { pass = pass, score = score };
    }

    // ----------------- Helpers (same safe readers / centering / resampling) -----------------

    private static bool GetSpriteAlphaMaskSafe(Sprite s, out bool[] mask, out int wOut, out int hOut, float alphaThreshold)
    {
        mask = null;
        wOut = Mathf.RoundToInt(s.rect.width);
        hOut = Mathf.RoundToInt(s.rect.height);

        Texture2D tex = s.texture;
        if (tex == null) return false;

        Rect tr = s.textureRect;
        int x0 = Mathf.FloorToInt(tr.x);
        int y0 = Mathf.FloorToInt(tr.y);
        int x1 = Mathf.CeilToInt(tr.x + tr.width);
        int y1 = Mathf.CeilToInt(tr.y + tr.height);

        x0 = Mathf.Clamp(x0, 0, tex.width - 1);
        y0 = Mathf.Clamp(y0, 0, tex.height - 1);
        x1 = Mathf.Clamp(x1, x0 + 1, tex.width);
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
            Debug.LogError($"GetPixels failed for sprite '{s.name}': {ex.Message}. Ensure Read/Write enabled.");
            return false;
        }

        int intendedW = wOut;
        int intendedH = hOut;
        if (readW != intendedW || readH != intendedH)
            pixels = ResampleNearest(pixels, readW, readH, intendedW, intendedH);

        mask = new bool[intendedW * intendedH];
        for (int i = 0; i < mask.Length; ++i) mask[i] = pixels[i].a > alphaThreshold;

        return true;
    }

    private static Color[] ResampleNearest(Color[] src, int srcW, int srcH, int dstW, int dstH)
    {
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

    private static bool[] CenterMaskOnCanvas(bool[] src, int srcW, int srcH, int canvasW, int canvasH)
    {
        bool[] canvas = new bool[canvasW * canvasH];
        int offsetX = (canvasW - srcW) / 2;
        int offsetY = (canvasH - srcH) / 2;
        for (int y = 0; y < srcH; ++y)
        {
            int cy = offsetY + y;
            for (int x = 0; x < srcW; ++x)
            {
                int cx = offsetX + x;
                canvas[cy * canvasW + cx] = src[y * srcW + x];
            }
        }
        return canvas;
    }
}
