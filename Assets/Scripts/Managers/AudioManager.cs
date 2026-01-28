using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource musicSource;

    const string MASTER_KEY = "MasterVolume";
    const string AMBIENT_KEY = "AmbientVolume";
    const string SFX_KEY = "SFXVolume";

    void Start()
    {
        LoadVolumes();
    }

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat(MASTER_KEY, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(MASTER_KEY, value);
    }

    public void SetAmbientVolume(float value)
    {
        audioMixer.SetFloat(AMBIENT_KEY, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(AMBIENT_KEY, value);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat(SFX_KEY, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(SFX_KEY, value);
    }

    void LoadVolumes()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(MASTER_KEY, 1f));
        SetAmbientVolume(PlayerPrefs.GetFloat(AMBIENT_KEY, 1f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFX_KEY, 1f));
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

}
