using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundAtRandomIntervals : MonoBehaviour
{
    [Header("Timing Settings")]
    public float minSeconds = 5f; // Minimum interval to wait before playing sound.
    public float maxSeconds = 15f; // Maximum interval to wait before playing sound.

    [Header("Playback Behavior")]
    [Tooltip("If checked, the sound plays once after a random delay and the loop stops.")]
    public bool playOnlyOnce = false;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource != null)
        {
            StartCoroutine(PlaySound());
        }
        else
        {
            Debug.LogError($"No AudioSource component found on {gameObject.name}. Please add one!");
        }
    }

    private IEnumerator PlaySound()
    {
        // We use a while loop that runs indefinitely unless stopped
        while (true)
        {
            float waitTime = Random.Range(minSeconds, maxSeconds);
            yield return new WaitForSeconds(waitTime);

            audioSource.Play();

            // If 'playOnlyOnce' is enabled, we break the loop and end the coroutine here
            if (playOnlyOnce)
            {
                yield break; 
            }
        }
    }
}