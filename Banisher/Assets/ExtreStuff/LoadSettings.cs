using UnityEngine;
using UnityEngine.Audio;

namespace TheUnusuals.Banisher.ExtreStuff
{
    public class LoadSettings : MonoBehaviour
    {
        public AudioMixer audioMixer;

        private void Start()
        {
            Load();
        }

        private void Load()
        {
            float musicSlider = PlayerPrefs.GetFloat("musicSlider", 0);
            float fxSlider = PlayerPrefs.GetFloat("fxSlider", 0);
            bool mute = PlayerPrefs.GetInt("mute", 0) != 0;

            audioMixer.SetFloat("musicVolume", musicSlider);
            audioMixer.SetFloat("fxVolume", fxSlider);
            AudioListener.pause = mute;
        }
    }
}