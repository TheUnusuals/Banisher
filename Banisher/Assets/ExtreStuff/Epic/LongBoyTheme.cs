using System;
using System.Collections;
using TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther;
using TheUnusuals.Banisher.BanisherPlayground.Scripts.Sight;
using UnityEngine;

namespace TheUnusuals.Banisher.ExtreStuff.Epic
{
    public class LongBoyTheme : MonoBehaviour
    {
        [SerializeField] private bool playedOnce = false;

        [SerializeField] private AudioSource epicSoundtrack;

        [SerializeField] private AudioSource levelMusic;

        private void OnTriggerEnter(Collider other)
        {
            if (!playedOnce && (other.GetComponentInParent<Seeable>() || other.GetComponentInChildren<Seeable>() || other.GetComponentInParent<ActiveRagdoll>()))
            {
                float volume = levelMusic.volume;
                levelMusic.volume = 0;
                epicSoundtrack.Play();
                playedOnce = true;
                StartCoroutine(UnpauseLevelMusic(epicSoundtrack.clip.length, volume));
            }
        }

        private IEnumerator UnpauseLevelMusic(float delayInSeconds, float volume)
        {
            yield return new WaitForSeconds(delayInSeconds);
            levelMusic.volume = volume;
        }
    }
}