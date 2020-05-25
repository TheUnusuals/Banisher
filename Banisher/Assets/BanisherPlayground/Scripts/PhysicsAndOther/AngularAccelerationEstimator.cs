using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class AngularAccelerationEstimator : MonoBehaviour {
        [SerializeField] private new Rigidbody rigidbody;

        private Vector3 previousAngularVelocity;

        public Vector3 EstimatedPreviousAcceleration { get; private set; }

        private void Awake() {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            Vector3 angularVelocity = rigidbody.angularVelocity;
            EstimatedPreviousAcceleration = Time.fixedDeltaTime != 0 ? PhysicsUtils.EstimateAcceleration(angularVelocity, previousAngularVelocity, Time.fixedDeltaTime) : Vector3.zero;
            previousAngularVelocity = angularVelocity;
        }
    }
}