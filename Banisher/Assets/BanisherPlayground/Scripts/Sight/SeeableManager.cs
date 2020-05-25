using System.Collections.Generic;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Sight {
    public class SeeableManager : MonoBehaviour {
        private const string SeeableManagerTag = "SeeableManager";

        private readonly List<Seeable> currentSeeableObjects = new List<Seeable>();

        public IReadOnlyCollection<Seeable> CurrentSeeableObjects => currentSeeableObjects;

        public void RegisterSeeable(Seeable seeable) {
            currentSeeableObjects.Add(seeable);
        }

        public void UnregisterSeeable(Seeable seeable) {
            currentSeeableObjects.Remove(seeable);
        }

        public static SeeableManager GetInstance() {
            return GameObject
                .FindWithTag(SeeableManagerTag)
                .GetComponent<SeeableManager>();
        }

    }
}