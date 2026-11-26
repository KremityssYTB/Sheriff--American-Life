using UnityEngine;

namespace SheriffAmericanLife.Legacy
{
    // Retained for compatibility with the original keyboard-driven greybox bootstrap.
    public class PatrolVehicleController : MonoBehaviour
    {
        [SerializeField] private float acceleration = 12f;
        [SerializeField] private float turnSpeed = 75f;
        [SerializeField] private float maxSpeed = 18f;
        private float speed;

        private void Update()
        {
            if (!Input.GetKey(KeyCode.E)) return;
            float throttle = Input.GetAxis("Vertical");
            float steering = Input.GetAxis("Horizontal");
            speed = Mathf.MoveTowards(speed, throttle * maxSpeed, acceleration * Time.deltaTime);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            transform.Rotate(Vector3.up, steering * turnSpeed * Mathf.Clamp01(Mathf.Abs(speed) / 5f) * Time.deltaTime);
        }
    }
}