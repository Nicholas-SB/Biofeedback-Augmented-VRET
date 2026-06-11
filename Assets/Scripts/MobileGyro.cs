using UnityEngine;
using UnityEngine.InputSystem; // Required for New Input System
using System.Collections;

public class MobileGyro : MonoBehaviour
{
    [Header("VR Settings")]
    [Tooltip("Higher = more responsive, Lower = smoother/heavier feeling")]
    public float smoothing = 20f; 

    [Header("Debug Info (Read Only)")]
    [SerializeField] private bool sensorsActive = false;
    
    private Quaternion gyroOffset = Quaternion.identity;
    private bool initialized = false;

    private void OnEnable()
    {
        // 1. In New Input System, sensors MUST be manually enabled
        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
            sensorsActive = true;
        }
        else
        {
            Debug.LogError("Gemi: No Attitude Sensor found on this device! VR tracking won't work.");
        }

        // Enable the touchscreen for the Recenter tap
        if (Touchscreen.current != null)
        {
            InputSystem.EnableDevice(Touchscreen.current);
        }
    }

    private void OnDisable()
    {
        // 2. Clean up when the app closes or script is disabled
        if (AttitudeSensor.current != null)
        {
            InputSystem.DisableDevice(AttitudeSensor.current);
        }
    }

    private void Start()
    {
        // Force the ROG Phone to use its high-end resolution and refresh rate
        Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, true);
        Application.targetFrameRate = 120; // Silky smooth for ROG hardware

        // Ensure the parent (XR Origin) isn't carrying a weird rotation
        if (transform.parent != null)
        {
            transform.parent.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        // 3. Safety check: Exit if sensor isn't ready or reporting data yet
        if (AttitudeSensor.current == null || !AttitudeSensor.current.enabled) return;

        // Read the raw attitude
        Quaternion rawGyro = AttitudeSensor.current.attitude.ReadValue();

        // 4. THE LANDSCAPE FIX: Map phone axes to Unity's world space
        // This stops the 'tilting when turning' issue in the Shinebox
        Quaternion convertedGyro = new Quaternion(rawGyro.x, rawGyro.y, -rawGyro.z, -rawGyro.w);
        
        // Apply 90-degree pitch so 'Forward' is the horizon, not the floor
        Quaternion finalRaw = Quaternion.Euler(90f, 0f, 0f) * convertedGyro;

        // 5. RECENTER LOGIC: Check for the first frame or a screen tap
        bool screenTapped = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        
        if (!initialized || screenTapped) 
        {
            Recenter(finalRaw);
            initialized = true;
        }

        // 6. APPLY ROTATION with Slerp for smoothing
        Quaternion targetRotation = gyroOffset * finalRaw;
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation, 
            targetRotation, 
            Time.deltaTime * smoothing
        );
    }

    /// <summary>
    /// Resets the 'Forward' direction to the current heading.
    /// Only affects the Y-axis (Yaw) so the horizon stays level.
    /// </summary>
    public void Recenter(Quaternion currentRotation)
    {
        float currentYaw = currentRotation.eulerAngles.y;
        gyroOffset = Quaternion.Euler(0, -currentYaw, 0);
        Debug.Log("Gemi: VR View Recentered!");
    }
}