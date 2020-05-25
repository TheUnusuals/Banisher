using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class LinearAccelerationEstimator : MonoBehaviour {
        [SerializeField] private Transform offset;

        [SerializeField] private new Rigidbody rigidbody;

        public Vector3 EstimatedPreviousAcceleration { get; private set; }

        public Rigidbody Rigidbody => rigidbody;

        private Vector3 previousVelocity;

        private void Awake() {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            Vector3 position = offset ? offset.position : rigidbody.worldCenterOfMass;
            Vector3 velocity = rigidbody.GetPointVelocity(position);
            EstimatedPreviousAcceleration = Time.fixedDeltaTime != 0 ? PhysicsUtils.EstimateAcceleration(velocity, previousVelocity, Time.fixedDeltaTime) : Vector3.zero;
            previousVelocity = velocity;
        }
    }
}