using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGhostRecording", menuName = "Ghost System/Recording")]
public class GhostRecording : ScriptableObject
{
    // These lists save the names so the Ghost knows which parameters to set
    public List<string> floatNames;
    public List<string> boolNames;
    public List<GhostFrame> frames;
}

/// <summary>
/// Saves the animation and position per frame to recreate
/// </summary>
[System.Serializable]
public class GhostFrame
{
    public Vector3 pos;
    public float[] floatValues;
    public bool[] boolValues;
    public bool facingRight;
}