using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class PhysicallyFollowPlayground : MonoBehaviour {
        [SerializeField] private Transform target;

        [SerializeField] private float positionFrequency = 10;
        [SerializeField] private float positionDamping = 1;

        [SerializeField] private float rotationFrequency = 10;
        [SerializeField] private float rotationDamping = 1;

        [SerializeField] private float force = 100;
        [SerializeField] private float maxSpeed = 1;
        [SerializeField] private float targetTime = 1;
        [SerializeField] private bool counteractGravity = true;
        [SerializeField] private AnimationCurve stopSmoothingCurve = AnimationCurve.EaseInOut(0, 0, 1, 0.2f);
        [SerializeField] private float damping = 5;

        [SerializeField] private float torque = 1;
        [SerializeField] private float rotationTargetTime = 1;
        [SerializeField] private float maxTorque = 100;
        [SerializeField] private float torqueDamping = 5;

        [SerializeField] private new Rigidbody rigidbody;

        private void Awake() {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            //PositionTest1();
            //PositionTest2();
            //PositionTest3();
            //PositionTest4();
            //PositionTest5();
            //PositionTest6();
            //PositionTest7();
            //PositionTest8();
            //PositionTest9();
            PositionTest10();

            if (counteractGravity) rigidbody.AddForce(-Physics.gravity, ForceMode.Force);

            //rigidbody.AddForce(rigidbody.velocity.normalized * -damping, ForceMode.Acceleration);

            //RotationTest1();
            //RotationTest2();
            //RotationTest3();
            //RotationTest4();
            RotationTest5();
        }

        private void RotationTest1() {
            Quaternion rotDiff = target.rotation * Quaternion.Inverse(rigidbody.rotation);
            Vector3 rotTorque = new Vector3(rotDiff.x, rotDiff.y, rotDiff.z);
            Vector3 rotTorqueNormalized = rotTorque.normalized;
            rigidbody.AddTorque(rotTorqueNormalized * Mathf.Min(rotTorque.magnitude * torque, maxTorque), ForceMode.Acceleration);
            rigidbody.AddTorque(rigidbody.angularVelocity * -torqueDamping, ForceMode.Acceleration);
        }

        private void RotationTest2() {
            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = target.rotation;
            Vector3 inertiaTensor = rigidbody.inertiaTensor;

            Quaternion rotationDiff = targetRotation * Quaternion.Inverse(currentRotation);

            rotationDiff.ToAngleAxis(out float angleDeg, out Vector3 rotationAxis);
            angleDeg = Mathf.DeltaAngle(0, angleDeg); // normalize angle between -180 and 180
            float angleRad = angleDeg * Mathf.Deg2Rad;

            //Vector3 requiredAngularAcceleration = GetRequiredAcceleration();
            Vector3 deltaAngularVelocity = rotationAxis.normalized * (angleRad / rotationTargetTime);

            Debug.Log($"{angleRad} rad {angleDeg} deg {deltaAngularVelocity}");

            Quaternion inertiaTensorTransform = currentRotation * rigidbody.inertiaTensorRotation;
            Vector3 torqueVector = inertiaTensorTransform * Vector3.Scale(inertiaTensor, Quaternion.Inverse(inertiaTensorTransform) * deltaAngularVelocity);

            rigidbody.AddTorque(torqueVector * torque, ForceMode.Force);
        }

        private void RotationTest3() {
            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = target.rotation;

            Vector3 relativeAngularVelocity = rigidbody.angularVelocity;
            Vector3 inertiaTensor = rigidbody.inertiaTensor;

            Quaternion rotationDiff = targetRotation * Quaternion.Inverse(currentRotation);

            rotationDiff.ToAngleAxis(out float angleDeg, out Vector3 rotationAxis);
            angleDeg = Mathf.DeltaAngle(0, angleDeg); // normalize angle between -180 and 180
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 targetAngularDisplacement = rotationAxis.normalized * angleRad;
            Vector3 requiredAngularAcceleration = GetRequiredAcceleration(targetAngularDisplacement, relativeAngularVelocity, rotationTargetTime);
            Vector3 targetAngularVelocity = requiredAngularAcceleration * rotationTargetTime;

            Vector3 requiredAngularVelocity = targetAngularVelocity - relativeAngularVelocity;

            Quaternion inertiaTensorTransform = currentRotation * rigidbody.inertiaTensorRotation;
            Vector3 torqueVector = inertiaTensorTransform * Vector3.Scale(inertiaTensor, Quaternion.Inverse(inertiaTensorTransform) * requiredAngularVelocity);
            Vector3 torqueVectorNormalized = torqueVector.normalized;

            //rigidbody.AddTorque(torqueVectorNormalized * Mathf.Min(torque, torqueVector.magnitude), ForceMode.Force);
            rigidbody.AddTorque(torqueVector, ForceMode.Force);
        }

        private void RotationTest4() {
            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = target.rotation;

            Vector3 currentAngularVelocity = rigidbody.angularVelocity;
            Vector3 inertiaTensor = rigidbody.inertiaTensor;

            Vector3 targetAngularVelocity = Vector3.zero;
            ;

            float kp = 9f * rotationFrequency * rotationFrequency;
            float kd = 4.5f * rotationFrequency * rotationDamping;

            float deltaTime = Time.fixedDeltaTime;

            Quaternion rotationDiff = targetRotation * Quaternion.Inverse(currentRotation);

            rotationDiff.ToAngleAxis(out float angleDeg, out Vector3 rotationAxis);
            angleDeg = Mathf.DeltaAngle(0, angleDeg); // normalize angle between -180 and 180
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 targetAngularDisplacement = rotationAxis.normalized * angleRad;

            Vector3 currentRotationVector = Vector3.zero;
            Vector3 targetRotationVector = targetAngularDisplacement;

            Vector3 requiredAngularVelocity = -kp * (currentRotationVector + currentAngularVelocity * deltaTime - targetRotationVector) - kd * (currentAngularVelocity - targetAngularVelocity);

            Quaternion inertiaTensorTransform = currentRotation * rigidbody.inertiaTensorRotation;
            Vector3 torqueVector = inertiaTensorTransform * Vector3.Scale(inertiaTensor, Quaternion.Inverse(inertiaTensorTransform) * requiredAngularVelocity);
            Vector3 torqueVectorNormalized = torqueVector.normalized;

            rigidbody.AddTorque(torqueVectorNormalized * Mathf.Min(torque, torqueVector.magnitude), ForceMode.Force);
        }

        private void RotationTest5() {
            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = target.rotation;

            Vector3 currentAngularVelocity = rigidbody.angularVelocity;
            Quaternion inertiaTensorRotation = rigidbody.inertiaTensorRotation;
            Vector3 inertiaTensor = rigidbody.inertiaTensor;

            Quaternion rotationDiff = targetRotation * Quaternion.Inverse(currentRotation);

            rotationDiff.ToAngleAxis(out float angleDeg, out Vector3 rotationAxis);
            angleDeg = Mathf.DeltaAngle(0, angleDeg); // normalize angle between -180 and 180
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float timeUntilStopped = GetTimeForVelocityChange3(currentAngularVelocity, Vector3.zero, TorqueToAngularAcceleration(rotationAxis * torque, currentRotation, inertiaTensorRotation, inertiaTensor).magnitude);

            Vector3 targetAngularDisplacement = rotationAxis.normalized * angleRad;
            Vector3 requiredAngularAcceleration = GetRequiredAcceleration(targetAngularDisplacement, currentAngularVelocity, Mathf.Max(rotationTargetTime, timeUntilStopped));

            Vector3 torqueVector = AngularAccelerationToTorque(requiredAngularAcceleration, currentRotation, inertiaTensorRotation, inertiaTensor);
            Vector3 torqueVectorNormalized = torqueVector.normalized;

            rigidbody.AddTorque(torqueVectorNormalized * Mathf.Min(torque, torqueVector.magnitude), ForceMode.Force);
        }

        private static Vector3 AngularAccelerationToTorque(Vector3 angularAcceleration, Quaternion currentRotation, Quaternion inertiaTensorRotation, Vector3 inertiaTensor) {
            Quaternion inertiaTensorTransform = currentRotation * inertiaTensorRotation;
            Vector3 torque = inertiaTensorTransform * Vector3.Scale(inertiaTensor, Quaternion.Inverse(inertiaTensorTransform) * angularAcceleration);
            return torque;
        }

        private static Vector3 TorqueToAngularAcceleration(Vector3 torque, Quaternion currentRotation, Quaternion inertiaTensorRotation, Vector3 inertiaTensor) {
            Quaternion inertiaTensorTransform = currentRotation * inertiaTensorRotation;
            Vector3 rotatedTorque = Quaternion.Inverse(inertiaTensorTransform) * torque;
            Vector3 angularAcceleration = inertiaTensorTransform * new Vector3(rotatedTorque.x / inertiaTensor.x, rotatedTorque.y / inertiaTensor.y, rotatedTorque.z / inertiaTensor.z);
            return angularAcceleration;
        }

        private void PositionTest1() {
            Vector3 relativeVelocity = rigidbody.velocity;

            Vector3 posDiff = target.position - rigidbody.position;
            Vector3 posDiffNormalized = posDiff.normalized;
            float distance = posDiff.magnitude;

            float projectedVelocitySign = Mathf.Sign(Vector3.Dot(relativeVelocity, posDiff));
            Vector3 projectedVelocity = Vector3.Project(relativeVelocity, posDiff) * projectedVelocitySign;
            float projectedVelocityMagnitude = projectedVelocity.magnitude;

            if (projectedVelocityMagnitude * projectedVelocitySign < maxSpeed) {
                Debug.Log($"yes relativeVelocity:{relativeVelocity} {relativeVelocity.magnitude}(m/s) posDiffNormalized:{posDiffNormalized} projectedVelocity:{projectedVelocity} {projectedVelocity.magnitude}(m/s) {((stopSmoothingCurve.Evaluate(distance) + (distance - targetTime * projectedVelocityMagnitude)) * force)}");
                rigidbody.AddForce(posDiffNormalized * ((stopSmoothingCurve.Evaluate(distance) + (distance - targetTime * projectedVelocityMagnitude)) * force), ForceMode.Force);
            } else {
                Debug.Log($"no  relativeVelocity:{relativeVelocity} {relativeVelocity.magnitude}(m/s) posDiffNormalized:{posDiffNormalized} projectedVelocity:{projectedVelocity} {projectedVelocity.magnitude}(m/s)");
            }
        }

        private void PositionTest2() {
            Vector3 relativeVelocity = rigidbody.velocity;

            Vector3 posDiff = target.position - rigidbody.position;
            Vector3 posDiffNormalized = posDiff.normalized;
            float distance = posDiff.magnitude;

            Vector3 targetForceDir = posDiffNormalized * (stopSmoothingCurve.Evaluate(distance) * force) - relativeVelocity;
            Vector3 targetForceDirNormalized = targetForceDir.normalized;

            rigidbody.AddForce(targetForceDir, ForceMode.Force);
        }

        private void PositionTest3() {
            Vector3 relativeVelocity = rigidbody.velocity;

            Vector3 posDiff = target.position - rigidbody.position;
            Vector3 posDiffNormalized = posDiff.normalized;
            float distance = posDiff.magnitude;

            Vector3 targetVelocity = posDiff / Time.fixedDeltaTime;

            Vector3 targetForceDir = targetVelocity - relativeVelocity;
            Vector3 targetForceDirNormalized = targetForceDir.normalized;

            Vector3 maxTargetVelocity = posDiff / 1f;
            Vector3 maxTargetForce = maxTargetVelocity / Time.fixedDeltaTime * rigidbody.mass;
            Vector3 maxTargetForceNormalized = maxTargetForce.normalized;

            Vector3 requiredAcceleration = GetRequiredAcceleration(posDiff, relativeVelocity, targetTime);
            Vector3 requiredForce = (requiredAcceleration - Physics.gravity) * rigidbody.mass;
            Vector3 requiredForceNormalized = requiredForce.normalized;

            //Debug.Log($"{Mathf.Min(force, requiredForce.magnitude):F2} (N) {requiredForce}");
            rigidbody.AddForce(requiredForceNormalized * Mathf.Min(force, requiredForce.magnitude), ForceMode.Force);
            // rigidbody.AddForce(requiredForce, ForceMode.Force);
        }

        private void PositionTest4() {
            Vector3 relativeVelocity = rigidbody.velocity;

            Vector3 posDiff = target.position - rigidbody.position;
            Vector3 posDiffNormalized = posDiff.normalized;
            float distance = posDiff.magnitude;

            Vector3 targetVelocity = posDiff / Time.fixedDeltaTime;

            Vector3 targetForceDir = targetVelocity - relativeVelocity;
            Vector3 targetForceDirNormalized = targetForceDir.normalized;

            float stopAcceleration = GetAccelerationToStop(relativeVelocity, distance);
            float stopForce = stopAcceleration * rigidbody.mass;

            if (stopForce < force + 0.1) {
                Debug.Log($"To   {force} {stopForce} {targetForceDirNormalized * force} {relativeVelocity}");
                rigidbody.AddForce(targetForceDirNormalized * force, ForceMode.Force);
            } else {
                Debug.Log($"From {-force} {stopForce} {targetForceDirNormalized * -force} {relativeVelocity}");
                rigidbody.AddForce(targetForceDirNormalized * -force, ForceMode.Force);
            }
        }

        private void PositionTest5() {
            Vector3 relativeVelocity = rigidbody.velocity;

            Vector3 posDiff = target.position - rigidbody.position;
            Vector3 posDiffNormalized = posDiff.normalized;
            float distance = posDiff.magnitude;

            Vector3 requiredAcceleration = GetRequiredAcceleration(posDiff, relativeVelocity, targetTime);
            Vector3 requiredForce = (requiredAcceleration - Physics.gravity) * rigidbody.mass;
            Vector3 requiredForceNormalized = requiredForce.normalized;

            Vector3 targetVelocity = ForceToVelocity(requiredForce, rigidbody.mass, targetTime);

            Vector3 requiredVelocity = targetVelocity - relativeVelocity;
            Vector3 targetForceDir = VelocityToForce(requiredVelocity, rigidbody.mass, targetTime);
            Vector3 targetForceDirNormalized = targetForceDir.normalized;

            //Debug.Log($"rf:{requiredForce} rv:{relativeVelocity} tv:{targetVelocity} reqv:{requiredVelocity} tfd:{targetForceDir} | {Mathf.Min(force, targetForceDir.magnitude):F2} (N) {targetForceDirNormalized * Mathf.Min(force, targetForceDir.magnitude)}");
            rigidbody.AddForce(targetForceDirNormalized * Mathf.Min(force, targetForceDir.magnitude), ForceMode.Force);
        }

        private void PositionTest6() {
            Vector3 relativeVelocity = rigidbody.velocity;

            Vector3 posDiff = target.position - rigidbody.position;
            Vector3 posDiffNormalized = posDiff.normalized;
            float distance = posDiff.magnitude;

            Vector3 requiredAcceleration = GetAccelerationToStop(relativeVelocity, posDiff);
            Vector3 requiredForce = (requiredAcceleration - Physics.gravity) * rigidbody.mass;
            Vector3 requiredForceNormalized = requiredForce.normalized;

            Vector3 targetVelocity = ForceToVelocity(requiredForce, rigidbody.mass, targetTime);

            Vector3 requiredVelocity = targetVelocity - relativeVelocity;
            Vector3 targetForceDir = VelocityToForce(requiredVelocity, rigidbody.mass, targetTime);
            Vector3 targetForceDirNormalized = targetForceDir.normalized;

            //Debug.Log($"rf:{requiredForce} rv:{relativeVelocity} tv:{targetVelocity} reqv:{requiredVelocity} tfd:{targetForceDir} | {Mathf.Min(force, targetForceDir.magnitude):F2} (N) {targetForceDirNormalized * Mathf.Min(force, targetForceDir.magnitude)}");
            rigidbody.AddForce(targetForceDirNormalized * Mathf.Min(force, targetForceDir.magnitude), ForceMode.Force);
        }

        /**
     * https://digitalopus.ca/site/pd-controllers/
     */
        private void PositionTest7() {
            Vector3 currentPosition = transform.position;
            Vector3 currentVelocity = rigidbody.velocity;

            Vector3 targetPosition = target.position;
            Vector3 targetVelocity = Vector3.zero;

            float kp = (6f * positionFrequency) * (6f * positionFrequency) * 0.25f;
            float kd = 4.5f * positionFrequency * positionDamping;

            float dt = Time.fixedDeltaTime;
            float g = 1 / (1 + kd * dt + kp * dt * dt);
            float ksg = kp * g;
            float kdg = (kd + kp * dt) * g;

            Vector3 requiredForce = (targetPosition - currentPosition) * ksg + (targetVelocity - currentVelocity) * kdg;

            rigidbody.AddForce(requiredForce, ForceMode.Force);
        }

        /**
     * http://www.jie-tan.net/project/spd.pdf
     */
        private void PositionTest8() {
            Vector3 currentPosition = transform.position;
            Vector3 currentVelocity = rigidbody.velocity;

            Vector3 targetPosition = target.position;
            Vector3 targetVelocity = Vector3.zero;

            Vector3 currentAcceleration = Physics.gravity;

            float kp = 9f * positionFrequency * positionFrequency;
            float kd = 4.5f * positionFrequency * positionDamping;

            float deltaTime = Time.fixedDeltaTime;

            Vector3 requiredForce = -kp * (currentPosition + currentVelocity * deltaTime - targetPosition) - kd * (currentVelocity + currentAcceleration * deltaTime - targetVelocity);
            Vector3 requiredForceNormalized = requiredForce.normalized;

            rigidbody.AddForce(requiredForceNormalized * Mathf.Min(force, requiredForce.magnitude), ForceMode.Force);
        }

        private void PositionTest9() {
            Vector3 relativeVelocity = rigidbody.velocity;

            Vector3 posDiff = target.position - rigidbody.position;
            Vector3 posDiffNormalized = posDiff.normalized;
            float distance = posDiff.magnitude;

            Vector3 requiredAcceleration = GetRequiredAcceleration(relativeVelocity, Vector3.zero, posDiff);
            Vector3 requiredForce = (requiredAcceleration - Physics.gravity) * rigidbody.mass;
            Vector3 requiredForceNormalized = requiredForce.normalized;

            Vector3 targetVelocity = ForceToVelocity(requiredForce, rigidbody.mass, targetTime);

            Vector3 requiredVelocity = targetVelocity - relativeVelocity;
            Vector3 targetForceDir = VelocityToForce(requiredVelocity, rigidbody.mass, targetTime);
            Vector3 targetForceDirNormalized = targetForceDir.normalized;

            rigidbody.AddForce(requiredForceNormalized * Mathf.Min(force, requiredForce.magnitude), ForceMode.Force);
        }

        private void PositionTest10() {
            Vector3 currentPosition = rigidbody.position;
            Vector3 targetPosition = target.position;

            Vector3 currentVelocity = rigidbody.velocity;

            Vector3 posDiff = targetPosition - currentPosition;
            Vector3 posDiffNormalized = posDiff.normalized;
            float distance = posDiff.magnitude;

            Vector3 currentAcceleration = Physics.gravity;

            float timeUntilStopped = GetTimeForVelocityChange3(currentVelocity, Vector3.zero, force / rigidbody.mass);

            Vector3 requiredAcceleration = GetRequiredAcceleration(posDiff, currentVelocity, Mathf.Max(targetTime, timeUntilStopped));
            Vector3 requiredForce = (requiredAcceleration - currentAcceleration) * rigidbody.mass;
            Vector3 requiredForceNormalized = requiredForce.normalized;

            rigidbody.AddForce(requiredForceNormalized * Mathf.Min(force, requiredForce.magnitude), ForceMode.Force);
        }

        private static Vector3 DisplacementToForce(Vector3 displacement, float time, float mass, float deltaTime) {
            Vector3 velocity = displacement / time;
            Vector3 force = velocity / deltaTime * mass;
            return force;
        }

        private static Vector3 ForceToDisplacement(Vector3 force, float time, float mass, float deltaTime) {
            Vector3 velocity = force / mass * deltaTime;
            Vector3 displacement = velocity * time;
            return displacement;
        }

        private static Vector3 ForceToVelocity(Vector3 force, float mass, float deltaTime) {
            return force / mass * deltaTime;
        }

        private static Vector3 VelocityToForce(Vector3 velocity, float mass, float deltaTime) {
            return velocity / deltaTime * mass;
        }

        // a=(h-v0*t)*2/t^2
        // a=(2/t)*(h/t-v0)
        private static Vector3 GetRequiredAcceleration(Vector3 displacement, Vector3 velocity, float time) {
            return 2 / time * (displacement / time - velocity);
        }

        // t=v0/a
        private static float GetTimeForVelocityStop(Vector3 velocity, float acceleration) {
            return velocity.magnitude / acceleration;
        }

        // t=(v-v0)/a
        private static float GetTimeForVelocityChange(Vector3 currentVelocity, Vector3 targetVelocity, float acceleration) {
            return Mathf.Max(
                Mathf.Abs(targetVelocity.x - currentVelocity.x),
                Mathf.Abs(targetVelocity.y - currentVelocity.y),
                Mathf.Abs(targetVelocity.z - currentVelocity.z)
            ) / acceleration;
        }

        // t=(v-v0)/a
        private static float GetTimeForVelocityChange3(Vector3 currentVelocity, Vector3 targetVelocity, float acceleration) {
            return (targetVelocity - currentVelocity).magnitude / acceleration;
        }

        // t=(v-v0)/a
        private static float GetTimeForVelocityChange2(Vector3 currentVelocity, Vector3 targetVelocity, float acceleration) {
            return Mathf.Abs((targetVelocity.magnitude - currentVelocity.magnitude) / acceleration);
        }

        // h=v0t+at^2/2
        // h=(v0+at/2)*t
        private static Vector3 GetDisplacement(Vector3 velocity, Vector3 acceleration, float time) {
            return (velocity + acceleration * (time / 2)) * time;
        }

        // v^2=v0^2+2ax
        // x=v0^2/2a
        private static float GetDisplacementUntilStopped(Vector3 velocity, float acceleration) {
            return velocity.sqrMagnitude / (2 * acceleration);
        }

        // v^2=v0^2+2ax
        // x=v0^2/2a
        private static Vector3 GetDisplacementUntilStopped(Vector3 velocity, Vector3 acceleration) {
            return new Vector3(
                velocity.x * velocity.x / (2 * acceleration.x),
                velocity.y * velocity.y / (2 * acceleration.y),
                velocity.z * velocity.z / (2 * acceleration.z)
            );
        }

        // v^2=v0^2+2ax
        // a=v0^2/2x
        private static float GetAccelerationToStop(Vector3 velocity, float displacement) {
            return velocity.sqrMagnitude / (2 * displacement);
        }

        // v^2=v0^2+2ax
        // a=v0^2/2x
        private static Vector3 GetAccelerationToStop(Vector3 velocity, Vector3 displacement) {
            return new Vector3(
                velocity.x * velocity.x / (2 * displacement.x),
                velocity.y * velocity.y / (2 * displacement.y),
                velocity.z * velocity.z / (2 * displacement.z)
            );
        }

        // a=(v^2-v0^2)/2x
        private static Vector3 GetRequiredAcceleration(Vector3 currentVelocity, Vector3 targetVelocity, Vector3 displacement) {
            return new Vector3(
                (targetVelocity.x * targetVelocity.x - currentVelocity.x * currentVelocity.x) / (2 * displacement.x),
                (targetVelocity.y * targetVelocity.y - currentVelocity.y * currentVelocity.y) / (2 * displacement.y),
                (targetVelocity.z * targetVelocity.z - currentVelocity.z * currentVelocity.z) / (2 * displacement.z)
            );
        }
    }
}