using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class UILogic : MonoBehaviour
{
    public Button pausebtn;
    public Button resumebtn;
    public Button quitbtn;
    public Button musicbtn;
    public Button soundbtn;
    public Button unmusicbtn;
    public Button unsoundbtn;
    public GameObject backgroundMusic;
    public GameObject direction;

    public GameObject pauseUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pausebtn.onClick.RemoveAllListeners();
        pausebtn.onClick.AddListener(TogglePause);
        resumebtn.onClick.RemoveAllListeners();
        resumebtn.onClick.AddListener(ResumeGame);
        quitbtn.onClick.RemoveAllListeners();
        quitbtn.onClick.AddListener(QuitGame);

        // Audio buttons
        if (musicbtn != null) musicbtn.onClick.RemoveAllListeners();
        if (unmusicbtn != null) unmusicbtn.onClick.RemoveAllListeners();
        if (soundbtn != null) soundbtn.onClick.RemoveAllListeners();
        if (unsoundbtn != null) unsoundbtn.onClick.RemoveAllListeners();

        if (musicbtn != null) musicbtn.onClick.AddListener(() => ToggleMusic(true));
        if (unmusicbtn != null) unmusicbtn.onClick.AddListener(() => ToggleMusic(false));
        if (soundbtn != null) soundbtn.onClick.AddListener(() => ToggleSound(true));
        if (unsoundbtn != null) unsoundbtn.onClick.AddListener(() => ToggleSound(false));

        // Initialize button visibility to match AudioManager state
        if (AudioManager.Instance != null)
        {
            bool musicIsMuted = AudioManager.Instance.IsMusicMuted();
            bool sfxIsMuted = AudioManager.Instance.IsSfxMuted();
            if (musicbtn != null) musicbtn.gameObject.SetActive(!musicIsMuted);
            if (unmusicbtn != null) unmusicbtn.gameObject.SetActive(musicIsMuted);
            if (soundbtn != null) soundbtn.gameObject.SetActive(!sfxIsMuted);
            if (unsoundbtn != null) unsoundbtn.gameObject.SetActive(sfxIsMuted);
        }

        // Show direction and hide after 5 seconds
        if (direction != null)
        {
            direction.SetActive(true);
            StartCoroutine(HideDirectionAfterDelay(5f));
        }
    }

    private void ToggleMusic(bool mute)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicMuted(mute);
            if (musicbtn != null) musicbtn.gameObject.SetActive(!mute);
            if (unmusicbtn != null) unmusicbtn.gameObject.SetActive(mute);
        }
    }

    private void ToggleSound(bool mute)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxMuted(mute);
            if (soundbtn != null) soundbtn.gameObject.SetActive(!mute);
            if (unsoundbtn != null) unsoundbtn.gameObject.SetActive(mute);
        }
    }

    void TogglePause()
    {
        if (Time.timeScale == 1f)
        {
            Time.timeScale = 0f;
            pausebtn.gameObject.SetActive(false);
            pauseUI.SetActive(true);
        }
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        pausebtn.gameObject.SetActive(true);
        pauseUI.SetActive(false);
    }
    void QuitGame()
    {
        SceneManager.LoadScene("Main menu");
    }

    private IEnumerator HideDirectionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (direction != null) direction.SetActive(false);
    }
}
