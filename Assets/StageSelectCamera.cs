using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSelectCamera : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] Transform cameraConfinerCollider;
    [SerializeField] CinemachineConfiner2D confiner;

    [Header("Camera Settings")]
    [SerializeField] int xOffsetAmount = 30;

    public void ShiftCamera(int direction)
    {
        cameraConfinerCollider.position += new Vector3(xOffsetAmount * direction, 0);
        confiner.InvalidateCache();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
