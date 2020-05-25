using UnityEngine;

public class BouncyBall : MonoBehaviour {
    private new Rigidbody rigidbody;

    private void Awake() {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other) {
        rigidbody.AddForce(0, 5, 0, ForceMode.Impulse);
    }
}
