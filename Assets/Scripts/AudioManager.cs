using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{

    [Header("Mixer")]
    public AudioMixer audioMixer;

    [Header("Exposed parameter names")]
    [SerializeField] string masterParam = "MasterVolume";   // name you exposed in the mixer
    [SerializeField] string musicParam = "MusicVolume";
    [SerializeField] string sfxParam = "SFXVolume";

    [Header("UI Volume Bars")]
    [SerializeField] UIBar masterBar;
    [SerializeField] UIBar musicBar;
    [SerializeField] UIBar sfxBar;
    [SerializeField] float defaultPerc;
 
    const string PREF_MASTER = "volume_master";
    const string PREF_MUSIC = "volume_music";
    const string PREF_SFX = "volume_sfx";

    // Start is called before the first frame update
    void Start()
    {
        masterBar.ValueChanged += (float value) => { UpdateValues(PREF_MASTER, masterParam, value); };
        musicBar.ValueChanged += (float value) => { UpdateValues(PREF_MUSIC, musicParam, value); };
        sfxBar.ValueChanged += (float value) => { UpdateValues(PREF_SFX, sfxParam, value); };
        //PlayerPrefs.DeleteKey(PREF_MUSIC);
        //PlayerPrefs.DeleteKey(PREF_SFX);
        float masterValue = PlayerPrefs.GetFloat(PREF_MASTER, defaultPerc);
        float musicValue = PlayerPrefs.GetFloat(PREF_MUSIC, defaultPerc);
        float sfxValue = PlayerPrefs.GetFloat(PREF_SFX, defaultPerc);
        masterBar.SetValue(masterValue);
        musicBar.SetValue(musicValue);
        sfxBar.SetValue(sfxValue);
        //ApplyVolumeToMixer(musicParam, musicValue);
        //ApplyVolumeToMixer(sfxParam, sfxValue);
        UpdateValues(PREF_MASTER, masterParam, masterValue);
        UpdateValues(PREF_MUSIC, musicParam, musicValue);
        UpdateValues(PREF_SFX, sfxParam, sfxValue);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateValues(string prefName, string paramName, float value)
    {
        SaveVolume(prefName, value); 
        ApplyVolumeToMixer(paramName, value);
    }

    void SaveVolume(string prefName, float value)
    {
        PlayerPrefs.SetFloat(prefName, value);
    }

    void ApplyVolumeToMixer(string exposedParam, float linearValue)
    {
        // clamp to avoid log(0)
        float v = Mathf.Clamp(linearValue, 0.0001f, 1f);
        float dB = Mathf.Log10(v) * 20f; // convert to decibels
        audioMixer.SetFloat(exposedParam, dB);
    }

    // Optional: immediate mute toggle
    public void SetMasterMute(bool isMuted)
    {
        if (isMuted) audioMixer.SetFloat(masterParam, -80f);
        else
        {
            float val = PlayerPrefs.GetFloat(PREF_MASTER, 1f);
            ApplyVolumeToMixer(masterParam, val);
        }
    }
}
