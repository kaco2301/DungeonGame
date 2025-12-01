using UnityEngine;

//지역이나 날씨 조절
public class EnvironmentManager : MonoBehaviour
{
    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        AudioManager.Instance.PlayBGM(audioSource, "Forest_Day");
    }
}
