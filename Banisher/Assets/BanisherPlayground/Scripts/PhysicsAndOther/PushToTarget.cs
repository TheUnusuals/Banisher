using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class PushToTarget : MonoBehaviour {
        [SerializeField] private Transform target;

        [SerializeField] private float force = 100;
        [SerializeField] private float maxForce = 100;
        [SerializeField] private float damping = 5;

        [SerializeField] private float torque = 100;
        [SerializeField] private float maxTorque = 100;
        [SerializeField] private float torqueDamping = 5;

        [SerializeField] private new Rigidbody rigidbody;

        private void Awake() {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            Vector3 posDiff = target.position - rigidbody.position;
            Vector3 posDiffNormalized = posDiff.normalized;
            rigidbody.AddForce(posDiffNormalized * Mathf.Min(posDiff.magnitude * force, maxForce), ForceMode.Acceleration);
            rigidbody.AddForce(rigidbody.velocity.normalized * -damping, ForceMode.Acceleration);

            Quaternion rotDiff = target.rotation * Quaternion.Inverse(rigidbody.rotation);
            Vector3 rotTorque = new Vector3(rotDiff.x, rotDiff.y, rotDiff.z);
            Vector3 rotTorqueNormalized = rotTorque.normalized;
            rigidbody.AddTorque(rotTorqueNormalized * Mathf.Min(rotTorque.magnitude * torque, maxTorque), ForceMode.Acceleration);
            rigidbody.AddTorque(rigidbody.angularVelocity * -torqueDamping, ForceMode.Acceleration);
        }
    }
}