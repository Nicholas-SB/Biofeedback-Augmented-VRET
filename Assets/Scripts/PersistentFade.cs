using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PersistentFade : MonoBehaviour
{
    public static PersistentFade Instance;
    public CanvasGroup curtainGroup; // Drag the 'Curtain' Panel's CanvasGroup here in Inspector

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Ensure we start transparent
            if(curtainGroup != null) curtainGroup.alpha = 0; 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DoFadeTransition(string sceneName)
    {
        StopAllCoroutines(); // Prevent double-triggering
        StartCoroutine(TransitionRoutine(sceneName));
    }

    IEnumerator TransitionRoutine(string sceneName)
    {
        // 1. FADE TO BLACK (Menu Scene)
        float timer = 0;
        while (timer < 1.5f)
        {
            timer += Time.deltaTime;
            curtainGroup.alpha = timer / 1.5f;
            yield return null;
        }
        curtainGroup.alpha = 1;

        // 2. WAIT (Time for user to put on the Shinebox)
        yield return new WaitForSeconds(5.5f);

        // 3. LOAD THE SCENE (This happens while screen is black)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // Wait until the new scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 4. FADE FROM BLACK (In the Easy/Medium/Hard Scene)
        // Since this script is persistent, it keeps running here!
        yield return new WaitForSeconds(1.0f); // Small buffer for phone to settle
        
        timer = 1.5f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            curtainGroup.alpha = timer / 1.5f;
            yield return null;
        }
        curtainGroup.alpha = 0;
        
        Debug.Log("Gemi: Room Revealed!");
    }
}