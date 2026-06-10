using System.Collections.Generic;
using UnityEngine;

public class SfxManager : MonoBehaviour
{
    private static SfxManager instance;
    private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, float> nextPlayTime = new Dictionary<string, float>();
    private AudioSource audioSource;

    public static void Play(string clipName, float volume = 1f, float cooldown = 0f)
    {
        if (string.IsNullOrEmpty(clipName)) return;
        EnsureInstance();
        instance.PlayInternal(clipName, volume, cooldown);
    }

    private static void EnsureInstance()
    {
        if (instance != null) return;

        GameObject obj = new GameObject("SfxManager");
        instance = obj.AddComponent<SfxManager>();
        DontDestroyOnLoad(obj);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.75f;
    }

    private void PlayInternal(string clipName, float volume, float cooldown)
    {
        if (cooldown > 0f)
        {
            if (nextPlayTime.TryGetValue(clipName, out float nextTime) && Time.unscaledTime < nextTime) return;
            nextPlayTime[clipName] = Time.unscaledTime + cooldown;
        }

        AudioClip clip = LoadClip(clipName);
        if (clip == null) return;

        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    private AudioClip LoadClip(string clipName)
    {
        if (clips.TryGetValue(clipName, out AudioClip cached)) return cached;

        AudioClip clip = Resources.Load<AudioClip>("Sfx/" + clipName);
        if (clip != null)
        {
            clips[clipName] = clip;
        }

        return clip;
    }
}
