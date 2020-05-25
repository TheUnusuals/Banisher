using System.Collections.Generic;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.Sight {
    public class Seeable : MonoBehaviour {
        [SerializeField] private bool scanColliders = true;
        [SerializeField] private List<Collider> seeableColliders;

        [SerializeField] private Transform seeableTransform;

        [SerializeField] private SeeableManager seeableManager;

        public IReadOnlyCollection<Collider> SeeableColliders => seeableColliders;

        public Transform SeeableTransform => seeableTransform;

        private void Awake() {
            if (!seeableTransform) seeableTransform = transform;
            if (!seeableManager) seeableManager = SeeableManager.GetInstance();
        }

        private void Start() {
            if (scanColliders) {
                seeableColliders.AddRange(GetComponentsInChildren<Collider>());
            }
        }

        private void OnEnable() {
            seeableManager.RegisterSeeable(this);
        }

        private void OnDisable() {
            seeableManager.UnregisterSeeable(this);
        }

        private void OnDestroy() {
            seeableManager.UnregisterSeeable(this);
        }
    }
}