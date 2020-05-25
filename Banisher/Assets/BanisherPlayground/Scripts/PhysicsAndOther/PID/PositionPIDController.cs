using UnityEngine;

// ReSharper disable once InconsistentNaming
namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther.PID {
    public class PositionPIDController : MonoBehaviour {
        [SerializeField] private Transform target;

        [SerializeField] private float maxForce = 100;

        [SerializeField] private float kp = 1;
        [SerializeField] private float ki = 0;
        [SerializeField] private float kd = 0;

        [SerializeField] private float integralLimit = 0.1f;

        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private Transform offset;

        [SerializeField] private Vector3 lastError;
        [SerializeField] private Vector3 integral;

        public Transform Target {
            get => target;
            set => target = value;
        }

        public float MaxForce {
            get => maxForce;
            set => maxForce = value;
        }

        private void Awake() {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            if (!target) {
                return;
            }

            Vector3 position = offset ? offset.position : rigidbody.worldCenterOfMass;
            Vector3 targetPosition = target.position;
            Vector3 lastErrorBefore = lastError;

            Vector3 requiredForce = PIDControllerUtils.GetValue(position, targetPosition, kp, ki, kd, Time.fixedDeltaTime, ref lastError, ref integral);
            Vector3 appliedForce = requiredForce.normalized * Mathf.Min(maxForce, requiredForce.magnitude);

            if (requiredForce.magnitude > maxForce) {
                integral = PIDControllerUtils.GetIntegralAfterBackCalculation(appliedForce, position, targetPosition, kp, ki, kd, Time.fixedDeltaTime, lastErrorBefore);
            }

            integral = new Vector3(
                Mathf.Clamp(integral.x, -integralLimit, integralLimit),
                Mathf.Clamp(integral.y, -integralLimit, integralLimit),
                Mathf.Clamp(integral.z, -integralLimit, integralLimit)
            );

            rigidbody.AddForce(appliedForce);
        }
    }
}