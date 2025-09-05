using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ParallaxController : MonoBehaviour
{
    [Header("Parallax Attributes")]
    [SerializeField] GameObject cam;
    [SerializeField] float parallaxEffect;

    private float length, startpos;

    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x; 
    }
    
    private void LateUpdate()
    {
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        //If the distance is bigger than the width, move the whole frame one width along
        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;

        // Follow camera y
        transform.position = new Vector3(transform.position.x, cam.transform.position.y, transform.position.z);
    }
}
