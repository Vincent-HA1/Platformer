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

    //Shift the camera set amount of times for the world
    public void SetCameraPosition(int worldIndex)
    {
        cameraConfinerCollider.position += new Vector3(xOffsetAmount * worldIndex, 0);
        confiner.InvalidateCache();
    }

    private void Start()
    {
        confiner.InvalidateCache();
    }
}
