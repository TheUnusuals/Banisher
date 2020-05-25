using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther.PID {
    // ReSharper disable once InconsistentNaming
    public class RotationPIDController : MonoBehaviour {
        [SerializeField] private Transform target;

        [SerializeField] private bool projectOnAxis = false;
        [SerializeField] private Vector3 projectedAxisUp = Vector3.up;
        [SerializeField] private Vector3 projectedAxisForward = Vector3.forward;

        [SerializeField] private float maxTorque = 100;

        [SerializeField] private float kp = 1;
        [SerializeField] private float ki = 0;
        [SerializeField] private float kd = 0;

        [SerializeField] private float integralLimit = 0.1f;

        [SerializeField] private float maxAngularVelocity = 1000;

        [SerializeField] private new Rigidbody rigidbody;

        [SerializeField] private Vector3 lastError;
        [SerializeField] private Vector3 integral;

        public Transform Target {
            get => target;
            set => target = value;
        }

        public float MaxTorque {
            get => maxTorque;
            set => maxTorque = value;
        }

        private void Awake() {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            if (!target) {
                return;
            }

            rigidbody.maxAngularVelocity = maxAngularVelocity;

            Quaternion rotation = GetProjectedRotation(rigidbody.transform);
            Quaternion targetRotation = GetProjectedRotation(target);
            Quaternion rotationDiff = targetRotation * Quaternion.Inverse(rotation);

            MathUtils.ToAngleAxisSafe(rotationDiff, out float angleDeg, out Vector3 rotationOffset);
            angleDeg = Mathf.DeltaAngle(0, angleDeg); // normalize angle between -180 and 180
            float angleRad = angleDeg * Mathf.Deg2Rad;
            rotationOffset = rotationOffset.normalized * angleRad;

            Vector3 lastErrorBefore = lastError;

            Vector3 requiredTorque = PIDControllerUtils.GetValue(Vector3.zero, rotationOffset, kp, ki, kd, Time.fixedDeltaTime, ref lastError, ref integral);
            Vector3 appliedTorque = requiredTorque.normalized * Mathf.Min(maxTorque, requiredTorque.magnitude);

            if (requiredTorque.magnitude > maxTorque) {
                integral = PIDControllerUtils.GetIntegralAfterBackCalculation(appliedTorque, Vector3.zero, rotationOffset, kp, ki, kd, Time.fixedDeltaTime, lastErrorBefore);
            }

            integral = new Vector3(
                Mathf.Clamp(integral.x, -integralLimit, integralLimit),
                Mathf.Clamp(integral.y, -integralLimit, integralLimit),
                Mathf.Clamp(integral.z, -integralLimit, integralLimit)
            );

            rigidbody.AddTorque(appliedTorque);
        }

        private Quaternion GetProjectedRotation(Transform rotatedTransform) {
            if (!projectOnAxis) {
                return rotatedTransform.rotation;
            }

            Quaternion rotation = rotatedTransform.rotation;

            Vector3 rotatedPerpendicularAxis = rotation * projectedAxisForward.normalized;
            Vector3 projectedPerpendicularAxis = Vector3.ProjectOnPlane(rotatedPerpendicularAxis, projectedAxisUp.normalized);

            Quaternion projectedRotation = Quaternion.LookRotation(projectedPerpendicularAxis.normalized, projectedAxisUp.normalized);

            return projectedRotation;
        }
    }
}