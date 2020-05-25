using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class RagdollBody2 : ARagdollBody {
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

        private void Update() {
            if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
                Debug.Break();
            }
        }

        private void FixedUpdate() {
            Vector3? groundPoint = GetTouchingGroundPoint();

            if (groundPoint == null) groundPoint = ragdollHips.position + Vector3.down * desiredHipHeight;

            if (groundPoint == null || !IsTouching) {
                hipPositionTargetFollow.enabled = false;
                chestPositionTargetFollow.enabled = false;
                headPositionTargetFollow.enabled = false;
            } else {
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

                hipPositionTargetFollow.enabled = true;
                chestPositionTargetFollow.enabled = true;
                headPositionTargetFollow.enabled = true;
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