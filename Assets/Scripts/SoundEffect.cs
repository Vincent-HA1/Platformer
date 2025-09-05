using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] bool loopingSound = false;


    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = loopingSound;
    }

    // Update is called once per frame
    void Update()
    {
        //If not looping, destroy this object after the clip has ended
        if (!loopingSound)
        {
            if(audioSource.time >= audioSource.clip.length)
            {
                Destroy(gameObject);
            }
        }
    }
}
