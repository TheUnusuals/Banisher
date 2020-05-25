using System;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class AttachedRigidbodies : MonoBehaviour {
        [SerializeField] private float massScale = 1f;
        [SerializeField] private float attachedMassScale = 1f;

        [SerializeField] private float inertiaTensorScale = 1f;
        [SerializeField] private float attachedInertiaTensorScale = 1f;
        [SerializeField] private bool ignoreAttachedMassForInertiaTensor = false;

        [SerializeField] private List<Rigidbody> directlyAttachedRigidbodies = new List<Rigidbody>();
        [SerializeField] private bool followAttachedRigidbodies = true;

        [SerializeField] private List<Joint> directlyAttachedJoints = new List<Joint>();
        [SerializeField] private bool scanForJoints = true;

        [SerializeField] private new Rigidbody rigidbody;

        private IReadOnlyList<Rigidbody> rigidbodies = Array.Empty<Rigidbody>();
        private float combinedMass;
        private Vector3 combinedInertiaTensor;

        private int lastCombinedMassFrame = -1;
        private int lastCombinedInertiaTensorFrame = -1;

        private readonly Dictionary<HashSet<Rigidbody>, IReadOnlyList<Rigidbody>> rigidbodyCache = new Dictionary<HashSet<Rigidbody>, IReadOnlyList<Rigidbody>>(HashSet<Rigidbody>.CreateSetComparer());

        public float CombinedMass {
            get {
                if (lastCombinedMassFrame != Time.frameCount) {
                    combinedMass = CalculateCombinedMassInternal(GetRigidbodies());
                    lastCombinedMassFrame = Time.frameCount;
                }

                return combinedMass;
            }
        }

        public Vector3 CombinedInertiaTensor {
            get {
                if (lastCombinedInertiaTensorFrame != Time.frameCount) {
                    combinedInertiaTensor = CalculateCombinedInertiaTensorInternal(GetRigidbodies());
                    lastCombinedInertiaTensorFrame = Time.frameCount;
                }

                return combinedInertiaTensor;
            }
        }

        private void Awake() {
            if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
        }

        public float CalculateCombinedMass(IEnumerable<Rigidbody> ignoredRigidbodies) {
            return CalculateCombinedMassInternal(GetRigidbodies(ignoredRigidbodies));
        }

        private float CalculateCombinedMassInternal(IEnumerable<Rigidbody> attachedRigidbodies) {
            return massScale * rigidbody.mass + attachedMassScale * attachedRigidbodies.Sum(attachedRigidbody => attachedRigidbody.mass);
        }

        public Vector3 CalculateCombinedInertiaTensor(IEnumerable<Rigidbody> ignoredRigidbodies) {
            return CalculateCombinedInertiaTensorInternal(GetRigidbodies(ignoredRigidbodies));
        }

        private Vector3 CalculateCombinedInertiaTensorInternal(IEnumerable<Rigidbody> attachedRigidbodies) {
            Vector3 worldCenterOfMass = rigidbody.worldCenterOfMass;
            Vector3 inertiaTensor = rigidbody.inertiaTensor;
            Quaternion inertiaTensorRotation = rigidbody.inertiaTensorRotation;
            Quaternion worldInertiaTensorRotation = rigidbody.rotation * inertiaTensorRotation;

            Vector3 attachedInertiaTensor = Vector3.zero;

            foreach (Rigidbody attachedRigidbody in attachedRigidbodies) {
                float attachedMass = ignoreAttachedMassForInertiaTensor ? 0 : attachedRigidbody.mass;
                Vector3 attachedWorldCenterOfMass = attachedRigidbody.worldCenterOfMass;

                Quaternion attachedToCombinedInertiaTensorRotation = attachedRigidbody.rotation * attachedRigidbody.inertiaTensorRotation;
                Vector3 rotatedAttachedInertiaTensor = attachedToCombinedInertiaTensorRotation * attachedRigidbody.inertiaTensor;

                float xDistance = MathUtils.DistanceBetweenPointAndLine(attachedWorldCenterOfMass, worldCenterOfMass, Vector3.right);
                float yDistance = MathUtils.DistanceBetweenPointAndLine(attachedWorldCenterOfMass, worldCenterOfMass, Vector3.up);
                float zDistance = MathUtils.DistanceBetweenPointAndLine(attachedWorldCenterOfMass, worldCenterOfMass, Vector3.forward);

                attachedInertiaTensor += new Vector3(
                    rotatedAttachedInertiaTensor.x + attachedMass * xDistance * xDistance,
                    rotatedAttachedInertiaTensor.y + attachedMass * yDistance * yDistance,
                    rotatedAttachedInertiaTensor.z + attachedMass * zDistance * zDistance
                );
            }

            return inertiaTensorScale * inertiaTensor + attachedInertiaTensorScale * (Quaternion.Inverse(worldInertiaTensorRotation) * attachedInertiaTensor);
        }

        public Vector3 CalculateCombinedJointForces() {
            return GetJoints().Aggregate(Vector3.zero, (combinedJointForce, joint) => {
                Rigidbody connectedBody = joint.connectedBody;
                Vector3 currentForce = joint.currentForce;
                return combinedJointForce + connectedBody.rotation * (connectedBody == rigidbody ? rigidbody.rotation * currentForce : -currentForce);
            });
        }

        public Vector3 CalculateCombinedJointTorques() {
            return GetJoints().Aggregate(Vector3.zero, (combinedJointTorque, joint) => {
                Rigidbody connectedBody = joint.connectedBody;
                Vector3 currentTorque = joint.currentTorque;
                return combinedJointTorque + connectedBody.rotation * (connectedBody == rigidbody ? rigidbody.rotation * currentTorque : -currentTorque);
            });
        }

        public IReadOnlyList<Rigidbody> GetRigidbodies() {
            return GetRigidbodies(Array.Empty<Rigidbody>());
        }

        public IReadOnlyList<Rigidbody> GetRigidbodies(IEnumerable<Rigidbody> ignoredRigidbodies) {
            HashSet<Rigidbody> ignored = new HashSet<Rigidbody>(ignoredRigidbodies.Append(rigidbody));

            if (rigidbodyCache.TryGetValue(ignored, out IReadOnlyList<Rigidbody> cachedRigidbodies)) {
                return cachedRigidbodies;
            }

            HashSet<Rigidbody> rigidbodies = new HashSet<Rigidbody>();

            foreach (Rigidbody attachedRigidbody in directlyAttachedRigidbodies) {
                if (!ignored.Contains(attachedRigidbody)) {
                    rigidbodies.Add(attachedRigidbody);

                    if (followAttachedRigidbodies) {
                        IEnumerable<AttachedRigidbodies> otherAttachedRigidbodiesComponents = attachedRigidbody
                            .GetComponents<AttachedRigidbodies>()
                            .Where(a => a.rigidbody == attachedRigidbody);

                        foreach (AttachedRigidbodies otherAttachedRigidbodies in otherAttachedRigidbodiesComponents) {
                            rigidbodies.UnionWith(otherAttachedRigidbodies.GetRigidbodies(ignored.Concat(rigidbodies)));
                        }
                    }
                }
            }

            return rigidbodyCache[ignored] = rigidbodies.ToList();
        }

        [Button]
        public void ClearRigidbodyCache() {
            rigidbodyCache.Clear();
        }

        public IEnumerable<Joint> GetJoints() {
            IEnumerable<Joint> joints = directlyAttachedJoints;

            if (scanForJoints) {
                return joints
                    .Concat(GetComponents<Joint>())
                    .Concat(directlyAttachedRigidbodies
                        .SelectMany(body => body.GetComponents<Joint>())
                        .Where(joint => joint.connectedBody == rigidbody)
                    );
            }

            return joints;
        }
    }
}