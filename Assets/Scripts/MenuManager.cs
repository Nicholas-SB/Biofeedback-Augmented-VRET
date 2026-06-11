using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] difficultyButtons;
    public TextMeshProUGUI statusText;
    public CanvasGroup curtainGroup; // Drag the 'Curtain' CanvasGroup here
    void Update()
    {
        // If the tracker exists and is connected, unlock buttons
        if (MageneTracker.Instance != null && MageneTracker.Instance.IsConnected)
        {
            statusText.text = "Magene H803 Connected! Pick a difficulty.";
            statusText.color = Color.green;
            SetButtonsInteractable(true);
        }
        else
        {
            // This should theoretically only show up if the sensor dies
            statusText.text = "Connection Lost! Please restart.";
            statusText.color = Color.red;
            SetButtonsInteractable(false);
        }
    }

    void SetButtonsInteractable(bool state)
    {
        foreach (var btn in difficultyButtons)
        {
            btn.interactable = state;
        }
    }

    public void StartGame(string sceneName)
    {
        // Simply call the immortal instance
        if (PersistentFade.Instance != null)
        {
            PersistentFade.Instance.DoFadeTransition(sceneName);
        }
        else
        {
            Debug.LogError("Gemi: PersistentFade Instance not found!");
            // Fallback if the fade object is missing
            SceneManager.LoadScene(sceneName);
        }
    }
    IEnumerator TransitionRoutine(string sceneName)
    {
        // 1. FADE TO BLACK
        float timer = 0;
        while (timer < 1.5f)
        {
            timer += Time.deltaTime;
            curtainGroup.alpha = timer / 1.5f; // Goes from 0 to 1
            yield return null;
        }
        curtainGroup.alpha = 1;

        // 2. WAIT FOR HEADSET INSERTION
        yield return new WaitForSeconds(5.5f); // Total 7s including fade
        
        // 3. LOAD THE SCENE
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone) yield return null;

        // 4. FADE FROM BLACK (Reveal the room)
        yield return new WaitForSeconds(1.0f); // Small buffer for the room to load
        timer = 1.5f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            curtainGroup.alpha = timer / 1.5f; // Goes from 1 to 0
            yield return null;
        }
        curtainGroup.alpha = 0;
        
        // Optional: Destroy the curtain once the game starts to save resources
        Destroy(curtainGroup.transform.parent.gameObject);
    }
}