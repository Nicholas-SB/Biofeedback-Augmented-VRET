using UnityEngine;

public class GazeFollower : MonoBehaviour
{
    [Header("Targeting")]
    public Transform targetCamera;
    
    [Header("Fine Tuning")]
    public Vector3 offset = new Vector3(0, 0, 0.2f);

    void LateUpdate()
    {
        if (targetCamera == null) return;

        transform.position = targetCamera.TransformPoint(offset);
        transform.rotation = targetCamera.rotation;
    }
}