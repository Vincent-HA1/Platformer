using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(PixelPerfectCamera))]
public class PixelPerfectCropController : MonoBehaviour
{
    private PixelPerfectCamera pixelPerfectCam;
    private int lastCheckHeight = 0;

    void Awake()
    {
        pixelPerfectCam = GetComponent<PixelPerfectCamera>();
    }

    void Start()
    {
        CheckResolutionAndCrop();
    }

    void Update()
    {
        // Only run the math if the player actually resized their screen
        if (Screen.height != lastCheckHeight)
        {
            CheckResolutionAndCrop();
        }
    }

    void CheckResolutionAndCrop()
    {
        lastCheckHeight = Screen.height;

        // Compare the actual screen height to your component's reference height
        if (Screen.height < pixelPerfectCam.refResolutionY)
        {
            // Screen is smaller than reference -> Turn on Y cropping
            pixelPerfectCam.cropFrameY = true;
        }
        else
        {
            // Screen is equal or larger -> Turn off Y cropping
            pixelPerfectCam.cropFrameY = false;
        }
    }
}