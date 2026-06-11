using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class CameraRecenter : MonoBehaviour
{
    // Drag your "XR Origin" or "Camera Offset" (the parent of the Main Camera) here in the Inspector
    public Transform cameraParent;

    void Start()
    {
        // Give the sensors a tiny moment to initialize
        Invoke("Recenter", 0.1f);
    }

    public void Recenter()
    {
        if (cameraParent == null)
        {
            Debug.LogError("Recenter failed: Please assign the Camera Parent in the Inspector!");
            return;
        }

        // 1. Get the current horizontal rotation (Yaw) of the camera
        float currentYaw = transform.localEulerAngles.y;

        // 2. Rotate the parent in the opposite direction
        // This effectively "zeros out" the view so you are facing forward
        cameraParent.localRotation = Quaternion.Euler(0, -currentYaw, 0);
        
        Debug.Log("View Recentered!");
    }
}