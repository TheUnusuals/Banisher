using System;
using System.Collections.Generic;
using System.Linq;
using TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy;
using TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther.PID;
using TheUnusuals.Banisher.BanisherPlayground.Scripts.Sight;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class ActiveRagdoll : MonoBehaviour {
        private enum RaycastShape {
            Ray,
            Sphere
        }

        private enum ActiveRagdollState {
            Idle,
            Lost,
            Catching
        }

        private const string AnimatorStateParameter = "State";
        private const string AnimatorIdleState = "Idle";
        private const string AnimatorRunState = "Run";
        private const string AnimatorPunchState = "Punch";

        [SerializeField] private Animator animator;

        [SerializeField] private PositionTargetInfo rootTarget;

        [SerializeField] private PositionPIDController movementPidController;

        [SerializeField] private Transform rotationTarget;
        [SerializeField] private RotationPIDController rotationPidController;

        [SerializeField] private Transform sightRaycastRoot;
        [SerializeField] private Seeable currentSightedObject;
        [SerializeField] private float maxSightDistance = 20;
        [SerializeField] private float stopDistance = 0.1f;
        [SerializeField] private float maxSightAngle = 45;
        [SerializeField] private Transform lastSightedTarget;

        [SerializeField] private float minVisEnergyRequired = 1;

        [SerializeField] private bool activeRagdollEnabled = true;

        [SerializeField] private Transform groundTarget;

        [SerializeField] private Transform raycastOrigin;
        [SerializeField] private float raycastAdditionalLength = 0.2f;
        [SerializeField] private RaycastShape raycastShape = RaycastShape.Sphere;
        [SerializeField] private float raycastRadius = 0.2f;

        [SerializeField] private List<Collider> colliders = new List<Collider>();

        [SerializeField] private List<ConfigurableJointHelper> jointHelpers = new List<ConfigurableJointHelper>();
        [SerializeField] private List<Behaviour> activatedComponentsInActiveRagdollMode = new List<Behaviour>();

        [SerializeField] private VisEnergy visEnergy;

        [SerializeField] private SeeableManager seeableManager;

        private readonly List<RagdollColliderInfo> ragdollColliderInfos = new List<RagdollColliderInfo>();

        private static readonly int StateParameter = Animator.StringToHash(AnimatorStateParameter);

        private bool IsTouching => ragdollColliderInfos.Any(ragdollColliderInfo => ragdollColliderInfo?.ragdollCollider.IsTouching ?? false);

        private Transform Destination {
            set => movementPidController.Target = value;
            get => movementPidController.Target;
        }

        public VisEnergy VisEnergy
        {
            get => visEnergy;
            set => visEnergy = value;
        }

        public bool IsAlive => visEnergy.Energy > minVisEnergyRequired;

        private void Awake() {
            if (!visEnergy) visEnergy = GetComponent<VisEnergy>();
            if (!seeableManager) seeableManager = SeeableManager.GetInstance();
        }

        private void Start() {
            foreach (Collider collider in colliders) {
                ragdollColliderInfos.Add(new RagdollColliderInfo {collider = collider, gameObject = collider.gameObject});
            }

            foreach (RagdollColliderInfo ragdollColliderInfo in ragdollColliderInfos) {
                if (!ragdollColliderInfo.gameObject)
                    ragdollColliderInfo.gameObject = ragdollColliderInfo.collider.gameObject;

                if (!ragdollColliderInfo.ragdollCollider) {
                    RagdollCollider ragdollCollider =
                        ragdollColliderInfo.gameObject.GetOrAddComponent<RagdollCollider>();

                    if (!ragdollCollider.Body || ragdollCollider.Body == this) {
                        ragdollCollider.Body = this;
                        ragdollColliderInfo.ragdollCollider = ragdollCollider;
                    }
                }
            }

            visEnergy.OnChange.AddListener(OnChangeVisEnergy);

            if (activeRagdollEnabled) {
                EnableActiveRagdollMode();
            } else {
                EnableRagdollOnlyMode();
            }
        }

        private void FixedUpdate() {
            if (activeRagdollEnabled) {
                UpdateTargets();
                UpdateSight();
            }
        }

        private void UpdateTargets() {
            Vector3 rootOffset = rootTarget.ProjectedOffset;
            Vector3? groundPoint = GetTouchingGroundPoint(rootOffset.magnitude, Vector3.down, Vector3.up);

            if (groundPoint.HasValue && IsTouching) {
                groundTarget.position = groundPoint.Value;

                rootTarget.target.gameObject.SetActive(true);
                rootTarget.target.position = groundTarget.position + rootOffset;

                if (Destination) {
                    Vector3 posDiff = Destination.position - rootTarget.target.position;

                    rotationTarget.rotation = Quaternion.LookRotation(posDiff, Vector3.up);
                    rotationPidController.enabled = true;
                } else {
                    rotationTarget.rotation = Quaternion.identity;
                    rotationPidController.enabled = false;
                }
            } else {
                rootTarget.target.gameObject.SetActive(false);
            }
        }

        private void UpdateSight() {
            if (currentSightedObject) {
                if (CheckIfCollidersAreInSight(currentSightedObject.SeeableColliders)) {
                    lastSightedTarget.position = currentSightedObject.SeeableTransform.position;

                    float distanceToSightedPosition = GetSightedDistance(currentSightedObject.SeeableTransform.position);

                    if (distanceToSightedPosition <= stopDistance) {
                        SetAnimatorState(AnimatorPunchState);
                    } else {
                        Destination = currentSightedObject.SeeableTransform;
                        SetAnimatorState(AnimatorRunState);
                    }
                } else {
                    currentSightedObject = null;
                    Destination = lastSightedTarget;
                    SetAnimatorState(AnimatorRunState);
                }
            } else {
                float currentDistanceToSeeable = float.PositiveInfinity;

                foreach (Seeable seeable in seeableManager.CurrentSeeableObjects) {
                    if (CheckIfCollidersAreInSight(seeable.SeeableColliders)) {
                        float seeableDistance = Vector3.Distance(sightRaycastRoot.position, seeable.SeeableTransform.position);

                        if (seeableDistance < currentDistanceToSeeable) {
                            currentSightedObject = seeable;
                            currentDistanceToSeeable = seeableDistance;
                        }
                    }
                }

                if (currentSightedObject) {
                    Destination = currentSightedObject.SeeableTransform;
                    lastSightedTarget.position = currentSightedObject.SeeableTransform.position;
                    SetAnimatorState(AnimatorRunState);
                } else {
                    float distanceToLastSightedPosition = GetSightedDistance(lastSightedTarget.position);

                    if (distanceToLastSightedPosition <= stopDistance) {
                        Destination = null;
                        SetAnimatorState(AnimatorIdleState);
                    }
                }
            }
        }

        private float GetSightedDistance(Vector3 sightedPosition) {
            float distanceToLastSightedPosition = Vector2.Distance(
                new Vector2(sightRaycastRoot.position.x, sightRaycastRoot.position.z),
                new Vector2(sightedPosition.x, sightedPosition.z)
            );

            return distanceToLastSightedPosition;
        }

        private bool CheckIfCollidersAreInSight(IEnumerable<Collider> seeableColliders) {
            foreach (Collider seeableCollider in seeableColliders) {
                RaycastHit? hit = CheckSight(seeableCollider);

                if (hit.HasValue) {
                    return true;
                }
            }

            return false;
        }

        private RaycastHit? CheckSight(Collider seeableCollider) {
            RaycastHit? hitPoint = null;

            Vector3 colliderOffset = seeableCollider.bounds.center - sightRaycastRoot.position;
            Vector3 colliderDirection = colliderOffset.normalized;

            float angleToCollider = Vector3.Angle(sightRaycastRoot.forward, colliderDirection);

            if (angleToCollider > maxSightAngle) {
                return null;
            }

            if (Physics.Raycast(sightRaycastRoot.position, colliderDirection, out RaycastHit hit, maxSightDistance)) {
                Debug.DrawRay(sightRaycastRoot.position, colliderDirection.normalized * maxSightDistance, Color.red);
                if (hit.collider == seeableCollider) {
                    hitPoint = hit;
                }
            }

            return hitPoint;
        }

        public void EnableRagdollOnlyMode() {
            activeRagdollEnabled = false;

            movementPidController.enabled = false;
            rootTarget.target.gameObject.SetActive(false);
            rotationTarget.gameObject.SetActive(false);

            foreach (ConfigurableJointHelper jointHelper in jointHelpers) {
                jointHelper.DrivesEnabled = false;
            }

            foreach (Behaviour component in activatedComponentsInActiveRagdollMode) {
                component.enabled = false;
            }
        }

        private void SetAnimatorState(string state) {
            if (state == AnimatorIdleState) {
                animator.SetInteger(StateParameter, 0);
            } else if (state == AnimatorRunState) {
                animator.SetInteger(StateParameter, 1);
            } else if (state == AnimatorPunchState) {
                animator.SetInteger(StateParameter, 2);
            }
        }

        public void EnableActiveRagdollMode() {
            activeRagdollEnabled = true;

            movementPidController.enabled = true;
            rootTarget.target.gameObject.SetActive(true);
            rotationTarget.gameObject.SetActive(true);

            foreach (ConfigurableJointHelper jointHelper in jointHelpers) {
                jointHelper.DrivesEnabled = true;
            }

            foreach (Behaviour component in activatedComponentsInActiveRagdollMode) {
                component.enabled = true;
            }
        }

        private void OnChangeVisEnergy(VisEnergy visEnergy, float previousEnergy) {
            if (visEnergy.Energy <= minVisEnergyRequired) {
                EnableRagdollOnlyMode();
            } else {
                EnableActiveRagdollMode();
            }
        }

        private Vector3? GetTouchingGroundPoint(float rootHeight, Vector3 down, Vector3 up) {
            Vector3? hitPoint = null;

            PrepareCollidersForRaycasting();

            float maxRaycastLength = rootHeight + raycastAdditionalLength;

            if (raycastShape == RaycastShape.Ray) {
                if (Physics.Raycast(raycastOrigin.position, down, out RaycastHit hit, maxRaycastLength)) {
                    hitPoint = raycastOrigin.position + down * Mathf.Min(hit.distance, rootHeight);
                }
            } else if (raycastShape == RaycastShape.Sphere) {
                if (Physics.SphereCast(raycastOrigin.position + up * raycastRadius, raycastRadius, down,
                    out RaycastHit hit, maxRaycastLength)) {
                    hitPoint = raycastOrigin.position + down * Mathf.Min(hit.distance, rootHeight);
                }
            }

            RestoreCollidersAfterRaycasting();

            return hitPoint;
        }

        private void PrepareCollidersForRaycasting() {
            foreach (RagdollColliderInfo ragdollColliderInfo in ragdollColliderInfos) {
                ragdollColliderInfo.layer = ragdollColliderInfo.gameObject.layer;
                ragdollColliderInfo.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }

        private void RestoreCollidersAfterRaycasting() {
            foreach (RagdollColliderInfo ragdollColliderInfo in ragdollColliderInfos) {
                ragdollColliderInfo.gameObject.layer = ragdollColliderInfo.layer;
            }
        }

        [Serializable]
        public class PositionTargetInfo {
            public Transform target;
            public Transform targetTransform;
            public Transform targetTransformGround;

            public Vector3 ProjectedOffset => Vector3.Project(targetTransformGround.InverseTransformPoint(targetTransform.position), targetTransformGround.up);
            public Quaternion OffsetRotation => Quaternion.Inverse(targetTransformGround.rotation) * targetTransform.rotation;
        }

        [Serializable]
        public class RagdollColliderInfo {
            public Collider collider;
            public RagdollCollider ragdollCollider;
            public GameObject gameObject;
            public int layer;
        }
    }
}