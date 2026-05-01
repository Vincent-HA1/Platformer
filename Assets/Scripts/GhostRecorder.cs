using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public class GhostRecorder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject playerToRecord;

    [Header("Recording Attributes")]
    [SerializeField] List<GhostFrame> recordedFrames = new List<GhostFrame>();
    [SerializeField] bool isRecording = false;

    Animator anim;
    SpriteRenderer sr;
    // We store the names once to ensure we apply them in the same order
    List<string> floatNames = new List<string>();
    List<string> boolNames = new List<string>();

    void Start()
    {
        anim = playerToRecord.GetComponent<Animator>();
        sr = playerToRecord.GetComponent<SpriteRenderer>();
        //Get parameter names
        foreach (var p in anim.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Float) floatNames.Add(p.name);
            if (p.type == AnimatorControllerParameterType.Bool) boolNames.Add(p.name);
        }
    }

    void FixedUpdate()
    {
        if (isRecording)
        {
            GhostFrame frame = new GhostFrame();
            frame.pos = transform.position;
            frame.facingRight = sr.flipX == false;

            // Capture Animation floats
            frame.floatValues = new float[floatNames.Count];
            for (int i = 0; i < floatNames.Count; i++)
            {
                frame.floatValues[i] = anim.GetFloat(floatNames[i]);
            }

            // Capture Animation bools
            frame.boolValues = new bool[boolNames.Count];
            for (int i = 0; i < boolNames.Count; i++)
            {
                frame.boolValues[i] = anim.GetBool(boolNames[i]);
            }
            recordedFrames.Add(frame); //Save frame
        }
    }

    // Save the list of frames into a scriptable object
    void ExportToAsset()
    {
        GhostRecording newAsset = ScriptableObject.CreateInstance<GhostRecording>();

        // Fill the data
        newAsset.floatNames = new List<string>(floatNames);
        newAsset.boolNames = new List<string>(boolNames);
        newAsset.frames = new List<GhostFrame>(recordedFrames);

        // Save into assets folder
#if UNITY_EDITOR
        string path = "Assets/Recordings/MyRecordedGhost.asset";
        // This checks if the file exists and returns a new name if it does
        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(path);

        AssetDatabase.CreateAsset(newAsset, uniquePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Saved to: " + uniquePath);
#endif
    }

    private void OnApplicationQuit()
    {
        //Export the asset on Quit
        if (recordedFrames.Count > 0)
        {
            ExportToAsset();
        }
    }

}





