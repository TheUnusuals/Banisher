using System.Collections.Generic;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class RagdollCollider : MonoBehaviour {
        [SerializeField] private MonoBehaviour body;
        public bool isTouching = false;

        public bool IsTouching => touchingColliders.Count > 0;

        public MonoBehaviour Body {
            get => body;
            set => body = value;
        }

        [SerializeField] private List<Collider> touchingColliders = new List<Collider>();

        private void Update() {
            isTouching = IsTouching;
        }

        private void OnCollisionEnter(Collision other) {
            RagdollCollider otherRagdollCollider = other.collider.GetComponent<RagdollCollider>();

            if (!otherRagdollCollider || otherRagdollCollider.Body != body) {
                touchingColliders.Add(other.collider);
            }
        }

        private void OnCollisionExit(Collision other) {
            touchingColliders.Remove(other.collider);
        }
    }
}