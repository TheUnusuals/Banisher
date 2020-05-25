using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public static class PhysicsUtils {
        public static Vector3 EstimateAcceleration(Vector3 velocity, Vector3 previousVelocity, float deltaTime) {
            // a=(v-v0)/t
            Vector3 acceleration = (velocity - previousVelocity) / deltaTime;
            return acceleration;
        }

        public static Vector3 AngularAccelerationToTorque(Vector3 angularAcceleration, Quaternion currentRotation, Quaternion inertiaTensorRotation, Vector3 inertiaTensor) {
            Quaternion inertiaTensorTransform = currentRotation * inertiaTensorRotation;
            Vector3 torque = inertiaTensorTransform * Vector3.Scale(inertiaTensor, Quaternion.Inverse(inertiaTensorTransform) * angularAcceleration);
            return torque;
        }

        public static Vector3 TorqueToAngularAcceleration(Vector3 torque, Quaternion currentRotation, Quaternion inertiaTensorRotation, Vector3 inertiaTensor) {
            Quaternion inertiaTensorTransform = currentRotation * inertiaTensorRotation;
            Vector3 rotatedTorque = Quaternion.Inverse(inertiaTensorTransform) * torque;
            Vector3 angularAcceleration = inertiaTensorTransform * new Vector3(
                inertiaTensor.x == 0 ? 0 : rotatedTorque.x / inertiaTensor.x,
                inertiaTensor.y == 0 ? 0 : rotatedTorque.y / inertiaTensor.y,
                inertiaTensor.z == 0 ? 0 : rotatedTorque.z / inertiaTensor.z
            );
            return angularAcceleration;
        }

        public static Vector3 GetTorqueOfForce(Vector3 force, Vector3 forcePoint, Vector3 centerOfMass) {
            Vector3 centerOfMassToForcePoint = forcePoint - centerOfMass;
            Vector3 arm = centerOfMassToForcePoint - Vector3.Project(centerOfMassToForcePoint, force.normalized);
            Vector3 torque = Vector3.Cross(arm, force);
            return torque;
        }
    }
}