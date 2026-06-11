using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; 
using System.Linq; 
using System.IO; 
using System.Text; 

public class VRET_LevelManager : MonoBehaviour
{
    [Header("Level Config")]
    public float minDuration = 180f; 
    public int requiredCalmBPM = 100; 
    public float calmDurationRequired = 10f; 

    private float _timeElapsed = 0f;
    private float _calmTimer = 0f;
    private bool _levelComplete = false;

    private List<float> _nnIntervalHistory = new List<float>();
    private int _maxBPM = 0; // Added back to track the peak
    private float _startTime;

    void Start()
    {
        _startTime = Time.time;
        InvokeRepeating("RecordBPM", 1f, 1f);
    }

    void RecordBPM()
    {
        if (MageneTracker.Instance != null && MageneTracker.Instance.IsConnected)
        {
            int currentBPM = MageneTracker.CurrentBPM;
            
            if (currentBPM > 0)
            {
                // Track Peak BPM
                if (currentBPM > _maxBPM) _maxBPM = currentBPM;

                // Convert BPM to Milliseconds (NN Interval) for SDNN
                float nnIntervalMs = 60000f / currentBPM;
                _nnIntervalHistory.Add(nnIntervalMs);
            }
        }
    }

    void Update()
    {
        if (_levelComplete) return;

        _timeElapsed += Time.deltaTime;

        if (_timeElapsed >= minDuration)
        {
            if (MageneTracker.Instance.IsConnected && MageneTracker.CurrentBPM <= requiredCalmBPM)
            {
                _calmTimer += Time.deltaTime;
                if (_calmTimer >= calmDurationRequired)
                {
                    StartCoroutine(EndLevelSequence());
                }
            }
            else
            {
                _calmTimer = 0f; 
            }
        }
    }

    IEnumerator EndLevelSequence()
    {
        _levelComplete = true;
        CancelInvoke("RecordBPM");

        float totalTime = Time.time - _startTime;
        float sdnn = CalculateSDNN(_nnIntervalHistory);
        float averageBPM = _nnIntervalHistory.Count > 0 ? 60000f / _nnIntervalHistory.Average() : 0;

        // Pass _maxBPM to the export function
        ExportSessionToTxt(totalTime, averageBPM, _maxBPM, sdnn);

        yield return new WaitForEndOfFrame();

        if (PersistentFade.Instance != null)
        {
            PersistentFade.Instance.DoFadeTransition("MenuScene");
        }
    }

    private float CalculateSDNN(List<float> history)
    {
        if (history.Count < 2) return 0;

        float avgNN = history.Average();
        double sumOfSquares = 0;

        foreach (float interval in history)
        {
            sumOfSquares += Mathf.Pow(interval - avgNN, 2);
        }

        return Mathf.Sqrt((float)(sumOfSquares / (history.Count - 1)));
    }

    private void ExportSessionToTxt(float totalTime, float avgBPM, int maxBPM, float sdnn)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "VRET_Session_Reports.txt");
        
        try
        {
            using (StreamWriter sw = new StreamWriter(filePath, true))
            {
                sw.WriteLine($"--- SESSION REPORT: {System.DateTime.Now} ---");
                sw.WriteLine($"Difficulty: {SceneManager.GetActiveScene().name}");
                sw.WriteLine($"Total Duration: {totalTime:F2}s");
                sw.WriteLine($"Average BPM: {avgBPM:F1}");
                sw.WriteLine($"Peak BPM: {maxBPM}");
                sw.WriteLine($"SDNN: {sdnn:F2} ms");
                sw.WriteLine($"Status: {(sdnn < 50 ? "Stress/Arousal" : "Healthy/Regulated")}");
                sw.WriteLine("-------------------------------------------\n");
                sw.Flush();
            }
            Debug.Log($"Data Saved: SDNN={sdnn:F2}ms");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save Error: {e.Message}");
        }
    }
}