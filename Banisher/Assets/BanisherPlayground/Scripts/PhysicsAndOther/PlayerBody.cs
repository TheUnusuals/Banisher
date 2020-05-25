using System.Collections.Generic;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class PlayerBody : MonoBehaviour {
        private enum PlayerBodyRaycast {
            Ray,
            Sphere
        }

        [SerializeField] private Rigidbody playArea;
        [SerializeField] private Transform playerHead;

        [SerializeField] private Transform groundTrackingTransform;
        [SerializeField] private Vector3 groundTrackingTransformOffset;

        [SerializeField] private List<Collider> ignoredColliders = new List<Collider>();

        [SerializeField] private PlayerBodyRaycast raycastShape = PlayerBodyRaycast.Sphere;
        [SerializeField] private float raycastRadius = 0.2f;

        private Vector3 PlayerGround => new Vector3(playerHead.position.x, playArea.position.y, playerHead.position.z);

        private void FixedUpdate() {
            if (groundTrackingTransform) {
                groundTrackingTransform.position = PlayerGround + groundTrackingTransformOffset;
                groundTrackingTransform.rotation = Quaternion.Euler(0, playerHead.rotation.eulerAngles.y, 0);
            }

            /*Vector3? groundPoint = GetTouchingGroundPoint();

        if (groundPoint == null) {
            playArea.isKinematic = false;
        } else {
            playArea.isKinematic = true;
            Vector3 playAreaOffset = playArea.position - PlayerGround;
            playArea.position = groundPoint.Value + playAreaOffset;
        }*/
        }

        private Vector3? GetTouchingGroundPoint() {
            int[] ignoredCollidersLayers = new int[ignoredColliders.Count];

            for (int i = 0; i < ignoredColliders.Count; i++) {
                Collider ignoredCollider = ignoredColliders[i];
                ignoredCollidersLayers[i] = ignoredCollider.gameObject.layer;
                ignoredCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }

            Vector3? hitPoint = null;

            if (raycastShape == PlayerBodyRaycast.Ray) {
                if (RaycastRay(out RaycastHit hit)) {
                    hitPoint = hit.point;
                }
            } else if (raycastShape == PlayerBodyRaycast.Sphere) {
                if (RaycastSphere(out RaycastHit hit)) {
                    hitPoint = playerHead.position + Vector3.down * (hit.distance + raycastRadius);
                }
            }

            for (int i = 0; i < ignoredColliders.Count; i++) {
                Collider ignoredCollider = ignoredColliders[i];
                ignoredCollider.gameObject.layer = ignoredCollidersLayers[i];
            }

            return hitPoint;
        }

        private bool RaycastRay(out RaycastHit hit) {
            return Physics.Raycast(playerHead.position, Vector3.down, out hit, (playerHead.position - PlayerGround).magnitude);
        }

        private bool RaycastSphere(out RaycastHit hit) {
            return Physics.SphereCast(playerHead.position + Vector3.up * raycastRadius, raycastRadius, Vector3.down, out hit, (playerHead.position - PlayerGround).magnitude);
        }
    }
}