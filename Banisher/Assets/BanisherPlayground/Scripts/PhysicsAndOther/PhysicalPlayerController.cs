using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class PhysicalPlayerController : MonoBehaviour {
        [SerializeField] private Transform playArea;
        [SerializeField] private Transform playerHead;

        [SerializeField] private Transform playerFeet;

        [SerializeField] private CharacterController characterController;
        [SerializeField] private CapsuleCollider characterControllerCollider;
        [SerializeField] private float characterControllerColliderOffsetFromGround = 0.5f;

        [SerializeField] private List<Collider> ignoredColliders = new List<Collider>();

        [SerializeField] private SteamVR_Action_Vector2 playerMovementAction;
        [SerializeField] private float maxSpeed = 2;

        private Vector3 gravityVelocity;

        private void Start() {
            foreach (Collider ignoredCollider in ignoredColliders) {
                Physics.IgnoreCollision(characterController, ignoredCollider);
                Physics.IgnoreCollision(characterControllerCollider, ignoredCollider);
            }
        }

        private void Update() {
            UpdateFeet();

            UpdateCharacterController();
            ApplyPlayerMoveAction(Time.deltaTime);

            if (!characterController.isGrounded) {
                ApplyGravity(Time.deltaTime);
            } else {
                gravityVelocity = Vector3.zero;
            }
        }

        private void LateUpdate() {
            SyncPlayerWithCharacterController();
        }

        private void ApplyGravity(float deltaTime) {
            gravityVelocity += Physics.gravity * deltaTime;
            Vector3 moveDelta = gravityVelocity * deltaTime;
            characterController.Move(moveDelta);
        }

        private Vector2 GetMovementAxis() {
            Vector2 movementAxis = playerMovementAction.axis;

            if (movementAxis != Vector2.zero) {
                return movementAxis;
            }

            if (Input.GetKey(KeyCode.Keypad8)) {
                movementAxis.y = 1;
            } else if (Input.GetKey(KeyCode.Keypad5)) {
                movementAxis.y = -1;
            }

            if (Input.GetKey(KeyCode.Keypad6)) {
                movementAxis.x = 1;
            } else if (Input.GetKey(KeyCode.Keypad4)) {
                movementAxis.x = -1;
            }

            return movementAxis.normalized;
        }

        private void ApplyPlayerMoveAction(float deltaTime) {
            Vector2 movementAxis = GetMovementAxis();

            if (movementAxis != Vector2.zero) {
                movementAxis *= maxSpeed * deltaTime;
                movementAxis = Quaternion.Euler(0, 0, -playerFeet.rotation.eulerAngles.y) * movementAxis;

                characterController.Move(new Vector3(movementAxis.x, 0, movementAxis.y));
            }
        }

        private void UpdateFeet() {
            Vector3 playerHeadPosition = playerHead.position;
            playerFeet.position = new Vector3(playerHeadPosition.x, playArea.position.y, playerHeadPosition.z);
            playerFeet.rotation = Quaternion.Euler(0, playerHead.rotation.eulerAngles.y, 0);
        }

        private void UpdateCharacterController() {
            float headHeight = playerHead.position.y - playArea.position.y;
            characterController.height = headHeight;
            characterController.center = new Vector3(0, headHeight / 2, 0);

            characterControllerCollider.height = characterController.height - characterControllerColliderOffsetFromGround;
            characterControllerCollider.center = characterController.center + Vector3.up * (characterControllerColliderOffsetFromGround / 2);

            Vector3 moveDelta = playerFeet.position - characterController.transform.position;
            characterController.Move(moveDelta);
        }

        private void SyncPlayerWithCharacterController() {
            Vector3 moveDelta = playerFeet.position - characterController.transform.position;

            playArea.position -= moveDelta;
            playerFeet.position = characterController.transform.position;
        }
    }
}