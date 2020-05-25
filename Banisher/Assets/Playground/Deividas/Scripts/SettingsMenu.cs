using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider fxSlider;
    public Toggle muteToggle;

    public void LoadSettings() {
        float musicSlider = PlayerPrefs.GetFloat("musicSlider", 0);
        float fxSlider = PlayerPrefs.GetFloat("fxSlider", 0);
        bool mute = PlayerPrefs.GetInt("mute", 0) != 0;
        SetMusicVolume(musicSlider);
        SetSoundFxVolume(fxSlider);
        MuteAudio(mute);
    }

    public void SetMusicVolume(float slider) {
        musicSlider.value = slider;
        audioMixer.SetFloat("musicVolume", slider);
        PlayerPrefs.SetFloat("musicSlider", slider);
        PlayerPrefs.Save();
    }

    public void SetSoundFxVolume(float slider) {
        fxSlider.value = slider;
        audioMixer.SetFloat("fxVolume", slider);
        PlayerPrefs.SetFloat("fxSlider", slider);
        PlayerPrefs.Save();
    }

    public void MuteAudio(bool mute) {
        muteToggle.isOn = mute;
        AudioListener.pause = mute;
        PlayerPrefs.SetInt("mute", mute ? 1 : 0);
        PlayerPrefs.Save();
    }
}