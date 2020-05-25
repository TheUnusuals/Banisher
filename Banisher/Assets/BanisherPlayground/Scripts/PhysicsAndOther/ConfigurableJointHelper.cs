using System;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class ConfigurableJointHelper : MonoBehaviour {
        [SerializeField] private Rigidbody connectedBody;

        [SerializeField] private bool adjustTargets = false;
        [SerializeField] private float maxAdjustedDistance = 1f;

        [SerializeField] private bool drivesEnabled = true;

        [SerializeField] private bool positionDriveEnabled = false;
        [SerializeField] private SpringSettings positionDrive = new SpringSettings(1000f, 10f, 100f);
        [SerializeField] private SpringSettings disabledPositionDrive;

        [SerializeField] private bool rotationDriveEnabled = false;
        [SerializeField] private SpringSettings rotationDrive = new SpringSettings(1000f, 10f, 100f);
        [SerializeField] private SpringSettings disabledRotationDrive;

        [SerializeField] private Transform rotationTarget;
        [SerializeField] private Transform rotationTargetConnectedTo;
        [SerializeField] private bool clampTargetRotationToLimits = true;

        [SerializeField] private ConfigurableJoint joint;

        private Quaternion anchorLocalRotation;
        private Quaternion connectedAnchorLocalRotation;

        private Rigidbody previousConnectedBody;

        private bool previousDrivesEnabled;

        private bool previousPositionDriveEnabled;
        private SpringSettings previousPositionDrive;
        private SpringSettings previousDisabledPositionDrive;

        private bool previousRotationDriveEnabled;
        private SpringSettings previousRotationDrive;
        private SpringSettings previousDisabledRotationDrive;

        private bool UsePositionDrive => positionDriveEnabled && drivesEnabled;
        private bool UseRotationDrive => rotationDriveEnabled && drivesEnabled;

        public Rigidbody ConnectedBody {
            get => connectedBody;
            set => connectedBody = value;
        }

        public bool AdjustTargets {
            get => adjustTargets;
            set => adjustTargets = value;
        }

        public float MaxAdjustedDistance {
            get => maxAdjustedDistance;
            set => maxAdjustedDistance = value;
        }

        public bool DrivesEnabled {
            get => drivesEnabled;
            set => drivesEnabled = value;
        }

        public bool PositionDriveEnabled {
            get => positionDriveEnabled;
            set => positionDriveEnabled = value;
        }

        public SpringSettings PositionDrive {
            get => positionDrive;
            set => positionDrive = value;
        }

        public SpringSettings DisabledPositionDrive {
            get => disabledPositionDrive;
            set => disabledPositionDrive = value;
        }

        public bool RotationDriveEnabled {
            get => rotationDriveEnabled;
            set => rotationDriveEnabled = value;
        }

        public SpringSettings RotationDrive {
            get => rotationDrive;
            set => rotationDrive = value;
        }

        public SpringSettings DisabledRotationDrive {
            get => disabledRotationDrive;
            set => disabledRotationDrive = value;
        }

        public Transform RotationTarget {
            get => rotationTarget;
            set => rotationTarget = value;
        }

        public Transform RotationTargetConnectedTo {
            get => rotationTargetConnectedTo;
            set => rotationTargetConnectedTo = value;
        }

        public bool ClampTargetRotationToLimits {
            get => clampTargetRotationToLimits;
            set => clampTargetRotationToLimits = value;
        }

        public ConfigurableJoint Joint {
            get => joint;
            set => joint = value;
        }

        private void Awake() {
            if (!joint) joint = GetComponent<ConfigurableJoint>();
        }

        private void Start() {
            if (!joint) return;

            SaveState();

            previousConnectedBody = joint.connectedBody;

            SetConnectedAxes();
            SetTargetRotation();

            if (UsePositionDrive) {
                SetPositionDrive();
            } else {
                SetDisabledPositionDrive();
            }

            if (UseRotationDrive) {
                SetRotationDrive();
            } else {
                SetDisabledRotationDrive();
            }
        }

        private void FixedUpdate() {
            if (!joint) return;

            if (previousConnectedBody != connectedBody) SetConnectedBody();

            SetTargetRotation();

            if (!previousPositionDrive.Equals(positionDrive) && UsePositionDrive) SetPositionDrive();
            if (!previousDisabledPositionDrive.Equals(disabledPositionDrive) && !UsePositionDrive) SetDisabledPositionDrive();

            if (!previousRotationDrive.Equals(rotationDrive) && UseRotationDrive) SetRotationDrive();
            if (!previousDisabledRotationDrive.Equals(disabledRotationDrive) && !UseRotationDrive) SetDisabledRotationDrive();

            if (previousPositionDriveEnabled != UsePositionDrive) {
                if (UsePositionDrive) {
                    SetPositionDrive();
                } else {
                    SetDisabledPositionDrive();
                }
            }

            if (previousRotationDriveEnabled != UseRotationDrive) {
                if (UseRotationDrive) {
                    SetRotationDrive();
                } else {
                    SetDisabledRotationDrive();
                }
            }

            if (adjustTargets && connectedBody) {
                Vector3 currentPosition = connectedBody.transform.position;
                Vector3 targetPosition = transform.position;

                Vector3 positionDifference = targetPosition - currentPosition;
                Vector3 adjustedTargetPosition = currentPosition + positionDifference.normalized * Mathf.Min(maxAdjustedDistance, positionDifference.magnitude);

                joint.anchor = joint.transform.InverseTransformPoint(adjustedTargetPosition);
            }

            SaveState();
        }

        private void SetConnectedBody() {
            if (connectedBody) {
                Transform connectedTransform = connectedBody.transform;
                Vector3 connectedLocalPosition = connectedTransform.localPosition;
                Quaternion connectedLocalRotation = connectedTransform.localRotation;

                Transform jointTransform = joint.transform;
                connectedTransform.position = jointTransform.position;
                connectedTransform.rotation = jointTransform.rotation;

                joint.connectedBody = connectedBody;

                connectedTransform.localPosition = connectedLocalPosition;
                connectedTransform.localRotation = connectedLocalRotation;
            } else {
                joint.connectedBody = connectedBody;
            }

            SetConnectedAxes();
        }

        private void SetConnectedAxes() {
            anchorLocalRotation = JointUtils.GetLocalAnchorAxes(joint);
            connectedAnchorLocalRotation = Quaternion.Inverse(connectedBody ? connectedBody.rotation : Quaternion.identity) * (transform.rotation * anchorLocalRotation);
        }

        private void SetTargetRotation() {
            if (rotationTarget) {
                Transform rotationTargetParent = rotationTarget.parent;
                Quaternion rotationTargetConnectedToRotation = rotationTargetConnectedTo ? rotationTargetConnectedTo.rotation : rotationTargetParent ? rotationTargetParent.rotation : Quaternion.identity;

                Quaternion targetRotation = Quaternion.Inverse(rotationTarget.rotation * anchorLocalRotation) * (rotationTargetConnectedToRotation * connectedAnchorLocalRotation);

                if (clampTargetRotationToLimits) {
                    Vector3 targetRotationEulerAngles = targetRotation.eulerAngles;
                    SoftJointLimit angularYLimit = joint.angularYLimit;
                    SoftJointLimit angularZLimit = joint.angularZLimit;

                    targetRotation.eulerAngles = new Vector3(
                        Mathf.Clamp(Mathf.DeltaAngle(0, targetRotationEulerAngles.x), joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit),
                        Mathf.Clamp(Mathf.DeltaAngle(0, targetRotationEulerAngles.y), -angularYLimit.limit, angularYLimit.limit),
                        Mathf.Clamp(Mathf.DeltaAngle(0, targetRotationEulerAngles.z), -angularZLimit.limit, angularZLimit.limit)
                    );
                }

                joint.targetRotation = targetRotation;
            } else {
                joint.targetRotation = Quaternion.identity;
            }
        }

        private void SaveState() {
            previousPositionDriveEnabled = UsePositionDrive;
            previousRotationDriveEnabled = UseRotationDrive;

            previousPositionDrive = positionDrive;
            previousDisabledPositionDrive = disabledPositionDrive;

            previousRotationDrive = rotationDrive;
            previousDisabledRotationDrive = disabledRotationDrive;
        }

        private void SetPositionDrive() {
            joint.xDrive = positionDrive;
            joint.yDrive = positionDrive;
            joint.zDrive = positionDrive;
        }

        private void SetDisabledPositionDrive() {
            joint.xDrive = disabledPositionDrive;
            joint.yDrive = disabledPositionDrive;
            joint.zDrive = disabledPositionDrive;
        }

        private void SetRotationDrive() {
            joint.angularXDrive = rotationDrive;
            joint.angularYZDrive = rotationDrive;
            joint.slerpDrive = rotationDrive;
        }

        private void SetDisabledRotationDrive() {
            joint.angularXDrive = disabledRotationDrive;
            joint.angularYZDrive = disabledRotationDrive;
            joint.slerpDrive = disabledRotationDrive;
        }

        [Serializable]
        public struct SpringSettings {
            [Min(0)] public float spring;
            [Min(0)] public float damping;
            [Min(0)] public float maxForce;

            public JointDrive Drive => new JointDrive {positionSpring = spring, positionDamper = damping, maximumForce = maxForce};

            public SpringSettings(float spring = 0, float damping = 0, float maxForce = 0) => (this.spring, this.damping, this.maxForce) = (spring, damping, maxForce);
            public SpringSettings(JointDrive drive) => (spring, damping, maxForce) = (drive.positionSpring, drive.positionDamper, drive.maximumForce);

            public static implicit operator JointDrive(SpringSettings springSettings) => springSettings.Drive;
            public static implicit operator SpringSettings(JointDrive jointDrive) => new SpringSettings(jointDrive);
        }
    }
}