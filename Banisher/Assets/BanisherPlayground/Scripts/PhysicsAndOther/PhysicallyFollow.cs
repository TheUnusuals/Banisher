using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class PhysicallyFollow : MonoBehaviour {
        public enum ProjectionDirection {
            Both = 0,
            Positive = 1,
            Negative = -1
        }

        [SerializeField] private Transform target;
        [SerializeField] private Rigidbody targetRigidbody;

        [SerializeField] private float maxForce = 1000;
        [SerializeField] private float positionTargetSpeed = 1000f;
        [SerializeField] private ForceMode positionForceMode = ForceMode.Force;
        [SerializeField] private bool projectPositionOnVector = false;
        [SerializeField] private Vector3 positionProjectionVector = new Vector3(0, 1, 0);
        [SerializeField] private ProjectionDirection positionProjectionDirection = ProjectionDirection.Both;

        [SerializeField] private float maxTorque = 1000;
        [SerializeField] private float rotationTargetSpeed = 1000f;
        [SerializeField] private ForceMode rotationForceMode = ForceMode.Force;
        [SerializeField] private float maxAngularVelocity = 1000f;

        [SerializeField] private bool includeGravity = true;
        [SerializeField] private bool useLocalRigidbodyPosition = false;
        [SerializeField] private bool useLocalRigidbodyRotation = false;

        [SerializeField] private Transform transformOffset;
        [SerializeField] private new Rigidbody rigidbody;

        public Transform TransformOffset {
            get => transformOffset;
            set => transformOffset = value;
        }

        public Rigidbody Rigidbody {
            get => rigidbody;
            set => rigidbody = value;
        }

        private void Awake() {
            if (!transformOffset) transformOffset = transform;
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            rigidbody.maxAngularVelocity = maxAngularVelocity;
            ApplyForce();
            ApplyTorque();
        }

        private void OnDrawGizmosSelected() {
            if (!target) return;

            Transform transform = transformOffset == null ? this.transform : transformOffset;
            Rigidbody rigidbody = this.rigidbody == null ? GetComponent<Rigidbody>() : this.rigidbody;

            Vector3 worldCenterOfMass = rigidbody.worldCenterOfMass;
            Vector3 currentPosition = worldCenterOfMass;
            Vector3 targetPosition;

            if (useLocalRigidbodyPosition) {
                Transform rigidbodyTransform = rigidbody.transform;
                Vector3 rigidbodyParentPos = rigidbodyTransform.parent == null ? Vector3.zero : rigidbodyTransform.parent.position;
                Quaternion rigidbodyParentRot = rigidbodyTransform.parent == null ? Quaternion.identity : rigidbodyTransform.parent.rotation;
                targetPosition = rigidbodyParentPos + rigidbodyParentRot * (target.localPosition + target.localRotation * rigidbody.centerOfMass);
            } else {
                targetPosition = target.position + target.rotation * Vector3.Scale(transform.InverseTransformPoint(worldCenterOfMass), transform.lossyScale);
            }

            Vector3 forward = useLocalRigidbodyRotation ? rigidbody.transform.localRotation * (rigidbody.transform.parent != null ? rigidbody.transform.parent.forward : Vector3.forward) : transform.forward;
            Vector3 targetForward = useLocalRigidbodyRotation ? target.localRotation * (rigidbody.transform.parent != null ? rigidbody.transform.parent.forward : Vector3.forward) : target.forward;

            Gizmos.color = new Color(1f, 0.01f, 0f, 0.73f);
            Gizmos.DrawSphere(currentPosition, 0.05f);
            Gizmos.DrawLine(currentPosition, currentPosition + forward * 0.1f);
            Gizmos.DrawLine(targetPosition, targetPosition + forward * 0.2f);

            Gizmos.color = new Color(0.06f, 1f, 0f, 0.73f);
            Gizmos.DrawSphere(targetPosition, 0.05f);
            Gizmos.DrawLine(targetPosition, targetPosition + targetForward * 0.1f);
        }

        private void ApplyForce() {
            Vector3 worldCenterOfMass = rigidbody.worldCenterOfMass;
            Vector3 currentPosition = worldCenterOfMass;
            Vector3 targetPosition;

            if (useLocalRigidbodyPosition) {
                Transform rigidbodyTransform = rigidbody.transform;
                Vector3 rigidbodyParentPos = rigidbodyTransform.parent == null ? Vector3.zero : rigidbodyTransform.parent.position;
                Quaternion rigidbodyParentRot = rigidbodyTransform.parent == null ? Quaternion.identity : rigidbodyTransform.parent.rotation;
                targetPosition = rigidbodyParentPos + rigidbodyParentRot * (target.localPosition + target.localRotation * rigidbody.centerOfMass);
            } else {
                targetPosition = target.position + target.rotation * Vector3.Scale(transformOffset.InverseTransformPoint(worldCenterOfMass), transformOffset.lossyScale);
            }

            Vector3 currentVelocity = rigidbody.velocity;
            Vector3 currentAcceleration = Vector3.zero;

            float mass = rigidbody.mass;

            if (includeGravity && rigidbody.useGravity) currentAcceleration += Physics.gravity;

            Vector3 posDiff = targetPosition - currentPosition;

            if (projectPositionOnVector) {
                posDiff = ProjectForPosition(posDiff);
                currentVelocity = ProjectForPosition(currentVelocity);
                currentAcceleration = ProjectForPosition(currentAcceleration);
            }

            float maxAcceleration = maxForce / mass;
            // t=(v-v0)/a
            float timeUntilStopped = maxAcceleration == 0 ? 0 : currentVelocity.magnitude / maxAcceleration;

            float targetTime = Mathf.Max(1f / positionTargetSpeed / posDiff.magnitude, timeUntilStopped, 2 * Time.fixedDeltaTime);
            // a=(h-v0*t)*2/t^2
            // a=(2/t)*(h/t-v0)
            Vector3 requiredAcceleration = 2 / targetTime * (posDiff / targetTime - currentVelocity);
            Vector3 requiredForce = (requiredAcceleration - currentAcceleration) * mass;
            if (projectPositionOnVector) requiredForce = ProjectForPosition(requiredForce);
            Vector3 requiredForceNormalized = requiredForce.normalized;

            rigidbody.AddForce(requiredForceNormalized * Mathf.Min(maxForce, requiredForce.magnitude), positionForceMode);
        }

        private Vector3 ProjectForPosition(Vector3 vector) {
            vector = Vector3.Project(vector, positionProjectionVector);

            if (positionProjectionDirection != ProjectionDirection.Both) {
                float sign = Mathf.Sign(Vector3.Dot(positionProjectionVector, vector));

                if (positionProjectionDirection == ProjectionDirection.Negative && sign > 0 || positionProjectionDirection == ProjectionDirection.Positive && sign < 0) {
                    vector = Vector3.zero;
                }
            }

            return vector;
        }

        private void ApplyTorque() {
            Transform rigidbodyTransform = rigidbody.transform;
            Quaternion currentRotation = useLocalRigidbodyRotation ? rigidbodyTransform.rotation : transformOffset.rotation;
            Transform rigidbodyParent = rigidbodyTransform.parent;
            Quaternion targetRotation = useLocalRigidbodyRotation ? (rigidbodyParent != null ? rigidbodyParent.rotation * target.localRotation : target.localRotation) : target.rotation;

            Vector3 currentAngularVelocity = rigidbody.angularVelocity;
            Quaternion inertiaTensorRotation = rigidbody.inertiaTensorRotation;
            Vector3 inertiaTensor = rigidbody.inertiaTensor;

            Quaternion rotationDiff = targetRotation * Quaternion.Inverse(currentRotation);

            rotationDiff.ToAngleAxis(out float angleDeg, out Vector3 rotationAxis);
            angleDeg = Mathf.DeltaAngle(0, angleDeg); // normalize angle between -180 and 180
            float angleRad = angleDeg * Mathf.Deg2Rad;
            rotationAxis = rotationAxis.normalized;

            Vector3 maxAngularAcceleration = TorqueToAngularAcceleration(rotationAxis * maxTorque, currentRotation, inertiaTensorRotation, inertiaTensor);
            float maxAngularAccelerationMagnitude = maxAngularAcceleration.magnitude;
            // t=(v-v0)/a
            float timeUntilStopped = maxAngularAccelerationMagnitude == 0 ? 0 : currentAngularVelocity.magnitude / maxAngularAccelerationMagnitude;

            Vector3 targetAngularDisplacement = rotationAxis.normalized * angleRad;
            float targetTime = Mathf.Max(1f / rotationTargetSpeed / angleRad, timeUntilStopped);
            // a=(h-v0*t)*2/t^2
            // a=(2/t)*(h/t-v0)
            Vector3 requiredAngularAcceleration = 2 / targetTime * (targetAngularDisplacement / targetTime - currentAngularVelocity);

            Vector3 torqueVector = AngularAccelerationToTorque(requiredAngularAcceleration, currentRotation, inertiaTensorRotation, inertiaTensor);
            Vector3 torqueVectorNormalized = torqueVector.normalized;

            rigidbody.AddTorque(torqueVectorNormalized * Mathf.Min(maxTorque, torqueVector.magnitude), rotationForceMode);
        }

        private static Vector3 AngularAccelerationToTorque(Vector3 angularAcceleration, Quaternion currentRotation, Quaternion inertiaTensorRotation, Vector3 inertiaTensor) {
            Quaternion inertiaTensorTransform = currentRotation * inertiaTensorRotation;
            Vector3 torque = inertiaTensorTransform * Vector3.Scale(inertiaTensor, Quaternion.Inverse(inertiaTensorTransform) * angularAcceleration);
            return torque;
        }

        private static Vector3 TorqueToAngularAcceleration(Vector3 torque, Quaternion currentRotation, Quaternion inertiaTensorRotation, Vector3 inertiaTensor) {
            Quaternion inertiaTensorTransform = currentRotation * inertiaTensorRotation;
            Vector3 rotatedTorque = Quaternion.Inverse(inertiaTensorTransform) * torque;
            Vector3 angularAcceleration = inertiaTensorTransform * new Vector3(
                inertiaTensor.x == 0 ? 0 : rotatedTorque.x / inertiaTensor.x,
                inertiaTensor.y == 0 ? 0 : rotatedTorque.y / inertiaTensor.y,
                inertiaTensor.z == 0 ? 0 : rotatedTorque.z / inertiaTensor.z
            );
            return angularAcceleration;
        }
    }
}