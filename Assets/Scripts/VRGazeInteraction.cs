using UnityEngine;

public class VRGazeInteraction : MonoBehaviour
{
    public float interactionDistance = 10f;
    public static GameObject GazeTarget; // Accessible by other scripts

    void Update()
    {
        // 1. Create a Ray starting from the camera center pointing forward
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // 2. Perform the Raycast
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // We found something!
            GazeTarget = hit.collider.gameObject;
            
            // Debugging: This draws a red line in your Scene view so you can see the "gaze"
            Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.red);
        }
        else
        {
            // Looking at the empty room/void
            GazeTarget = null;
        }
    }
}