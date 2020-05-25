using UnityEngine;

// ReSharper disable once InconsistentNaming
namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther.PID {
    public static class PIDControllerUtils {
        public static float GetValue(float currentValue, float targetValue, float kp, float ki, float kd, float deltaTime, ref float lastError, ref float integral) {
            float error = targetValue - currentValue;
            float derivative = (error - lastError) / deltaTime;
            integral += error * deltaTime;
            lastError = error;
            return kp * error + ki * integral + kd * derivative;
        }

        public static Vector3 GetValue(Vector3 currentValue, Vector3 targetValue, float kp, float ki, float kd, float deltaTime, ref Vector3 lastError, ref Vector3 integral) {
            return new Vector3(
                GetValue(currentValue.x, targetValue.x, kp, ki, kd, deltaTime, ref lastError.x, ref integral.x),
                GetValue(currentValue.y, targetValue.y, kp, ki, kd, deltaTime, ref lastError.y, ref integral.y),
                GetValue(currentValue.z, targetValue.z, kp, ki, kd, deltaTime, ref lastError.z, ref integral.z)
            );
        }

        public static float GetIntegralAfterBackCalculation(float value, float currentValue, float targetValue, float kp, float ki, float kd, float deltaTime, float lastError) {
            if (ki == 0) return 0;
            float error = targetValue - currentValue;
            float derivative = (error - lastError) / deltaTime;
            float integral = (value - kp * error - kd * derivative) / ki;
            return integral;
        }

        public static Vector3 GetIntegralAfterBackCalculation(Vector3 value, Vector3 currentValue, Vector3 targetValue, float kp, float ki, float kd, float deltaTime, Vector3 lastError) {
            return new Vector3(
                GetIntegralAfterBackCalculation(value.x, currentValue.x, targetValue.x, kp, ki, kd, deltaTime, lastError.x),
                GetIntegralAfterBackCalculation(value.y, currentValue.y, targetValue.y, kp, ki, kd, deltaTime, lastError.y),
                GetIntegralAfterBackCalculation(value.z, currentValue.z, targetValue.z, kp, ki, kd, deltaTime, lastError.z)
            );
        }
    }
}