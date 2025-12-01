using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private BGMAudioSO bgmSO;
    [SerializeField] private SFXAudioSO sfxSO;

    private void Awake()
    {
        // ΩÃ±€≈Ê √ ±‚»≠
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayBGM(AudioSource audioSource, string key, bool loop = true)
    {
        var audioData = bgmSO.GetAudio(key);
        if (audioData == null) return;

        audioSource.clip = audioData.clip;
        audioSource.volume = audioData.volume;
        audioSource.loop = loop;
        audioSource.Play();
    }

    public void PlaySFX(AudioSource audioSource, string key)
    {
        var audioData = sfxSO.GetAudio(key);
        if (audioData == null) return;

        audioSource.PlayOneShot(audioData.clip, audioData.volume);
    }

}
