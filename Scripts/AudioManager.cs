using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // Lazy Singleton Property (Ensures only one exists and can be accessed easily)
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // FIX: Use FindFirstObjectByType to replace the obsolete FindObjectOfType
                var go = Object.FindFirstObjectByType<AudioManager>(); 
                
                if (go == null)
                {
                    // If no existing manager, create a new GameObject and add the component
                    go = new GameObject("AudioManager").AddComponent<AudioManager>();
                }
                
                _instance = go;
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("SFX Clips (assign in inspector)")]
    public AudioClip sfxJump;
    public AudioClip sfxCoin;
    public AudioClip sfxFall;
    public AudioClip sfxDamage;
    public AudioClip sfxLand;
    public AudioClip sfxPickup;
    public AudioClip sfxHeartPurchase;
    public AudioClip sfxQuestionCorrect;
    public AudioClip sfxQuestionWrong;
    public AudioClip sfxFinish;
    public AudioClip sfxTimesUp;
    public AudioClip sfxUI;
    // ⭐ NEW: Dedicated clip for the Lose UI
    public AudioClip sfxLose; 

    [Header("Settings")]
    public float masterVolume = 1f;
    public float pitchVariance = 0.05f;

    AudioSource musicSource;
    private float savedMusicVolume = 1f;
    private bool musicMuted = false;
    private bool sfxMuted = false;

    void Awake()
    {
        // Check for duplicates
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        // Note: The Singleton logic in the 'get' accessor handles the core instantiation and DontDestroyOnLoad.
        // We set the instance here if the object was already placed in the scene manually.
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Music source initialization
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        savedMusicVolume = musicSource.volume = masterVolume;
    }

    // Play a clip from inspector with optional pitch variance & volume
    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (sfxMuted) return;
        
        // Creates a temporary AudioSource for a true, reliable one-shot
        GameObject tempGO = new GameObject("OneShotSFX");
        tempGO.transform.parent = this.transform; 
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        
        // Configure pitch and volume
        float pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        tempSource.pitch = pitch;
        tempSource.volume = Mathf.Clamp01(masterVolume * volume);
        tempSource.spatialBlend = 0f; // 2D sound
        
        // Play and destroy
        tempSource.PlayOneShot(clip);
        Destroy(tempGO, clip.length + 0.1f);
    }

    public bool IsMusicMuted() => musicMuted;
    public bool IsSfxMuted() => sfxMuted;

    public void SetMusicMuted(bool muted)
    {
        musicMuted = muted;
        if (musicSource != null)
        {
            if (muted)
            {
                savedMusicVolume = musicSource.volume;
                musicSource.volume = 0f;
            }
            else
            {
                musicSource.volume = Mathf.Clamp01(masterVolume);
            }
        }
        // Also attempt to mute/unmute other potential background-music AudioSources in the scene.
        // Heuristics: loop == true && spatialBlend == 0 (2D music), or GameObject name contains common music keywords.
    var allSources = UnityEngine.Object.FindObjectsByType<AudioSource>(UnityEngine.FindObjectsSortMode.None);
        foreach (var src in allSources)
        {
            if (src == musicSource) continue;
            if (src == null) continue;
            // Ignore temporary one-shot sources parented to this AudioManager
            if (src.transform.IsChildOf(this.transform)) continue;

            bool looksLikeMusic = (src.loop && src.spatialBlend < 0.1f);
            if (!looksLikeMusic)
            {
                string n = src.gameObject.name.ToLowerInvariant();
                if (n.Contains("music") || n.Contains("background") || n.Contains("bgm") || n.Contains("ambient"))
                {
                    looksLikeMusic = true;
                }
            }

            if (looksLikeMusic)
            {
                src.mute = muted;
            }
        }
    }

    public void SetSfxMuted(bool muted)
    {
        sfxMuted = muted;
    }

    // Convenience methods for assigned clips
    public void PlayJump() => PlaySfx(sfxJump, 1f);
    public void PlayCoin() => PlaySfx(sfxCoin, 1f);
    public void PlayFall() => PlaySfx(sfxFall, 1f);
    public void PlayDamage() => PlaySfx(sfxDamage, 1f);
    public void PlayLand() => PlaySfx(sfxLand, 1f);
    public void PlayPickup() => PlaySfx(sfxPickup, 1f);
    public void PlayHeartPurchase() => PlaySfx(sfxHeartPurchase, 1f);
    public void PlayQuestionCorrect() => PlaySfx(sfxQuestionCorrect, 1f);
    public void PlayQuestionWrong() => PlaySfx(sfxQuestionWrong, 1f);
    public void PlayFinish() => PlaySfx(sfxFinish, 1f);
    // Play the Time's Up bell (assign a bell clip to `sfxTimesUp` in the inspector)
    public void PlayTimesUp() => PlaySfx(sfxTimesUp, 1f);
    public void PlayUI() => PlaySfx(sfxUI, 1f);
    // ⭐ NEW: Use this method in your LoseUI script!
    public void PlayLoseUI() => PlaySfx(sfxLose, 1f); 

    // Play a 3D one-shot at a world position
    public void PlaySfxAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float spatialBlend = 1f)
    {
        if (clip == null) return;
        
        GameObject temp = new GameObject("OneShot3D");
        temp.transform.position = position;
        AudioSource a = temp.AddComponent<AudioSource>();
        a.clip = clip;
        a.spatialBlend = spatialBlend; // 1 = 3D
        a.spatialize = false;
        a.rolloffMode = AudioRolloffMode.Linear;
        a.Play();
        Destroy(temp, clip.length + 0.1f);
    }
}
