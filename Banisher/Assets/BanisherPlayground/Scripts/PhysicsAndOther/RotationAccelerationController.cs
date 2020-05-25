using System.Collections;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class RotationAccelerationController : MonoBehaviour {
        public enum AffectBodies {
            Both,
            This,
            Other
        }

        [SerializeField] private Rigidbody other;
        [SerializeField] private AngularAccelerationEstimator otherAngularAccelerationEstimator;
        [SerializeField] private AttachedRigidbodies otherAttachedRigidbodies;
        [SerializeField] private Transform otherAnchor;

        [SerializeField] private AffectBodies affectBodies = AffectBodies.Both;

        [SerializeField] private float targetTime = 0.001f;
        [SerializeField] private float maxTorque = 100f;

        [SerializeField] private bool useProportionalTime = true;
        [SerializeField] private float minTargetTime = 0.05f;
        [SerializeField] private float maxTargetTime = 0.1f;

        [SerializeField] private float externalAngularAccelerationScale = 1;
        [SerializeField] private float maxAngularVelocity = 1000;

        [SerializeField] private bool useLocalRotation = false;
        [SerializeField] private Transform otherTargetLocalRotation;
        [SerializeField] private Transform targetLocalRotation;

        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private AngularAccelerationEstimator angularAccelerationEstimator;
        [SerializeField] private AttachedRigidbodies attachedRigidbodies;
        [SerializeField] private Transform anchor;

        [SerializeField] private bool assignMissingAngularAccelerationEstimatorOnAwake = true;
        [SerializeField] private bool assignMissingAttachedRigidbodiesOnAwake = true;

        private Vector3 previousAppliedAngularAcceleration;
        private Vector3 previousExternalAngularAcceleration;

        private Vector3 previousOtherAppliedAngularAcceleration;
        private Vector3 previousOtherExternalAngularAcceleration;

        private bool AffectThis => affectBodies == AffectBodies.Both || affectBodies == AffectBodies.This;
        private bool AffectOther => affectBodies == AffectBodies.Both || affectBodies == AffectBodies.Other;

        private void Awake() {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();

            if (assignMissingAngularAccelerationEstimatorOnAwake) {
                if (!angularAccelerationEstimator) angularAccelerationEstimator = GetComponent<AngularAccelerationEstimator>();
                if (!otherAngularAccelerationEstimator) otherAngularAccelerationEstimator = other.GetComponent<AngularAccelerationEstimator>();
            }

            if (assignMissingAttachedRigidbodiesOnAwake) {
                if (!attachedRigidbodies) attachedRigidbodies = GetComponent<AttachedRigidbodies>();
                if (!otherAttachedRigidbodies) otherAttachedRigidbodies = other.GetComponent<AttachedRigidbodies>();
            }
        }

        private void FixedUpdate() {
            IEnumerator ApplyTorquesCoroutine() {
                yield return new WaitForFixedUpdate();
                ApplyTorques();
            }

            StartCoroutine(ApplyTorquesCoroutine());
        }

        private void ApplyTorques() {
            rigidbody.maxAngularVelocity = maxAngularVelocity;
            other.maxAngularVelocity = maxAngularVelocity;

            Transform transform = rigidbody.transform;
            Quaternion rotation = anchor ? anchor.rotation : transform.rotation;
            Vector3 angularVelocity = rigidbody.angularVelocity;
            bool isKinematic = rigidbody.isKinematic;
            Vector3 inertiaTensor = attachedRigidbodies ? attachedRigidbodies.CalculateCombinedInertiaTensor(new[] {other}) : rigidbody.inertiaTensor;
            Quaternion inertiaTensorRotation = rigidbody.inertiaTensorRotation;
            Quaternion worldInertiaTensorRotation = rotation * inertiaTensorRotation;
            Vector3 rotatedInertiaTensor = Quaternion.Inverse(worldInertiaTensorRotation) * inertiaTensor;

            Transform otherTransform = other.transform;
            Quaternion otherRotation = otherAnchor ? otherAnchor.rotation : otherTransform.rotation;
            Vector3 otherAngularVelocity = other.angularVelocity;
            bool otherIsKinematic = other.isKinematic;
            Vector3 otherInertiaTensor = otherAttachedRigidbodies ? otherAttachedRigidbodies.CalculateCombinedInertiaTensor(new[] {rigidbody}) : other.inertiaTensor;
            Quaternion otherInertiaTensorRotation = other.inertiaTensorRotation;
            Quaternion otherWorldInertiaTensorRotation = otherRotation * otherInertiaTensorRotation;
            Vector3 otherRotatedInertiaTensor = Quaternion.Inverse(otherWorldInertiaTensorRotation) * otherInertiaTensor;

            Quaternion targetRotation, otherTargetRotation;

            if (useLocalRotation) {
                Transform parent = transform.parent;
                Quaternion parentRotation = parent ? parent.rotation : Quaternion.identity;
                Quaternion localRotation = targetLocalRotation ? targetLocalRotation.localRotation : Quaternion.Inverse(parentRotation) * (anchor ? anchor.rotation : transform.rotation);

                Transform otherParent = otherTransform.parent;
                Quaternion otherParentRotation = otherParent ? otherParent.rotation : Quaternion.identity;
                Quaternion otherLocalRotation = otherTargetLocalRotation ? otherTargetLocalRotation.localRotation : Quaternion.Inverse(otherParentRotation) * (otherAnchor ? otherAnchor.rotation : otherTransform.rotation);

                targetRotation = parentRotation * localRotation;
                otherTargetRotation = otherParentRotation * otherLocalRotation;
            } else {
                targetRotation = otherRotation;
                otherTargetRotation = rotation;
            }

            float accelerationRatio = otherIsKinematic || !AffectOther ? 1 : isKinematic || !AffectThis ? 0 : otherRotatedInertiaTensor.magnitude / (otherRotatedInertiaTensor.magnitude + rotatedInertiaTensor.magnitude);
            float otherAccelerationRatio = 1 - accelerationRatio;

            Vector3 estimatedAngularAcceleration = isKinematic || !angularAccelerationEstimator ? previousAppliedAngularAcceleration : angularAccelerationEstimator.EstimatedPreviousAcceleration;
            Vector3 currentExternalAngularAcceleration = attachedRigidbodies ? PhysicsUtils.TorqueToAngularAcceleration(attachedRigidbodies.CalculateCombinedJointTorques(), rotation, inertiaTensorRotation, inertiaTensor) : Vector3.zero;
            Vector3 estimatedExternalAngularAcceleration = estimatedAngularAcceleration - previousAppliedAngularAcceleration - previousExternalAngularAcceleration + currentExternalAngularAcceleration;

            Vector3 otherEstimatedAngularAcceleration = otherIsKinematic || !otherAngularAccelerationEstimator ? previousOtherAppliedAngularAcceleration : otherAngularAccelerationEstimator.EstimatedPreviousAcceleration;
            Vector3 currentOtherExternalAngularAcceleration = otherAttachedRigidbodies ? PhysicsUtils.TorqueToAngularAcceleration(otherAttachedRigidbodies.CalculateCombinedJointTorques(), otherRotation, otherInertiaTensorRotation, otherInertiaTensor) : Vector3.zero;
            Vector3 otherEstimatedExternalAngularAcceleration = otherEstimatedAngularAcceleration - previousOtherAppliedAngularAcceleration - previousOtherExternalAngularAcceleration + currentOtherExternalAngularAcceleration;

            Vector3 appliedTorque = GetRequiredTorque(rotation, angularVelocity, estimatedExternalAngularAcceleration, targetRotation, otherAngularVelocity, otherEstimatedExternalAngularAcceleration, inertiaTensor, inertiaTensorRotation, accelerationRatio);
            Vector3 otherAppliedTorque = GetRequiredTorque(otherRotation, otherAngularVelocity, otherEstimatedExternalAngularAcceleration, otherTargetRotation, angularVelocity, estimatedExternalAngularAcceleration, otherInertiaTensor, otherInertiaTensorRotation, otherAccelerationRatio);

            rigidbody.AddTorque(appliedTorque);
            other.AddTorque(otherAppliedTorque);

            previousAppliedAngularAcceleration = isKinematic ? Vector3.zero : PhysicsUtils.TorqueToAngularAcceleration(appliedTorque, rotation, inertiaTensorRotation, inertiaTensor);
            previousExternalAngularAcceleration = currentExternalAngularAcceleration;

            previousOtherAppliedAngularAcceleration = otherIsKinematic ? Vector3.zero : PhysicsUtils.TorqueToAngularAcceleration(otherAppliedTorque, otherRotation, otherInertiaTensorRotation, otherInertiaTensor);
            previousOtherExternalAngularAcceleration = currentOtherExternalAngularAcceleration;
        }

        private Vector3 GetRequiredTorque(Quaternion rotation, Vector3 angularVelocity, Vector3 angularAcceleration, Quaternion otherRotation, Vector3 otherAngularVelocity, Vector3 otherAngularAcceleration, Vector3 inertiaTensor, Quaternion inertiaTensorRotation, float accelerationRatio) {
            Quaternion rotationDiff = otherRotation * Quaternion.Inverse(rotation);

            MathUtils.ToAngleAxisSafe(rotationDiff, out float angleDeg, out Vector3 rotationAxis);
            angleDeg = Mathf.DeltaAngle(0, angleDeg); // normalize angle between -180 and 180
            float angleRad = angleDeg * Mathf.Deg2Rad;
            rotationAxis = rotationAxis.normalized;

            float maxAngularAcceleration = PhysicsUtils.TorqueToAngularAcceleration(rotationAxis * maxTorque, rotation, inertiaTensorRotation, inertiaTensor).magnitude;

            Vector3 requiredAngularAcceleration = AccelerationControllerUtils.GetRequiredAcceleration(
                Vector3.zero,
                angularVelocity,
                rotationAxis * angleRad,
                otherAngularVelocity,
                otherAngularAcceleration,
                maxAngularAcceleration * accelerationRatio,
                useProportionalTime ? Mathf.Clamp(angleRad == 0 ? 0 : targetTime / angleRad, minTargetTime, maxTargetTime) : targetTime
            );

            Vector3 requiredTorque = PhysicsUtils.AngularAccelerationToTorque(requiredAngularAcceleration - externalAngularAccelerationScale * angularAcceleration, rotation, inertiaTensorRotation, inertiaTensor);
            Vector3 appliedTorque = requiredTorque.normalized * Mathf.Min(maxTorque * accelerationRatio, requiredTorque.magnitude);

            return appliedTorque;
        }
    }
}