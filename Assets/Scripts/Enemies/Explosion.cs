using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    // Run from animation event. Ends the explosion animation
    public void EndExplosion()
    {
        Destroy(gameObject);
    }
}
