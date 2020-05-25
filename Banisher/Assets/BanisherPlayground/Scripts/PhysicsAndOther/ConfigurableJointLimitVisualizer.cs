using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class ConfigurableJointLimitVisualizer : MonoBehaviour {
        [SerializeField] private bool showAnchor = false;
        [SerializeField] private bool showConnectedAnchor = false;

        [SerializeField] private bool showXAxisLimits = false;
        [SerializeField] private bool showYAxisLimits = false;
        [SerializeField] private bool showZAxisLimits = false;

        [SerializeField] private ConfigurableJoint joint;

        [SerializeField] private bool useLimitsMesh = true;
        [SerializeField] private bool showReferenceLimitsMesh = false;
        [SerializeField] private Mesh limitsMesh;
        [SerializeField] private Vector3 limitsMeshOffset;
        [SerializeField] private Vector3 limitsMeshRotation;
        [SerializeField] private Vector3 limitsMeshScale = new Vector3(0.1f, 0.1f, 0.1f);

        private Quaternion connectedAnchorLocalRotation;
        private bool isAwake = false;

        private void Awake() {
            joint = GetComponent<ConfigurableJoint>();

            if (!joint) return;

            Rigidbody connectedBody = joint.connectedBody;
            Vector3 connectedPosition = connectedBody != null ? connectedBody.transform.position : Vector3.zero;
            Quaternion connectedRotation = connectedBody != null ? connectedBody.transform.rotation : Quaternion.identity;

            Quaternion anchorLocalRotation = JointUtils.GetLocalAnchorAxes(joint, out Vector3 anchorLocalRight, out Vector3 anchorLocalForward, out Vector3 anchorLocalUp);
            Quaternion anchorRotation = transform.rotation * anchorLocalRotation;

            connectedAnchorLocalRotation = Quaternion.Inverse(connectedRotation) * anchorRotation;

            isAwake = true;
        }

        private void OnDrawGizmosSelected() {
            ConfigurableJoint joint = this.joint != null ? this.joint : GetComponent<ConfigurableJoint>();
            if (!joint) return;

            Rigidbody connectedBody = joint.connectedBody;
            Vector3 connectedPosition = connectedBody != null ? connectedBody.transform.position : Vector3.zero;
            Quaternion connectedRotation = connectedBody != null ? connectedBody.transform.rotation : Quaternion.identity;
            Vector3 connectedScale = connectedBody != null ? connectedBody.transform.lossyScale : Vector3.zero;

            Quaternion anchorLocalRotation = JointUtils.GetLocalAnchorAxes(joint, out Vector3 anchorLocalRight, out Vector3 anchorLocalForward, out Vector3 anchorLocalUp);
            Quaternion anchorRotation = transform.rotation * anchorLocalRotation;

            Quaternion connectedAnchorLocalRotation = isAwake ? this.connectedAnchorLocalRotation : Quaternion.Inverse(connectedRotation) * anchorRotation;
            Quaternion connectedAnchorRotation = connectedRotation * connectedAnchorLocalRotation;

            Vector3 anchorPosition = transform.TransformPoint(joint.anchor);
            Vector3 connectedAnchorPosition = connectedPosition + connectedRotation * Vector3.Scale(joint.connectedAnchor, connectedScale);

            Quaternion xAxisLocalRotationLowLimit = Quaternion.Euler(-joint.lowAngularXLimit.limit, 0, 0);
            Quaternion xAxisLocalRotationHighLimit = Quaternion.Euler(-joint.highAngularXLimit.limit, 0, 0);

            Quaternion yAxisLocalRotationLowLimit = Quaternion.Euler(0, -joint.angularYLimit.limit, 0);
            Quaternion yAxisLocalRotationHighLimit = Quaternion.Euler(0, joint.angularYLimit.limit, 0);

            Quaternion zAxisLocalRotationLowLimit = Quaternion.Euler(0, 0, -joint.angularZLimit.limit);
            Quaternion zAxisLocalRotationHighLimit = Quaternion.Euler(0, 0, joint.angularZLimit.limit);

            if (showConnectedAnchor) {
                Gizmos.color = new Color(0f, 0f, 1f);
                Gizmos.DrawLine(connectedAnchorPosition, connectedAnchorPosition + connectedAnchorRotation * Vector3.forward * 0.1f);
                Gizmos.color = new Color(0f, 1f, 0f);
                Gizmos.DrawLine(connectedAnchorPosition, connectedAnchorPosition + connectedAnchorRotation * Vector3.up * 0.1f);
                Gizmos.color = new Color(1f, 0f, 0f);
                Gizmos.DrawLine(connectedAnchorPosition, connectedAnchorPosition + connectedAnchorRotation * Vector3.right * 0.1f);
            }

            if (showAnchor) {
                Gizmos.color = new Color(0f, 0f, 1f);
                Gizmos.DrawLine(anchorPosition, anchorPosition + anchorRotation * Vector3.forward * 0.2f);
                Gizmos.color = new Color(0f, 1f, 0f);
                Gizmos.DrawLine(anchorPosition, anchorPosition + anchorRotation * Vector3.up * 0.2f);
                Gizmos.color = new Color(1f, 0f, 0f);
                Gizmos.DrawLine(anchorPosition, anchorPosition + anchorRotation * Vector3.right * 0.2f);
            }

            Quaternion limitsMeshWorldRotation = connectedAnchorRotation;

            if (showReferenceLimitsMesh) {
                Gizmos.color = new Color(1f, 1f, 1f, 0.25f);
                Quaternion meshRotation = limitsMeshWorldRotation * Quaternion.Euler(limitsMeshRotation);
                Gizmos.DrawMesh(limitsMesh, transform.position + meshRotation * Vector3.Scale(limitsMeshOffset, limitsMeshScale), meshRotation, limitsMeshScale);
                Gizmos.color = new Color(1f, 0f, 0f);
                Gizmos.DrawLine(anchorPosition, anchorPosition + meshRotation * Vector3.right * 0.4f);
            }

            if (showXAxisLimits) {
                Gizmos.color = new Color(1f, 1f, 1f, 0.25f);

                if (useLimitsMesh && limitsMesh != null) {
                    Quaternion lowMeshRotation = limitsMeshWorldRotation * xAxisLocalRotationLowLimit * Quaternion.Euler(limitsMeshRotation);
                    Gizmos.DrawMesh(limitsMesh, transform.position + lowMeshRotation * Vector3.Scale(limitsMeshOffset, limitsMeshScale), lowMeshRotation, limitsMeshScale);
                    Quaternion highMeshRotation = limitsMeshWorldRotation * xAxisLocalRotationHighLimit * Quaternion.Euler(limitsMeshRotation);
                    Gizmos.DrawMesh(limitsMesh, transform.position + highMeshRotation * Vector3.Scale(limitsMeshOffset, limitsMeshScale), highMeshRotation, limitsMeshScale);
                } else {
                    Gizmos.DrawLine(transform.position, transform.position + connectedAnchorRotation * xAxisLocalRotationLowLimit * Vector3.forward * 0.1f);
                    Gizmos.DrawLine(transform.position, transform.position + connectedAnchorRotation * xAxisLocalRotationHighLimit * Vector3.forward * 0.1f);
                }
            }

            if (showYAxisLimits) {
                Gizmos.color = new Color(1f, 1f, 1f, 0.25f);

                if (useLimitsMesh && limitsMesh != null) {
                    Quaternion lowMeshRotation = limitsMeshWorldRotation * yAxisLocalRotationLowLimit * Quaternion.Euler(limitsMeshRotation);
                    Gizmos.DrawMesh(limitsMesh, transform.position + lowMeshRotation * Vector3.Scale(limitsMeshOffset, limitsMeshScale), lowMeshRotation, limitsMeshScale);
                    Quaternion highMeshRotation = limitsMeshWorldRotation * yAxisLocalRotationHighLimit * Quaternion.Euler(limitsMeshRotation);
                    Gizmos.DrawMesh(limitsMesh, transform.position + highMeshRotation * Vector3.Scale(limitsMeshOffset, limitsMeshScale), highMeshRotation, limitsMeshScale);
                } else {
                    Gizmos.DrawLine(transform.position, transform.position + connectedAnchorRotation * yAxisLocalRotationLowLimit * Vector3.forward * 0.1f);
                    Gizmos.DrawLine(transform.position, transform.position + connectedAnchorRotation * yAxisLocalRotationHighLimit * Vector3.forward * 0.1f);
                }
            }

            if (showZAxisLimits) {
                Gizmos.color = new Color(1f, 1f, 1f, 0.25f);

                if (useLimitsMesh && limitsMesh != null) {
                    Quaternion lowMeshRotation = limitsMeshWorldRotation * zAxisLocalRotationLowLimit * Quaternion.Euler(limitsMeshRotation);
                    Gizmos.DrawMesh(limitsMesh, transform.position + lowMeshRotation * Vector3.Scale(limitsMeshOffset, limitsMeshScale), lowMeshRotation, limitsMeshScale);
                    Quaternion highMeshRotation = limitsMeshWorldRotation * zAxisLocalRotationHighLimit * Quaternion.Euler(limitsMeshRotation);
                    Gizmos.DrawMesh(limitsMesh, transform.position + highMeshRotation * Vector3.Scale(limitsMeshOffset, limitsMeshScale), highMeshRotation, limitsMeshScale);
                    Gizmos.DrawLine(anchorPosition, anchorPosition + lowMeshRotation * Vector3.forward * 0.4f);
                    Gizmos.DrawLine(anchorPosition, anchorPosition + highMeshRotation * Vector3.forward * 0.4f);
                } else {
                    Gizmos.DrawLine(transform.position, transform.position + connectedAnchorRotation * zAxisLocalRotationLowLimit * Vector3.down * 0.1f);
                    Gizmos.DrawLine(transform.position, transform.position + connectedAnchorRotation * zAxisLocalRotationHighLimit * Vector3.down * 0.1f);
                }
            }
        }
    }
}