using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class RagdollBody : ARagdollBody {
        private enum PlayerBodyRaycast {
            Ray,
            Sphere
        }

        private enum PlayerFoot {
            None,
            Left,
            Right
        }

        [SerializeField] private Transform ragdollHips;

        [SerializeField] private float desiredHipHeight = 1f;
        [SerializeField] private float desiredFeetSeparation = 0.2f;
        [SerializeField] private float footPlacementThreshold = 0.05f;
        [SerializeField] private float pullingLegUpMaxTime = 0.5f;
        [SerializeField] private float puttingLegDownMaxTime = 0.5f;

        [SerializeField] private Vector3 hipTargetRotationOffset = new Vector3(0, 0, 0);
        [SerializeField] private Transform hipPositionTarget;
        [SerializeField] private PhysicallyFollow hipPositionTargetFollow;

        [SerializeField] private Vector3 chestOffset = new Vector3(0, 0.1f, 0);
        [SerializeField] private Vector3 chestTargetRotationOffset = new Vector3(0, 0, 0);
        [SerializeField] private Transform chestPositionTarget;
        [SerializeField] private PhysicallyFollow chestPositionTargetFollow;

        [SerializeField] private Vector3 headOffset = new Vector3(0, 0.3f, 0);
        [SerializeField] private Vector3 headTargetRotationOffset = new Vector3(0, 0, 0);
        [SerializeField] private Transform headPositionTarget;
        [SerializeField] private PhysicallyFollow headPositionTargetFollow;

        [SerializeField] private Vector3 kneeOffset = new Vector3(0, 0.5f, 0.1f);

        [SerializeField] private Transform leftFootPositionTarget;
        [SerializeField] private PhysicallyFollow leftFootPositionTargetFollow;

        [SerializeField] private Vector3 leftKneeRotationOffset = new Vector3(0, 0, 0);
        [SerializeField] private Vector3 leftKneeUpRotationOffset = new Vector3(0, 0, 0);
        [SerializeField] private Transform leftKneePositionTarget;
        [SerializeField] private PhysicallyFollow leftKneePositionTargetFollow;

        [SerializeField] private Transform rightFootPositionTarget;
        [SerializeField] private PhysicallyFollow rightFootPositionTargetFollow;

        [SerializeField] private Vector3 rightKneeRotationOffset = new Vector3(0, 0, 0);
        [SerializeField] private Vector3 rightKneeUpRotationOffset = new Vector3(0, 0, 0);
        [SerializeField] private Transform rightKneePositionTarget;
        [SerializeField] private PhysicallyFollow rightKneePositionTargetFollow;

        [SerializeField] private PlayerFoot currentActiveFoot = PlayerFoot.None;
        [SerializeField] private PlayerFoot previousActiveFoot = PlayerFoot.None;
        [SerializeField] private bool pullingLegUp = false;
        [SerializeField] private float legMovementTimeLeft = 0f;

        [SerializeField] private float raycastLength = 1f;
        [SerializeField] private PlayerBodyRaycast raycastShape = PlayerBodyRaycast.Sphere;
        [SerializeField] private float raycastRadius = 0.2f;

        [SerializeField] private List<Collider> colliders = new List<Collider>();
        [SerializeField] private List<RagdollCollider> ragdollColliders = new List<RagdollCollider>();

        private bool IsTouching => ragdollColliders.Any(ragdollCollider => ragdollCollider.IsTouching);

        private int[] ragdollCollidersLayers = new int[0];

        private void Start() {
            foreach (Collider collider in colliders) {
                RagdollCollider ragdollCollider = collider.gameObject.GetOrAddComponent<RagdollCollider>();

                if (!ragdollCollider.Body) {
                    ragdollCollider.Body = this;
                    ragdollColliders.Add(ragdollCollider);
                }
            }
        }

        private void FixedUpdate() {
            Vector3? groundPoint = GetTouchingGroundPoint();

            if (groundPoint == null) groundPoint = ragdollHips.position + Vector3.down * desiredHipHeight;

            /*if (groundPoint == null || !IsTouching) {
            hipPositionTargetFollow.enabled = false;
            chestPositionTargetFollow.enabled = false;
            headPositionTargetFollow.enabled = false;
            leftFootPositionTargetFollow.enabled = false;
            rightFootPositionTargetFollow.enabled = false;

            currentActiveFoot = PlayerFoot.None;
        } else {*/
            Vector3 up = (ragdollHips.position - groundPoint.Value).normalized;
            Vector3 forward = Vector3.ProjectOnPlane(ragdollHips.forward, up).normalized;
            Vector3 right = Vector3.Cross(up, forward);
            Quaternion forwardRotation = Quaternion.LookRotation(forward, up);

            hipPositionTarget.position = groundPoint.Value + up * desiredHipHeight;
            chestPositionTarget.position = hipPositionTarget.position + forwardRotation * chestOffset;
            headPositionTarget.position = chestPositionTarget.position + forwardRotation * headOffset;

            hipPositionTarget.rotation = forwardRotation * Quaternion.Euler(hipTargetRotationOffset);
            chestPositionTarget.rotation = forwardRotation * Quaternion.Euler(chestTargetRotationOffset);
            headPositionTarget.rotation = forwardRotation * Quaternion.Euler(headTargetRotationOffset);

            leftFootPositionTarget.position = groundPoint.Value - right * (desiredFeetSeparation / 2);
            leftKneePositionTarget.position = leftFootPositionTarget.position + forwardRotation * new Vector3(-kneeOffset.x, kneeOffset.y, kneeOffset.z);
            rightFootPositionTarget.position = groundPoint.Value + right * (desiredFeetSeparation / 2);
            rightKneePositionTarget.position = rightFootPositionTarget.position + forwardRotation * new Vector3(kneeOffset.x, kneeOffset.y, kneeOffset.z);

            leftFootPositionTarget.rotation = forwardRotation;
            leftKneePositionTarget.rotation = forwardRotation;
            rightFootPositionTarget.rotation = forwardRotation;
            rightKneePositionTarget.rotation = forwardRotation;

            hipPositionTargetFollow.enabled = true;
            chestPositionTargetFollow.enabled = true;
            headPositionTargetFollow.enabled = true;

            DoLegAdjustment(Time.fixedDeltaTime);
            //}
        }

        private void DoLegAdjustment(float deltaTime) {
            if (currentActiveFoot == PlayerFoot.None) {
                bool isLeftFootOffset = (leftFootPositionTargetFollow.TransformOffset.position - leftFootPositionTarget.position).magnitude > footPlacementThreshold;
                bool isRightFootOffset = (rightFootPositionTargetFollow.TransformOffset.position - rightFootPositionTarget.position).magnitude > footPlacementThreshold;

                if (isLeftFootOffset && (!isRightFootOffset || previousActiveFoot != PlayerFoot.Left)) {
                    currentActiveFoot = PlayerFoot.Left;
                    legMovementTimeLeft = pullingLegUpMaxTime;
                    pullingLegUp = true;
                } else if (isRightFootOffset && (!isLeftFootOffset || previousActiveFoot != PlayerFoot.Right)) {
                    currentActiveFoot = PlayerFoot.Right;
                    legMovementTimeLeft = pullingLegUpMaxTime;
                    pullingLegUp = true;
                }
            }

            if (currentActiveFoot != PlayerFoot.Left) {
                leftKneePositionTarget.rotation *= Quaternion.Euler(leftKneeRotationOffset);
            }

            if (currentActiveFoot != PlayerFoot.Right) {
                rightKneePositionTarget.rotation *= Quaternion.Euler(rightKneeRotationOffset);
            }

            if (currentActiveFoot == PlayerFoot.None) return;

            Transform footTarget, kneeTarget;
            PhysicallyFollow footFollow, kneeFollow;
            Vector3 kneeRotationOffset, kneeUpRotationOffset;

            if (currentActiveFoot == PlayerFoot.Left) {
                footTarget = leftFootPositionTarget;
                kneeTarget = leftKneePositionTarget;
                footFollow = leftFootPositionTargetFollow;
                kneeFollow = leftKneePositionTargetFollow;
                kneeRotationOffset = leftKneeRotationOffset;
                kneeUpRotationOffset = leftKneeUpRotationOffset;
            } else {
                footTarget = rightFootPositionTarget;
                kneeTarget = rightKneePositionTarget;
                footFollow = rightFootPositionTargetFollow;
                kneeFollow = rightKneePositionTargetFollow;
                kneeRotationOffset = rightKneeRotationOffset;
                kneeUpRotationOffset = rightKneeUpRotationOffset;
            }

            if (pullingLegUp) {
                kneeTarget.rotation *= Quaternion.Euler(kneeUpRotationOffset);
                footFollow.enabled = false;
                //kneeFollow.enabled = true;
            } else {
                kneeTarget.rotation *= Quaternion.Euler(kneeRotationOffset);
                footFollow.enabled = true;
                //kneeFollow.enabled = false;
            }

            legMovementTimeLeft -= deltaTime;

            if (legMovementTimeLeft <= 0) {
                if (pullingLegUp) {
                    pullingLegUp = false;
                    legMovementTimeLeft = puttingLegDownMaxTime;
                } else {
                    previousActiveFoot = currentActiveFoot;
                    currentActiveFoot = PlayerFoot.None;
                    footFollow.enabled = false;
                }
            }
        }

        private Vector3? GetTouchingGroundPoint() {
            Vector3? hitPoint = null;

            PrepareCollidersForRaycasting();

            if (raycastShape == PlayerBodyRaycast.Ray) {
                if (Physics.Raycast(ragdollHips.position, Vector3.down, out RaycastHit hit, desiredHipHeight)) {
                    hitPoint = hit.point;
                }
            } else if (raycastShape == PlayerBodyRaycast.Sphere) {
                if (Physics.SphereCast(ragdollHips.position + Vector3.up * raycastRadius, raycastRadius, Vector3.down, out RaycastHit hit, raycastLength)) {
                    hitPoint = ragdollHips.position + Vector3.down * Mathf.Min(hit.distance + raycastRadius, desiredHipHeight);
                }
            }

            RestoreCollidersAfterRaycasting();

            return hitPoint;
        }

        private void PrepareCollidersForRaycasting() {
            if (ragdollCollidersLayers.Length != colliders.Count) ragdollCollidersLayers = new int[colliders.Count];

            for (int i = 0; i < colliders.Count; i++) {
                ragdollCollidersLayers[i] = colliders[i].gameObject.layer;
                colliders[i].gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }

        private void RestoreCollidersAfterRaycasting() {
            for (int i = 0; i < colliders.Count; i++) {
                colliders[i].gameObject.layer = ragdollCollidersLayers[i];
            }
        }
    }
}