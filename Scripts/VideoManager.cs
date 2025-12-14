using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class VideoManager : MonoBehaviour
{
    public VideoPlayer vid;
    public GameObject videoCanvas;
    public GameObject menuCanvas;
    public UnityEvent OnVideoEnd;
    public bool isLevel10;

    void Start()
    {
        if (vid != null)
            vid.loopPointReached += OnVideoFinished;

        if (!isLevel10)
        {
            // LEVEL 1 (or any non-level-10 scene)
            if (videoCanvas != null) videoCanvas.SetActive(true);
            if (menuCanvas != null) menuCanvas.SetActive(false);
            // Ensure the clip is prepared before playing to avoid early loop events
            PrepareAndPlay();
        }
        else
        {
            // LEVEL 10 — do NOT play automatically
            if (videoCanvas != null) videoCanvas.SetActive(false);
            if (menuCanvas != null) menuCanvas.SetActive(true);
        }
    }

    void OnDestroy()
    {
        if (vid != null)
            vid.loopPointReached -= OnVideoFinished;
    }

    private void PrepareAndPlay()
    {
        if (vid == null) return;

        // Ensure the VideoPlayer component and its GameObject are enabled so Prepare() can be called
        if (!vid.gameObject.activeInHierarchy)
            vid.gameObject.SetActive(true);
        if (!vid.enabled)
            vid.enabled = true;

        try
        {
            if (!vid.isPrepared)
            {
                vid.prepareCompleted += OnVideoPrepared;
                vid.Prepare();
            }
            else
            {
                vid.Play();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("VideoManager: exception preparing video: " + ex.Message + ". Falling back to Play().");
            try
            {
                vid.Play();
            }
            catch (System.Exception ex2)
            {
                Debug.LogError("VideoManager: failed to Play() video: " + ex2.Message);
            }
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnVideoPrepared;
        vp.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video finished!");

        if (videoCanvas != null) videoCanvas.SetActive(false);
        if (menuCanvas != null) menuCanvas.SetActive(true);

        OnVideoEnd?.Invoke();

        if (isLevel10)
        {
            // Guard: only load main menu when the video was actually prepared / had content.
            if (vp.isPrepared || vp.frameCount > 0)
            {
                // After final level video → go to Main Menu
                SceneManager.LoadSceneAsync("Main menu");
            }
            else
            {
                Debug.LogWarning("VideoManager: loopPointReached fired but video not prepared; delaying Main Menu load.");
                StartCoroutine(DelayedLoadMainMenu());
            }
        }
    }

    System.Collections.IEnumerator DelayedLoadMainMenu()
    {
        // small realtime delay to allow any pending frames to be processed
        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadSceneAsync("Main menu");
    }

    // CALL THIS ONLY IN LEVEL 10
    public void TriggerLevel10Vid()
    {
        Debug.Log("Triggering Level 10 ending video...");

        // Ensure game time is running
        Time.timeScale = 1f;

        if (videoCanvas != null) videoCanvas.SetActive(true);
        if (menuCanvas != null) menuCanvas.SetActive(false);

        // If VideoPlayer component or its GameObject were disabled in inspector, enable them so Prepare() can run
        if (vid != null)
        {
            if (!vid.gameObject.activeInHierarchy)
                vid.gameObject.SetActive(true);
            if (!vid.enabled)
                vid.enabled = true;
        }

        // Prepare and play - preparation avoids premature loop events or freezes
        PrepareAndPlay();
    }
}
