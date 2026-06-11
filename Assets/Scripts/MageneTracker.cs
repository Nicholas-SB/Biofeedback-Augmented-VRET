using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Android; // Required for manual permission requests on Android 12+
using UnityEngine.SceneManagement; // Required for transitioning to the Menu

public class MageneTracker : MonoBehaviour
{
    // --- SINGLETON & STATUS LOGIC ---
    public static MageneTracker Instance { get; private set; }
    public bool IsConnected { get; private set; } = false;
    public static int CurrentBPM = 0;

    // --- BLE CONFIGURATION ---
    private string _heartRateService = "180D"; 
    private string _heartRateCharacteristic = "2A37"; 
    private string _deviceAddress;
    private bool _isScanning = false;
    private bool _isInitialized = false;

    // Target MAC Address for your specific Magene H803
    private string _targetMacAddress = "EC:A8:57:80:40:E6";

    void Awake()
    {
        // Singleton: Keep this object alive across all scenes (Scan -> Menu -> Easy)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Destroy any duplicates created if the Scan scene is re-entered
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Kick off the permission check (Android only)
        #if UNITY_ANDROID && !UNITY_EDITOR
            StartCoroutine(AskForPermissions());
        #else
            InitializeBluetooth();
        #endif
    }

    private IEnumerator AskForPermissions()
    {
        // Modern Android requires these for Bluetooth LE to function
        string[] permissions = {
            "android.permission.BLUETOOTH_SCAN",
            "android.permission.BLUETOOTH_CONNECT",
            "android.permission.ACCESS_FINE_LOCATION"
        };

        foreach (string p in permissions)
        {
            if (!Permission.HasUserAuthorizedPermission(p))
            {
                Debug.Log($"Gemi: Requesting {p}");
                Permission.RequestUserPermission(p);
                
                float waitTime = 0;
                while (!Permission.HasUserAuthorizedPermission(p) && waitTime < 3.0f)
                {
                    yield return new WaitForSeconds(0.5f);
                    waitTime += 0.5f;
                }
            }
        }

        InitializeBluetooth();
    }

    void InitializeBluetooth()
    {
        if (_isInitialized) return;

        BluetoothLEHardwareInterface.Initialize(true, false, () => {
            Debug.Log("Gemi: Bluetooth Initialized!");
            _isInitialized = true;
            StartScan();
        }, (error) => {
            Debug.LogError("Gemi Error: " + error);
        });
    }

    void StartScan()
    {
        if (_isScanning) return;
        _isScanning = true;
        IsConnected = false;

        Debug.Log("--- Starting Aggressive Scan for: " + _targetMacAddress + " ---");

        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) => {
            if (address.Equals(_targetMacAddress, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("🎯 FOUND TARGET DEVICE! Connecting now...");
                ConfirmMagene(address);
            }
        });

        Invoke("RetryScan", 10f);
    }

    void ConfirmMagene(string address)
    {
        _deviceAddress = address;
        _isScanning = false;
        BluetoothLEHardwareInterface.StopScan();
        CancelInvoke("RetryScan"); 
        ConnectToDevice();
    }

    void RetryScan()
    {
        if (_isScanning && !IsConnected)
        {
            Debug.Log("⚠️ Target not found. Restarting scan...");
            BluetoothLEHardwareInterface.StopScan();
            _isScanning = false;
            StartScan();
        }
    }

    void ConnectToDevice()
    {
        BluetoothLEHardwareInterface.ConnectToPeripheral(_deviceAddress, (name) => {
            Debug.Log("Gemi: Connected to " + name);
            IsConnected = true; 

            // --- THE KEY MOVE: AUTO-TRANSITION TO MENU ---
            // Now that we are connected, we leave the scan scene forever.
            // This prevents Start() from ever being called again.
            SceneManager.LoadScene("MenuScene"); 

        }, null, (address, serviceUUID, characteristicUUID) => {

            if (characteristicUUID.ToUpper().Contains("2A37"))
            {
                Debug.Log("🎯 Found Heart Rate Characteristic! Subscribing...");

                BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(
                    _deviceAddress, 
                    _heartRateService, 
                    _heartRateCharacteristic, 
                    null, 
                    (deviceAddr, charUUID, bytes) => {
                        ParseHeartRate(bytes);
                    }
                );
            }
        }, (disconnectAddr) => {
            Debug.LogWarning("Gemi Warning: Device disconnected. Restarting scan...");
            IsConnected = false;
            _isScanning = false;
            
            // If disconnected, you could optionally force a return to the scan scene:
            // SceneManager.LoadScene("BluetoothScanScene");
            StartScan(); 
        });
    }

    void ParseHeartRate(byte[] data)
    {
        if (data.Length > 1)
        {
            CurrentBPM = data[1]; 
            // Debug.Log("LIVE BPM: " + CurrentBPM);
        }
    }
}