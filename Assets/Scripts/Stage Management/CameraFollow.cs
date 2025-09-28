using UnityEngine;

//THIS SCRIPT IS NO LONGER USED. CINEMACHINE IS BEING USED INSTEAD
public class CameraFollow : MonoBehaviour
{

    [Header("References")]
    [SerializeField] Transform playerTarget;

    [Header("Camera Attributes")]
    [SerializeField] float maxYRatio = 0.7f;
    [SerializeField] float maxY;
    [SerializeField] float minY;
    [SerializeField] float cameraLeeway = 0.05f;
    [SerializeField] float minCameraThreshold = 5;

    [Header("Pan Settings")]
    [Tooltip("World units per second")]
    [SerializeField] float panSpeed = 2f;


    Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
    }

    //Update is called once per frame
    void LateUpdate()
    {
        float panDirection = SafeFrameCheck();
        //If we need to pan
        if (panDirection != 0)
        {
            //Get the actual position of the ratio
            float maxYRatioYPos = camera.ScreenToWorldPoint(new Vector3(0, maxYRatio, 0)).y;
            float difference = Mathf.Abs(playerTarget.position.y - maxYRatioYPos);//Get the difference between the ratio and the player
            float deltaY = panSpeed * panDirection * Time.deltaTime;
            deltaY = Mathf.Clamp(deltaY, -difference, difference); //Clamping so it never moves more than the y difference (i.e. beyond the ratio)
            float newYPos = Mathf.Clamp(transform.position.y + deltaY, minY, maxY);//Clamp to min and max y
            transform.position = new Vector3(playerTarget.position.x, newYPos, transform.position.z);

        }
        else
        {
            //Still follow horizontal regardless of pan
            transform.position = new Vector3(playerTarget.position.x, transform.position.y, transform.position.z);
        }

    }

    float SafeFrameCheck()
    {

        Vector3 screenPos = camera.WorldToScreenPoint(playerTarget.position);
        float ratio = screenPos.y / camera.pixelHeight;
        if (ratio < maxYRatio - cameraLeeway)// if we're below our safe frame
        {
            //Need to move camera down to fit the player
            return -1;
        }

        if (ratio > maxYRatio + cameraLeeway) // if we're above our safe frame, return false
        {
            //Need to move camera up to fit player
            return 1;
        }


        return 0; // we're inside our safe frame.
    }


}
