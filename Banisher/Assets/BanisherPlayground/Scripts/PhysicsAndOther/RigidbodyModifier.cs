using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyModifier : MonoBehaviour {
        [SerializeField] private float initialMaxDepenetrationVelocity;

        [SerializeField] private float maxDepenetrationVelocity;

        private new Rigidbody rigidbody;
    
        private void Awake() {
            rigidbody = GetComponent<Rigidbody>();
            initialMaxDepenetrationVelocity = rigidbody.maxDepenetrationVelocity;
        }

        private void Update() {
            rigidbody.maxDepenetrationVelocity = maxDepenetrationVelocity;
        }
    }
}
