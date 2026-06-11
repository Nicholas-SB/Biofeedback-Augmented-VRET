using UnityEngine;

public class GyroCamera : MonoBehaviour
{
    private bool _gyroEnabled;
    private Gyroscope _gyro;
    private GameObject _cameraContainer;
    private Quaternion _rotation;

    void Start()
    {
        _cameraContainer = new GameObject("Camera Container");
        _cameraContainer.transform.position = transform.position;
        transform.SetParent(_cameraContainer.transform);

        _gyroEnabled = EnableGyro();
    }

    bool EnableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            _gyro = Input.gyro;
            _gyro.enabled = true;

            _cameraContainer.transform.rotation = Quaternion.Euler(90f, 90f, 0f);
            _rotation = new Quaternion(0, 0, 1, 0);

            return true;
        }
        return false;
    }

    void Update()
    {
        if (_gyroEnabled)
        {
            transform.localRotation = _gyro.attitude * _rotation;
        }
    }
}