using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class ComplexPositionAccelerationController : MonoBehaviour {
        [SerializeField] private Rigidbody other;
        [SerializeField] private LinearAccelerationEstimator otherLinearAccelerationEstimator;
        [SerializeField] private AngularAccelerationEstimator otherAngularAccelerationEstimator;
        [SerializeField] private AttachedRigidbodies otherAttachedRigidbodies;
        [SerializeField] private Transform otherOffset;

        [SerializeField] private float targetTime = 0.1f;
        [SerializeField] private float maxForce = 100f;

        [SerializeField] private bool useProportionalTime = false;
        [SerializeField] private float minTargetTime = 0.05f;
        [SerializeField] private float maxTargetTime = 0.1f;

        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private LinearAccelerationEstimator linearAccelerationEstimator;
        [SerializeField] private AngularAccelerationEstimator angularAccelerationEstimator;
        [SerializeField] private AttachedRigidbodies attachedRigidbodies;
        [SerializeField] private Transform offset;

        private Vector3 previousAppliedAcceleration;
        private Vector3 previousAppliedAngularAcceleration;

        private Vector3 previousOtherAppliedAcceleration;
        private Vector3 previousOtherAppliedAngularAcceleration;

        private void Awake() {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
            if (!linearAccelerationEstimator) linearAccelerationEstimator = GetComponent<LinearAccelerationEstimator>();
            if (!attachedRigidbodies) attachedRigidbodies = GetComponent<AttachedRigidbodies>();

            if (!otherLinearAccelerationEstimator) otherLinearAccelerationEstimator = other.GetComponent<LinearAccelerationEstimator>();
            if (!otherAttachedRigidbodies) otherAttachedRigidbodies = other.GetComponent<AttachedRigidbodies>();
        }

        private void FixedUpdate() {
            Transform transform = rigidbody.transform;
            Vector3 position = offset ? offset.position : transform.position;
            Quaternion rotation = offset ? offset.rotation : transform.rotation;
            Vector3 velocity = rigidbody.velocity;
            bool isKinematic = rigidbody.isKinematic;
            float mass = attachedRigidbodies ? attachedRigidbodies.CombinedMass : rigidbody.mass;
            Quaternion inertiaTensorRotation = rigidbody.inertiaTensorRotation;
            Vector3 inertiaTensor = attachedRigidbodies ? attachedRigidbodies.CombinedInertiaTensor : rigidbody.inertiaTensor;

            Transform otherTransform = other.transform;
            Vector3 otherPosition = otherOffset ? otherOffset.position : otherTransform.position;
            Quaternion otherRotation = offset ? otherOffset.rotation : otherTransform.rotation;
            Vector3 otherVelocity = other.velocity;
            bool otherIsKinematic = other.isKinematic;
            float otherMass = otherAttachedRigidbodies ? otherAttachedRigidbodies.CombinedMass : other.mass;
            Quaternion otherInertiaTensorRotation = other.inertiaTensorRotation;
            Vector3 otherInertiaTensor = otherAttachedRigidbodies ? otherAttachedRigidbodies.CombinedInertiaTensor : other.inertiaTensor;

            Vector3 estimatedAcceleration = isKinematic || !linearAccelerationEstimator ? previousAppliedAcceleration : linearAccelerationEstimator.EstimatedPreviousAcceleration;
            Vector3 estimatedExternalAcceleration = estimatedAcceleration - previousAppliedAcceleration;
            Vector3 estimatedAngularAcceleration = isKinematic || !angularAccelerationEstimator ? previousAppliedAngularAcceleration : angularAccelerationEstimator.EstimatedPreviousAcceleration;
            Vector3 estimatedExternalAngularAcceleration = estimatedAngularAcceleration - previousAppliedAngularAcceleration;

            Vector3 otherEstimatedAcceleration = otherIsKinematic || !otherLinearAccelerationEstimator ? previousOtherAppliedAcceleration : otherLinearAccelerationEstimator.EstimatedPreviousAcceleration;
            Vector3 otherEstimatedExternalAcceleration = otherEstimatedAcceleration - previousOtherAppliedAcceleration;
            Vector3 otherEstimatedAngularAcceleration = otherIsKinematic || !otherAngularAccelerationEstimator ? previousOtherAppliedAngularAcceleration : otherAngularAccelerationEstimator.EstimatedPreviousAcceleration;
            Vector3 otherEstimatedExternalAngularAcceleration = otherEstimatedAngularAcceleration - previousOtherAppliedAngularAcceleration;

            float accelerationRatio = otherIsKinematic ? 1 : isKinematic ? 0 : otherMass / (otherMass + mass);
            float otherAccelerationRatio = 1 - accelerationRatio;

            Vector3 appliedForce = GetRequiredForce(position, velocity, otherPosition, otherVelocity, otherEstimatedExternalAcceleration, mass, accelerationRatio);
            Vector3 otherAppliedForce = GetRequiredForce(otherPosition, otherVelocity, position, velocity, estimatedExternalAcceleration, otherMass, otherAccelerationRatio);

            rigidbody.AddForce(appliedForce);
            other.AddForce(otherAppliedForce);

            previousAppliedAcceleration = isKinematic ? Vector3.zero : appliedForce / mass;
            previousOtherAppliedAcceleration = otherIsKinematic ? Vector3.zero : otherAppliedForce / otherMass;
        }

        private Vector3 GetRequiredForce(Vector3 position, Vector3 velocity, Vector3 otherPosition, Vector3 otherVelocity, Vector3 otherAcceleration, float mass, float accelerationRatio) {
            float positionDiff = (position - otherPosition).magnitude;

            Vector3 requiredAcceleration = AccelerationControllerUtils.GetRequiredAcceleration(
                position,
                velocity,
                otherPosition,
                otherVelocity,
                otherAcceleration,
                maxForce / mass * accelerationRatio,
                useProportionalTime ? Mathf.Clamp(positionDiff == 0 ? 0 : targetTime / positionDiff, minTargetTime, maxTargetTime) : targetTime
            );

            Vector3 requiredForce = requiredAcceleration * mass;
            Vector3 appliedForce = requiredForce.normalized * Mathf.Min(maxForce * accelerationRatio, requiredForce.magnitude);

            return appliedForce;
        }
    }
}