using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public static class MathUtils {
        public static float DistanceBetweenPointAndLine(Vector3 point, Vector3 linePoint, Vector3 lineDirectionNormalized) {
            /*Vector3 diff = linePoint - point;
        return (diff - Vector3.Dot(diff, lineDirectionNormalized) * lineDirectionNormalized).magnitude;*/
            return Vector3.Cross(lineDirectionNormalized, point - linePoint).magnitude;
        }

        public static void ToAngleAxisSafe(Quaternion quaternion, out float angleDeg, out Vector3 rotationAxis) {
            quaternion.ToAngleAxis(out angleDeg, out rotationAxis);
            if (angleDeg == 0) rotationAxis = Vector3.right;
        }
    }
}