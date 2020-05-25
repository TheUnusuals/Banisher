using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class Banishable : MonoBehaviour {
        public float banishedMassMultiplier = 0.01f;
        public string banishedLayer = "banished";

        public bool isBanished = false;

        [SerializeField] private new Rigidbody rigidbody;
        private float originalMass;
        private int originalLayer;

        private void Awake() {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
        }

        private void Start() {
            if (isBanished) {
                Banish();
            } else {
                ComeBack();
            }
        }

        public void Banish() {
            if (!isBanished) {
                originalMass = rigidbody.mass;
                originalLayer = rigidbody.gameObject.layer;
                rigidbody.mass *= banishedMassMultiplier;
                rigidbody.gameObject.layer = LayerMask.NameToLayer(banishedLayer);

                isBanished = true;
            }
        }

        public void ComeBack() {
            if (isBanished) {
                rigidbody.mass = originalMass;
                rigidbody.gameObject.layer = originalLayer;

                isBanished = false;
            }
        }

        public void ToggleBanishment() {
            if (isBanished) {
                ComeBack();
            } else {
                Banish();
            }
        }
    }
}