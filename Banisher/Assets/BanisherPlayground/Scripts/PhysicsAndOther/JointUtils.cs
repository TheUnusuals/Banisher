using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public static class JointUtils {
        public static Quaternion GetLocalAnchorAxes(ConfigurableJoint joint) {
            return GetLocalAnchorAxes(joint, out Vector3 _, out Vector3 _, out Vector3 _);
        }

        public static Quaternion GetLocalAnchorAxes(ConfigurableJoint joint, out Vector3 anchorLocalRight, out Vector3 anchorLocalForward, out Vector3 anchorLocalUp) {
            anchorLocalRight = joint.axis.normalized;
            anchorLocalForward = Vector3.Cross(anchorLocalRight, joint.secondaryAxis).normalized;
            anchorLocalUp = Vector3.Cross(anchorLocalForward, anchorLocalRight).normalized;
            Quaternion anchorLocalRotation = Quaternion.LookRotation(anchorLocalForward, anchorLocalUp);
            return anchorLocalRotation;
        }
    }
}