using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class RigidbodyVisualizer : MonoBehaviour {
        [SerializeField] private bool showInertia = false;

        [SerializeField] private new Rigidbody rigidbody;

        private void Awake() {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
        }

        private void OnDrawGizmosSelected() {
            Rigidbody rigidbody = this.rigidbody ? this.rigidbody : GetComponent<Rigidbody>();

            Transform transform = rigidbody.transform;
            Vector3 worldCenterOfMass = rigidbody.worldCenterOfMass;

            Vector3 forward = transform.forward;
            Vector3 up = transform.up;
            Vector3 right = transform.right;

            if (showInertia) {
                Vector3 rotatedInertia = rigidbody.inertiaTensorRotation * rigidbody.inertiaTensor;

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(worldCenterOfMass - forward * rotatedInertia.z, worldCenterOfMass + forward * rotatedInertia.z);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(worldCenterOfMass - up * rotatedInertia.y, worldCenterOfMass + up * rotatedInertia.y);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(worldCenterOfMass - right * rotatedInertia.x, worldCenterOfMass + right * rotatedInertia.x);
            }
        }
    }
}