using System.Collections;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class PositionAccelerationController : MonoBehaviour {
        public enum CenterOfMassAnchor {
            None,
            This,
            Other
        }

        [SerializeField] private Rigidbody other;
        [SerializeField] private LinearAccelerationEstimator otherLinearAccelerationEstimator;
        [SerializeField] private AttachedRigidbodies otherAttachedRigidbodies;
        [SerializeField] private Transform otherOffset;
        [SerializeField] private LinearAccelerationEstimator otherParentLinearAccelerationEstimator;

        [SerializeField] private float targetTime = 0.01f;
        [SerializeField] private float maxForce = 100f;

        [SerializeField] private bool useProportionalTime = false;
        [SerializeField] private float minTargetTime = 0.05f;
        [SerializeField] private float maxTargetTime = 0.1f;

        [SerializeField] private float externalLinearAccelerationScale = 1;

        [SerializeField] private CenterOfMassAnchor offsetToCenterOfMass = CenterOfMassAnchor.None;

        [SerializeField] private bool useLocalPosition = false;

        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private LinearAccelerationEstimator linearAccelerationEstimator;
        [SerializeField] private AttachedRigidbodies attachedRigidbodies;
        [SerializeField] private Transform offset;
        [SerializeField] private LinearAccelerationEstimator parentLinearAccelerationEstimator;

        [SerializeField] private bool assignMissingLinearAccelerationEstimatorOnAwake = true;
        [SerializeField] private bool assignMissingAttachedRigidbodiesOnAwake = true;

        private Vector3 previousAppliedAcceleration;
        private Vector3 previousExternalAcceleration;

        private Vector3 previousOtherAppliedAcceleration;
        private Vector3 previousOtherExternalAcceleration;

        private void Awake() {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();

            if (assignMissingLinearAccelerationEstimatorOnAwake) {
                if (!linearAccelerationEstimator) linearAccelerationEstimator = GetComponent<LinearAccelerationEstimator>();
                if (!otherLinearAccelerationEstimator) otherLinearAccelerationEstimator = other.GetComponent<LinearAccelerationEstimator>();
            }

            if (assignMissingAttachedRigidbodiesOnAwake) {
                if (!attachedRigidbodies) attachedRigidbodies = GetComponent<AttachedRigidbodies>();
                if (!otherAttachedRigidbodies) otherAttachedRigidbodies = other.GetComponent<AttachedRigidbodies>();
            }
        }

        private void FixedUpdate() {
            IEnumerator ApplyForcesCoroutine() {
                yield return new WaitForFixedUpdate();
                ApplyForces();
            }

            StartCoroutine(ApplyForcesCoroutine());
            //ApplyForces();
        }

        private void ApplyForces() {
            Transform transform = rigidbody.transform;
            Vector3 position = offset ? offset.position : (offsetToCenterOfMass == CenterOfMassAnchor.Other ? transform.TransformPoint(other.centerOfMass) : rigidbody.worldCenterOfMass);
            Vector3 velocity = rigidbody.velocity;
            bool isKinematic = rigidbody.isKinematic;
            float mass = attachedRigidbodies ? attachedRigidbodies.CalculateCombinedMass(new[] {other}) : rigidbody.mass;

            Transform otherTransform = other.transform;
            Vector3 otherPosition = otherOffset ? otherOffset.position : (offsetToCenterOfMass == CenterOfMassAnchor.This ? otherTransform.TransformPoint(rigidbody.centerOfMass) : other.worldCenterOfMass);
            Vector3 otherVelocity = other.velocity;
            bool otherIsKinematic = other.isKinematic;
            float otherMass = otherAttachedRigidbodies ? otherAttachedRigidbodies.CalculateCombinedMass(new[] {rigidbody}) : other.mass;

            float accelerationRatio = otherIsKinematic ? 1 : isKinematic ? 0 : otherMass / (otherMass + mass);
            float otherAccelerationRatio = 1 - accelerationRatio;

            Vector3 estimatedAcceleration = isKinematic || !linearAccelerationEstimator ? previousAppliedAcceleration : linearAccelerationEstimator.EstimatedPreviousAcceleration;
            Vector3 currentExternalAcceleration = attachedRigidbodies ? attachedRigidbodies.CalculateCombinedJointForces() / mass : Vector3.zero;
            Vector3 estimatedExternalAcceleration = estimatedAcceleration - previousAppliedAcceleration - previousExternalAcceleration + currentExternalAcceleration;

            Vector3 otherEstimatedAcceleration = otherIsKinematic || !otherLinearAccelerationEstimator ? previousOtherAppliedAcceleration : otherLinearAccelerationEstimator.EstimatedPreviousAcceleration;
            Vector3 currentOtherExternalAcceleration = otherAttachedRigidbodies ? otherAttachedRigidbodies.CalculateCombinedJointForces() / otherMass : Vector3.zero;
            Vector3 otherEstimatedExternalAcceleration = otherEstimatedAcceleration - previousOtherAppliedAcceleration - previousOtherExternalAcceleration + currentOtherExternalAcceleration;

            Vector3 targetPosition, targetVelocity, targetAcceleration;
            Vector3 otherTargetPosition, otherTargetVelocity, otherTargetAcceleration;

            if (useLocalPosition) {
                Transform parent = transform.parent;
                Transform otherParent = otherTransform.parent;

                Vector3 targetPositionOffset = otherParent ? otherParent.InverseTransformPoint(otherPosition) : otherPosition;
                targetPosition = parent ? parent.TransformPoint(targetPositionOffset) : targetPositionOffset;
                targetVelocity = parentLinearAccelerationEstimator ? parentLinearAccelerationEstimator.Rigidbody.velocity : velocity;
                targetAcceleration = parentLinearAccelerationEstimator ? parentLinearAccelerationEstimator.EstimatedPreviousAcceleration : estimatedExternalAcceleration;

                Vector3 otherTargetPositionOffset = parent ? parent.InverseTransformPoint(position) : position;
                otherTargetPosition = otherParent ? otherParent.TransformPoint(otherTargetPositionOffset) : otherTargetPositionOffset;
                otherTargetVelocity = otherParentLinearAccelerationEstimator ? otherParentLinearAccelerationEstimator.Rigidbody.velocity : otherVelocity;
                otherTargetAcceleration = otherParentLinearAccelerationEstimator ? otherParentLinearAccelerationEstimator.EstimatedPreviousAcceleration : otherEstimatedExternalAcceleration;
            } else {
                targetPosition = otherPosition;
                targetVelocity = otherVelocity;
                targetAcceleration = otherEstimatedExternalAcceleration;

                otherTargetPosition = position;
                otherTargetVelocity = velocity;
                otherTargetAcceleration = estimatedExternalAcceleration;
            }

            Vector3 appliedForce = GetRequiredForce(position, velocity, estimatedExternalAcceleration, targetPosition, targetVelocity, targetAcceleration, mass, accelerationRatio);
            Vector3 otherAppliedForce = GetRequiredForce(otherPosition, otherVelocity, estimatedExternalAcceleration, otherTargetPosition, otherTargetVelocity, otherTargetAcceleration, otherMass, otherAccelerationRatio);

            rigidbody.AddForce(appliedForce);
            other.AddForce(otherAppliedForce);

            previousAppliedAcceleration = isKinematic ? Vector3.zero : appliedForce / mass;
            previousExternalAcceleration = currentExternalAcceleration;

            previousOtherAppliedAcceleration = otherIsKinematic ? Vector3.zero : otherAppliedForce / otherMass;
            previousOtherExternalAcceleration = currentOtherExternalAcceleration;
        }

        private Vector3 GetRequiredForce(Vector3 position, Vector3 velocity, Vector3 acceleration, Vector3 otherPosition, Vector3 otherVelocity, Vector3 otherAcceleration, float mass, float accelerationRatio) {
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

            Vector3 requiredForce = (requiredAcceleration - externalLinearAccelerationScale * acceleration) * mass;
            Vector3 appliedForce = requiredForce.normalized * Mathf.Min(maxForce * accelerationRatio, requiredForce.magnitude);

            return appliedForce;
        }
    }
}