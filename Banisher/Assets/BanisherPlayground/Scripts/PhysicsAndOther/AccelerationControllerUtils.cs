using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public static class AccelerationControllerUtils {
        public static float GetRequiredAcceleration(float value, float velocity, float targetValue, float targetVelocity, float maxAcceleration, float targetTime) {
            float valueDelta = targetValue - value;
            // t=|(v-v0)/a|
            float minTimeToTargetVelocity = maxAcceleration == 0 ? 0 : Mathf.Abs((targetVelocity - velocity) / maxAcceleration);
            float requiredTime = Mathf.Max(targetTime, minTimeToTargetVelocity);
            // a=(h-v0*t)*2/t^2
            // a=(2/t)*(h/t-v0)
            float requiredAcceleration = 2 / requiredTime * (valueDelta / requiredTime - velocity);
            return Mathf.Sign(requiredAcceleration) * Mathf.Min(Mathf.Abs(requiredAcceleration), maxAcceleration);
        }

        public static float GetRequiredAcceleration(float value, float velocity, float targetValue, float targetVelocity, float targetAcceleration, float maxAcceleration, float targetTime) {
            float valueDelta = targetValue - value;
            // t=|(v-v0)/a|
            float maxAccelerationDiff = maxAcceleration - targetAcceleration;
            float minTimeToTargetVelocity = maxAccelerationDiff == 0 ? 0 : Mathf.Abs((targetVelocity - velocity) / maxAccelerationDiff);
            float requiredTime = Mathf.Max(targetTime, minTimeToTargetVelocity);
            // aA=2/t((xB-xA)/t+vB-vA)+aB
            float requiredAcceleration = 2 / requiredTime * (valueDelta / requiredTime + targetVelocity - velocity) + targetAcceleration;
            return Mathf.Sign(requiredAcceleration) * Mathf.Min(Mathf.Abs(requiredAcceleration), maxAcceleration);
        }

        public static Vector3 GetRequiredAcceleration(Vector3 value, Vector3 velocity, Vector3 targetValue, Vector3 targetVelocity, float maxAcceleration, float targetTime) {
            Vector3 valueDelta = targetValue - value;
            // t=|(v-v0)/a|
            float minTimeToTargetVelocity = maxAcceleration == 0 ? 0 : (targetVelocity - velocity).magnitude / maxAcceleration;
            float requiredTime = Mathf.Max(targetTime, minTimeToTargetVelocity);
            // a=(h-v0*t)*2/t^2
            // a=(2/t)*(h/t-v0)
            Vector3 requiredAcceleration = 2 / requiredTime * (valueDelta / requiredTime - velocity);
            return requiredAcceleration.normalized * Mathf.Min(maxAcceleration, requiredAcceleration.magnitude);
        }

        public static Vector3 GetRequiredAcceleration(Vector3 value, Vector3 velocity, Vector3 targetValue, Vector3 targetVelocity, Vector3 targetAcceleration, float maxAcceleration, float targetTime) {
            Vector3 valueDelta = targetValue - value;
            // t=|(v-v0)/a|
            float maxAccelerationDiff = maxAcceleration - targetAcceleration.magnitude;
            float minTimeToTargetVelocity = maxAccelerationDiff == 0 ? 0 : Mathf.Abs((velocity - targetVelocity).magnitude / maxAccelerationDiff);
            float requiredTime = Mathf.Max(targetTime, minTimeToTargetVelocity);
            // aA=2/t((xB-xA)/t+vB-vA)+aB
            Vector3 requiredAcceleration = 2 / requiredTime * (valueDelta / requiredTime + targetVelocity - velocity) + targetAcceleration;
            return requiredAcceleration.normalized * Mathf.Min(maxAcceleration, requiredAcceleration.magnitude);
        }
    }
}