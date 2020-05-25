using UnityEngine;

namespace TheUnusuals.Banisher.BanisherPlayground.Scripts.PhysicsAndOther {
    public class TransformBlender : MonoBehaviour {
        [SerializeField] private Transform target;

        [SerializeField] private Transform transform1;
        [SerializeField] private Transform transform2;

        [SerializeField] private bool blendPosition = true;
        [SerializeField] [Range(0, 1)] private float positionBlend = 0;
        [SerializeField] private bool useLocalPosition = false;

        [SerializeField] private bool blendRotation = true;
        [SerializeField] [Range(0, 1)] private float rotationBlend = 0;
        [SerializeField] private bool useLocalRotation = false;

        private void Awake() {
            if (!target) target = transform;
        }

        private void Update() {
            if (blendPosition) {
                if (useLocalPosition) {
                    target.localPosition = Vector3.LerpUnclamped(transform1.localPosition, transform2.localPosition, positionBlend);
                } else {
                    target.position = Vector3.LerpUnclamped(transform1.position, transform2.position, positionBlend);
                }
            }

            if (blendRotation) {
                if (useLocalRotation) {
                    target.localRotation = Quaternion.LerpUnclamped(transform1.localRotation, transform2.localRotation, rotationBlend);
                } else {
                    target.rotation = Quaternion.LerpUnclamped(transform1.rotation, transform2.rotation, rotationBlend);
                }
            }
        }
    }
}