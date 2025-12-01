using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SFXSO", menuName = "AudioSO/SFXSO")]
public class SFXAudioSO : ScriptableObject
{
    [Serializable]
    public class AudioData
    {
        public string key;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public List<AudioData> audioList = new List<AudioData>();

    private Dictionary<string, AudioData> audioDict;

    private void OnEnable()
    {
        audioDict = new Dictionary<string, AudioData>();
        foreach (var audio in audioList)
        {
            if (!audioDict.ContainsKey(audio.key))
                audioDict.Add(audio.key, audio);
        }
    }

    public AudioData GetAudio(string key)
    {
        if (audioDict.TryGetValue(key, out var audioData))
            return audioData;
        Debug.LogWarning($"Audio key not found: {key}");
        return null;
    }
}
