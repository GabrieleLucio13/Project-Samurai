using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    
    [Header("UI Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider ambientSlider;
    [SerializeField] private Slider sfxSlider;
    
    [Header("Default Volumes")]
    [SerializeField] private float defaultMasterVolume = 1f;
    [SerializeField] private float defaultAmbientVolume = 1f;
    [SerializeField] private float defaultSFXVolume = 1f;

    const string MASTER_KEY = "MasterVolume";
    const string AMBIENT_KEY = "AmbientVolume";
    const string SFX_KEY = "SFXVolume";

    void Start()
    {
        SyncSliders();
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
        SetMasterVolume(PlayerPrefs.GetFloat(MASTER_KEY, defaultMasterVolume));
        SetAmbientVolume(PlayerPrefs.GetFloat(AMBIENT_KEY, defaultAmbientVolume));
        SetSFXVolume(PlayerPrefs.GetFloat(SFX_KEY, defaultSFXVolume));
    }

        private void SyncSliders()
    {
        if (masterSlider != null)
            masterSlider.value = PlayerPrefs.GetFloat(MASTER_KEY, defaultMasterVolume);
        if (ambientSlider != null)
            ambientSlider.value = PlayerPrefs.GetFloat(AMBIENT_KEY, defaultAmbientVolume);
        if (sfxSlider != null)
            sfxSlider.value = PlayerPrefs.GetFloat(SFX_KEY, defaultSFXVolume);
    }

}
