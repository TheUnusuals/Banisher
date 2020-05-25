using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class DepenetrationModifier : MonoBehaviour {
        [SerializeField] private float depenetration = 1;
        [SerializeField] private float depenetrationEffectOffset = 0.1f;
        [SerializeField] private bool constantDepenetration = false;
        [SerializeField] private ForceMode forceMode = ForceMode.Acceleration;

        private new Rigidbody rigidbody;
        private new Collider collider;

        private void Awake() {
            rigidbody = GetComponent<Rigidbody>();
            collider = GetComponent<Collider>();
        }

        private void OnCollisionStay(Collision other) {
            if (other.rigidbody && Physics.ComputePenetration(
                other.collider, other.transform.position, other.transform.rotation,
                collider, transform.position, transform.rotation,
                out Vector3 direction, out float distance) && distance > depenetrationEffectOffset) {
                float amount = constantDepenetration ? 1 : distance;
                other.rigidbody.AddForce(direction * (amount * depenetration), forceMode);
            }
        }
    }
}