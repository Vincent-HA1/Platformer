using System.Collections.Generic;
using UnityEngine;

public class GhostPlayer : MonoBehaviour
{
    [Header("Ghost Player References")]
    [SerializeField] GhostRecording recordingToPlay;

    private int currentFrame = 0;
    bool playRecording = false;
    Animator shadowAnim;
    SpriteRenderer shadowSpriteRenderer;
    Rigidbody2D shadowRigidbody;

    public void StartMoving()
    {
        playRecording = true;
    }

    void Start()
    {
        shadowAnim = GetComponent<Animator>();
        shadowSpriteRenderer = GetComponent<SpriteRenderer>();
        shadowRigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (playRecording && recordingToPlay != null && currentFrame < recordingToPlay.frames.Count)
        {
            GhostFrame frame = recordingToPlay.frames[currentFrame];
            shadowRigidbody.MovePosition(frame.pos);
            shadowSpriteRenderer.flipX = !frame.facingRight;

            // Apply recorded floats by name
            for (int i = 0; i < recordingToPlay.floatNames.Count; i++)
            {
                shadowAnim.SetFloat(recordingToPlay.floatNames[i], frame.floatValues[i]);
            }

            // Apply recorded bools by name
            for (int i = 0; i < recordingToPlay.boolNames.Count; i++)
            {
                shadowAnim.SetBool(recordingToPlay.boolNames[i], frame.boolValues[i]);
            }

            currentFrame++;
        }
    }
}